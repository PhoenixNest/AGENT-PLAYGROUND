using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace OrbitalSalvage.Render
{
    public sealed partial class Canvas2D
    {
        public const int ParamStride = 28;
        const int IdxRect = 0, IdxFlags = 1, IdxAlpha = 2, IdxColor = 3, IdxShadow = 4, IdxGx0 = 5, IdxGx1 = 6, IdxGa = 7, IdxGb = 8,
            IdxGcol = 9, IdxGoff = 17, IdxEdge = 19, IdxMaskRect = 20, IdxBlur = 21, IdxImgA = 22, IdxImgB = 23, IdxImgC = 24, IdxImgD = 25, IdxText = 26;

        const int AccPass = 0, AccClearPass = 1;
        const int FillSourceOver = 0, FillClear = 1;
        const int MaskCoverage = 0, MaskBlurH = 1, MaskBlurV = 2, MaskClip = 3, MaskConst = 4;

        static readonly int IdParamBase = Shader.PropertyToID("_C2D_ParamBase");
        static readonly int IdParams = Shader.PropertyToID("_C2D_Params");
        static readonly int IdEdges = Shader.PropertyToID("_C2D_Edges");
        static readonly int IdTarget = Shader.PropertyToID("_C2D_Target");
        static readonly int IdFlipY = Shader.PropertyToID("_C2D_FlipY");
        static readonly int IdAccum = Shader.PropertyToID("_C2D_Accum");
        static readonly int IdMask = Shader.PropertyToID("_C2D_Mask");
        static readonly int IdClipTex = Shader.PropertyToID("_C2D_ClipTex");
        static readonly int IdLayerTex = Shader.PropertyToID("_C2D_Layer");
        static readonly float[] gradOff = new float[8];

        readonly RenderTexture target;
        readonly RenderTexture accum;
        readonly RenderTexture maskA, maskB;
        readonly List<RenderTexture> clipPool = new List<RenderTexture>();
        readonly List<Texture2D> snapshots = new List<Texture2D>();
        int clipUsed;
        readonly CommandBuffer cmd = new CommandBuffer { name = "Canvas2D" };
        GraphicsBuffer edgeBuffer, paramBuffer;
        Vector4[] edges = new Vector4[4096];
        int edgeCount;
        Vector4[] prm = new Vector4[ParamStride * 256];
        int prmCount;
        Material matAccum, matFill, matMask, matLayer;

        /// <summary>Frozen captures keep the CPU box blur. Live draws record a GPU box blur and do not read the frame back.</summary>
        public bool ExactBackdrop = true;

        /// <summary>Experiment (<c>noshadow</c>). Skips blurred shadows on every canvas. Zero-blur shadows still draw.</summary>
        public static bool SkipBlurredShadows;

        /// <summary>Experiment (<c>nogamefx</c>). <see cref="GameRenderer"/> copies this onto <see cref="SkipBlurredFx"/>.</summary>
        public static bool SkipGameFx;

        /// <summary>
        /// Experiment (<c>rt16</c>). Read when the canvas is constructed. When true, the shadow
        /// accum is <see cref="RenderTextureFormat.ARGBHalf"/> instead of <see cref="RenderTextureFormat.RFloat"/>.
        /// </summary>
        public static bool HalfAccum;

        /// <summary>Experiment. This canvas skips blurred shadows. Default false.</summary>
        public bool SkipBlurredFx;

        int pendingDraws;

        /// <summary>
        /// When true, <see cref="EndFrame"/> still records the frame on the CPU but returns before
        /// buffer uploads, <c>Shader.SetGlobal*</c>, <see cref="UploadText"/>, and
        /// <see cref="Graphics.ExecuteCommandBuffer"/>. The next <see cref="BeginFrame"/> clears the
        /// command buffer, edge and param counts, glyph records, and backdrop snapshots, as it does
        /// after a submitted frame. Edge, param, and glyph arrays keep a high-water capacity.
        /// <see cref="Flush"/> and <see cref="Replay"/> throw: both execute so a caller can read
        /// pixels, and a skipped submit has nothing to read. <see cref="GameRenderer.Render"/> only
        /// calls <see cref="BeginFrame"/> and <see cref="EndFrame"/> (no backdrop readback). Default false.
        /// </summary>
        public bool SkipSubmit;

        /// <summary>Times this canvas called <see cref="Graphics.ExecuteCommandBuffer"/>.</summary>
        public int ExecuteCount { get; private set; }

        public RenderTexture Target => target;
        public CommandBuffer Commands => cmd;
        public int EdgeCount => edgeCount;
        public int DrawCount { get; private set; }

        public Canvas2D(int width, int height)
        {
            Width = width;
            Height = height;
            target = MakeRT(RenderTextureFormat.ARGB32, "Canvas2D.Target");
            accum = MakeRT(HalfAccum ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.RFloat, "Canvas2D.Accum");
            maskA = MakeRT(RenderTextureFormat.RHalf, "Canvas2D.MaskA");
            maskB = MakeRT(RenderTextureFormat.RHalf, "Canvas2D.MaskB");
            matAccum = MakeMaterial("Hidden/Canvas2D/Accumulate");
            matFill = MakeMaterial("Hidden/Canvas2D/Fill");
            matMask = MakeMaterial("Hidden/Canvas2D/Mask");
            InitText();
        }

        RenderTexture MakeRT(RenderTextureFormat fmt, string name)
        {
            var rt = new RenderTexture(Width, Height, 0, fmt, RenderTextureReadWrite.Linear)
            {
                name = name, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, useMipMap = false, autoGenerateMips = false,
            };
            rt.Create();
            return rt;
        }

        static Material MakeMaterial(string shaderName)
        {
            var sh = Shader.Find(shaderName);
            if (sh == null)
            {
                // Player builds: the shader is only present through the material in Resources/Canvas2D (ParityAssetBuilder.BuildCanvasMaterials).
                var res = Resources.Load<Material>("Canvas2D/" + shaderName.Substring(shaderName.LastIndexOf('/') + 1));
                if (res != null) sh = res.shader;
            }
            if (sh == null) throw new InvalidOperationException("Shader not found: " + shaderName);
            return new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
        }

        public void Dispose()
        {
            DisposeText();
            edgeBuffer?.Dispose();
            paramBuffer?.Dispose();
            DestroyOwned(matLayer);
            DestroyOwned(matBlur);
            for (int i = 0; i < blurPairN; i++) ReleasePair(blurPairs[i]);
            blurPairN = 0;
            cmd.Release();
            foreach (var c in clipPool) c.Release();
            ReleaseSnapshots();
            target.Release(); accum.Release(); maskA.Release(); maskB.Release();
        }

        static void DestroyOwned(UnityEngine.Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(obj);
            else UnityEngine.Object.DestroyImmediate(obj);
        }

        // BackdropBlur snapshots are sampled by draws recorded after that call's Flush.
        // They must stay alive until the command buffer has executed (EndFrame or Flush).
        // The next BeginFrame is the first moment they are unused, so release them here
        // instead of letting the list grow for the life of the canvas.
        void ReleaseSnapshots()
        {
            for (int i = 0; i < snapshots.Count; i++)
            {
                var shot = snapshots[i];
                if (shot == null) continue;
                if (Application.isPlaying) UnityEngine.Object.Destroy(shot);
                else UnityEngine.Object.DestroyImmediate(shot);
            }
            snapshots.Clear();
        }

        public void BeginFrame()
        {
            pendingDraws = 0;
            ReleaseSnapshots();
            cmd.Clear();
            edgeCount = 0;
            prmCount = 0;
            DrawCount = 0;
            clipUsed = 0;
            state = new State();
            stack.Clear();
            path.Reset();
            BeginFrameText();
            cmd.SetRenderTarget(target);
            cmd.ClearRenderTarget(false, true, Color.clear);
        }

        public void EndFrame() { Submit(false); }

        /// <summary>Source-over a premultiplied texture onto the frame. Recorded in order with the other draws.</summary>
        public void BlitLayer(Texture tex)
        {
            if (tex == null || state.globalAlpha <= 0) return;
            if (matLayer == null)
            {
                var sh = Resources.Load<Shader>("Canvas2D/LayerBlit");
                if (sh == null) sh = Shader.Find("Hidden/Canvas2D/LayerBlit");
                if (sh == null) throw new InvalidOperationException("missing LayerBlit shader");
                matLayer = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            }
            PaintProfile.Begin(PaintProfile.Blit);
            int pb = NewParams();
            prm[pb + IdxRect] = new Vector4(0, 0, Width, Height);
            prm[pb + IdxAlpha] = new Vector4((float)state.globalAlpha, 0, 0, 0);
            cmd.SetRenderTarget(target);
            cmd.SetGlobalTexture(IdLayerTex, tex);
            Draw(matLayer, 0, 6, pb);
            RenderStats.Blits++;
            PaintProfile.End(PaintProfile.Blit);
        }

        /// <summary>Executes the draws so far and leaves <see cref="Target"/> intact for a readback.</summary>
        public void Flush() { Submit(true); }

        void Submit(bool keepDrawing)
        {
            if (SkipSubmit)
            {
                pendingDraws = 0;
                if (keepDrawing)
                    throw new InvalidOperationException("Canvas2D.SkipSubmit refuses Flush: it would execute the command buffer and read pixels back.");
                return;
            }
            PaintProfile.Begin(PaintProfile.Submit);
            PaintProfile.CountFlush();
            EnsureBuffer(ref edgeBuffer, edgeCount, 16);
            EnsureBuffer(ref paramBuffer, prmCount, 16);
            if (edgeCount > 0) edgeBuffer.SetData(edges, 0, 0, edgeCount);
            if (prmCount > 0) paramBuffer.SetData(prm, 0, 0, prmCount);
            Shader.SetGlobalBuffer(IdEdges, edgeBuffer);
            Shader.SetGlobalBuffer(IdParams, paramBuffer);
            Shader.SetGlobalVector(IdTarget, new Vector4(Width, Height, 1f / Width, 1f / Height));
            Shader.SetGlobalFloat(IdFlipY, 1f);
            Shader.SetGlobalTexture(IdAccum, accum);
            UploadText();
            ExecuteRecorded();
            PaintProfile.End(PaintProfile.Submit);
            if (!keepDrawing) return;
            cmd.Clear();
            edgeCount = 0;
            prmCount = 0;
            cmd.SetRenderTarget(target);
        }

        /// <summary>
        /// CSS backdrop-filter: blur(sigmaCssPx) of what has already been drawn, shown inside the user-space rect.
        /// Sigma is in user px; the current transform maps it to device pixels.
        /// </summary>
        public void BackdropBlur(double x, double y, double w, double h, double sigmaUser)
        {
            PaintProfile.Begin(PaintProfile.Backdrop);
            if (sigmaUser <= 0 || w == 0 || h == 0 || state.globalAlpha <= 0) { PaintProfile.End(PaintProfile.Backdrop); return; }
            var origin = state.ctm.Apply(0, 0);
            var unit = state.ctm.Apply(sigmaUser, 0);
            double sigmaDev = Math.Abs(unit.x - origin.x);
            if (sigmaDev < 0.5) { PaintProfile.End(PaintProfile.Backdrop); return; }
            var p0 = state.ctm.Apply(x, y);
            var p1 = state.ctm.Apply(x + w, y + h);
            int x0 = Clamp((int)Math.Floor(Math.Min(p0.x, p1.x)), 0, Width);
            int x1 = Clamp((int)Math.Ceiling(Math.Max(p0.x, p1.x)), 0, Width);
            int top = (int)Math.Floor(Math.Min(p0.y, p1.y));
            int bot = (int)Math.Ceiling(Math.Max(p0.y, p1.y));
            int y0 = Clamp(Height - bot, 0, Height);
            int y1 = Clamp(Height - top, 0, Height);
            if (x1 <= x0 || y1 <= y0) { PaintProfile.End(PaintProfile.Backdrop); return; }

            int[] radii = BoxRadii(sigmaDev);
            if (!ExactBackdrop)
            {
                RenderStats.BackdropGpu++;
                double gsx = Math.Min(p0.x, p1.x);
                double gsy = Math.Min(p0.y, p1.y);
                BackdropBlurGpu(radii, gsx, gsy, Math.Abs(p1.x - p0.x), Math.Abs(p1.y - p0.y), x, y, w, h, x0, y0, x1, y1);
                PaintProfile.End(PaintProfile.Backdrop);
                return;
            }
            RenderStats.BackdropExact++;
            int maxR = 0;
            for (int i = 0; i < radii.Length; i++) if (radii[i] > maxR) maxR = radii[i];
            // Padding stays the original framebuffer; only [x0,y0,x1,y1) is blurred, matching BoxH/BoxV.
            int wx0 = Clamp(x0 - maxR, 0, Width);
            int wy0 = Clamp(y0 - maxR, 0, Height);
            int wx1 = Clamp(x1 + maxR, 0, Width);
            int wy1 = Clamp(y1 + maxR, 0, Height);
            int ww = wx1 - wx0, wh = wy1 - wy0;

            Flush();
            var prev = RenderTexture.active;
            RenderTexture.active = target;
            var shot = new Texture2D(ww, wh, TextureFormat.RGBA32, false, true) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            PaintProfile.CountNewTexture();
            PaintProfile.Begin(PaintProfile.Readback);
            PaintProfile.CropW = ww;
            PaintProfile.CropH = wh;
            PaintProfile.FrameW = Width;
            PaintProfile.FrameH = Height;
            long rp0 = PaintProfile.Stamp();
            shot.ReadPixels(new Rect(wx0, wy0, ww, wh), 0, 0, false);
            PaintProfile.ReadPixelsTicks += PaintProfile.Stamp() - rp0;
            PaintProfile.CountReadPixels();
            shot.Apply(false, false);
            RenderTexture.active = prev;
            snapshots.Add(shot);
            var px = shot.GetPixels32();
            PaintProfile.End(PaintProfile.Readback);

            PaintProfile.Begin(PaintProfile.CpuBlur);
            BoxBlurWindow(px, ww, wh, wx0, wy0, Width, Height, x0, y0, x1, y1, radii);
            shot.SetPixels32(px);
            shot.Apply(false, false);
            PaintProfile.End(PaintProfile.CpuBlur);

            bool smooth = state.imageSmoothing;
            state.imageSmoothing = false;
            var img = new CanvasImage(shot);
            double sx = Math.Min(p0.x, p1.x);
            double sy = Math.Min(p0.y, p1.y);
            double sw = Math.Abs(p1.x - p0.x);
            double sh = Math.Abs(p1.y - p0.y);
            // Image.shader flips y against the texture height, so the crop's y-down origin is the top of the window.
            double yDown0 = Height - wy1;
            DrawImage(img, sx - wx0, sy - yDown0, sw, sh, x, y, w, h);
            state.imageSmoothing = smooth;
            PaintProfile.End(PaintProfile.Backdrop);
        }

        Material matBlur;
        sealed class BlurPair
        {
            public int w, h;
            public RenderTexture a, b;
        }
        readonly BlurPair[] blurPairs = new BlurPair[4];
        int blurPairN;

        /// <summary>GPU copy of <see cref="Target"/> after the command buffer has been executed.</summary>
        public RenderTexture CopyTarget(string name)
        {
            var rt = new RenderTexture(Width, Height, 0, target.format, RenderTextureReadWrite.Linear)
            {
                name = name,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false,
                hideFlags = HideFlags.HideAndDontSave,
            };
            rt.Create();
            Graphics.CopyTexture(target, rt);
            return rt;
        }

        void BackdropBlurGpu(int[] radii, double sx, double sy, double sw, double sh, double x, double y, double w, double h, int x0, int y0, int x1, int y1)
        {
            if (matBlur == null)
            {
                var shader = Resources.Load<Shader>("Canvas2D/BackdropBlur");
                if (shader == null) shader = Shader.Find("Hidden/Canvas2D/BackdropBlur");
                if (shader == null) throw new InvalidOperationException("missing BackdropBlur shader");
                matBlur = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            }
            int radSum = 0;
            for (int i = 0; i < radii.Length; i++) radSum += radii[i];
            int cx0 = Clamp(x0 - radSum, 0, Width);
            int cy0 = Clamp(y0 - radSum, 0, Height);
            int cx1 = Clamp(x1 + radSum, 0, Width);
            int cy1 = Clamp(y1 + radSum, 0, Height);
            int cw = cx1 - cx0, ch = cy1 - cy0;
            if (cw < 1 || ch < 1) return;
            BlurPair pair = BlurPairFor(cw, ch);
            cmd.CopyTexture(target, 0, 0, cx0, cy0, cw, ch, pair.a, 0, 0, 0, 0);
            for (int pass = 0; pass < radii.Length; pass++)
            {
                matBlur.SetFloat("_Radius", radii[pass]);
                matBlur.SetVector("_Axis", new Vector4(1f, 0f, 0f, 0f));
                cmd.Blit(pair.a, pair.b, matBlur);
                matBlur.SetVector("_Axis", new Vector4(0f, 1f, 0f, 0f));
                cmd.Blit(pair.b, pair.a, matBlur);
            }
            bool smooth = state.imageSmoothing;
            state.imageSmoothing = false;
            double localSx = sx - cx0;
            double localSy = sy - (Height - cy1);
            DrawTexture(pair.a, localSx, localSy, sw, sh, x, y, w, h);
            state.imageSmoothing = smooth;
        }

        BlurPair BlurPairFor(int w, int h)
        {
            for (int i = 0; i < blurPairN; i++)
                if (blurPairs[i].w == w && blurPairs[i].h == h) return blurPairs[i];
            var pair = new BlurPair { w = w, h = h };
            if (blurPairN < blurPairs.Length)
            {
                blurPairs[blurPairN++] = pair;
            }
            else
            {
                ReleasePair(blurPairs[0]);
                for (int i = 1; i < blurPairs.Length; i++) blurPairs[i - 1] = blurPairs[i];
                blurPairs[blurPairs.Length - 1] = pair;
            }
            pair.a = MakeSized(w, h, "Canvas2D.BlurA");
            pair.b = MakeSized(w, h, "Canvas2D.BlurB");
            PaintProfile.CountNewTexture();
            return pair;
        }

        RenderTexture MakeSized(int w, int h, string name)
        {
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = name, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, useMipMap = false, autoGenerateMips = false,
                hideFlags = HideFlags.HideAndDontSave,
            };
            rt.Create();
            return rt;
        }

        static void ReleasePair(BlurPair pair)
        {
            if (pair == null) return;
            if (pair.a != null) { pair.a.Release(); DestroyOwned(pair.a); }
            if (pair.b != null) { pair.b.Release(); DestroyOwned(pair.b); }
        }

        static int Clamp(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);

        /// <summary>
        /// Same three box passes as <see cref="BoxBlur"/>, on a crop of the frame.
        /// <paramref name="px"/> holds rows <paramref name="wy0"/>..<paramref name="wy0"/>+<paramref name="wh"/> and columns
        /// <paramref name="wx0"/>..+<paramref name="ww"/> of a <paramref name="frameW"/>×<paramref name="frameH"/> image.
        /// Pixels outside [x0,y0,x1,y1) stay at the crop's original values, which is what the full-frame passes copy through.
        /// </summary>
        static void BoxBlurWindow(Color32[] px, int ww, int wh, int wx0, int wy0, int frameW, int frameH, int x0, int y0, int x1, int y1, int[] radii)
        {
            var tmp = new Color32[px.Length];
            Array.Copy(px, tmp, px.Length);
            for (int pass = 0; pass < radii.Length; pass++)
            {
                BoxHWindow(px, tmp, ww, wx0, wy0, frameW, x0, y0, x1, y1, radii[pass]);
                BoxVWindow(tmp, px, ww, wx0, wy0, frameH, x0, y0, x1, y1, radii[pass]);
            }
        }

        static int WinIndex(int ww, int wx0, int wy0, int x, int y) => (y - wy0) * ww + (x - wx0);

        static void BoxHWindow(Color32[] src, Color32[] dst, int ww, int wx0, int wy0, int frameW, int x0, int y0, int x1, int y1, int r)
        {
            for (int y = y0; y < y1; y++)
            {
                int x = x0;
                int a = Math.Max(0, x - r);
                int b = Math.Min(frameW - 1, x + r);
                int sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                for (int i = a; i <= b; i++)
                {
                    var c = src[WinIndex(ww, wx0, wy0, i, y)];
                    sumR += c.r; sumG += c.g; sumB += c.b; sumA += c.a;
                }
                while (true)
                {
                    int count = b - a + 1;
                    dst[WinIndex(ww, wx0, wy0, x, y)] = new Color32((byte)(sumR / count), (byte)(sumG / count), (byte)(sumB / count), (byte)(sumA / count));
                    if (++x >= x1) break;
                    int na = Math.Max(0, x - r);
                    int nb = Math.Min(frameW - 1, x + r);
                    while (a < na)
                    {
                        var c = src[WinIndex(ww, wx0, wy0, a, y)];
                        sumR -= c.r; sumG -= c.g; sumB -= c.b; sumA -= c.a;
                        a++;
                    }
                    while (b < nb)
                    {
                        b++;
                        var c = src[WinIndex(ww, wx0, wy0, b, y)];
                        sumR += c.r; sumG += c.g; sumB += c.b; sumA += c.a;
                    }
                }
            }
        }

        static void BoxVWindow(Color32[] src, Color32[] dst, int ww, int wx0, int wy0, int frameH, int x0, int y0, int x1, int y1, int r)
        {
            for (int x = x0; x < x1; x++)
            {
                int y = y0;
                int a = Math.Max(0, y - r);
                int b = Math.Min(frameH - 1, y + r);
                int sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                for (int i = a; i <= b; i++)
                {
                    var c = src[WinIndex(ww, wx0, wy0, x, i)];
                    sumR += c.r; sumG += c.g; sumB += c.b; sumA += c.a;
                }
                while (true)
                {
                    int count = b - a + 1;
                    dst[WinIndex(ww, wx0, wy0, x, y)] = new Color32((byte)(sumR / count), (byte)(sumG / count), (byte)(sumB / count), (byte)(sumA / count));
                    if (++y >= y1) break;
                    int na = Math.Max(0, y - r);
                    int nb = Math.Min(frameH - 1, y + r);
                    while (a < na)
                    {
                        var c = src[WinIndex(ww, wx0, wy0, x, a)];
                        sumR -= c.r; sumG -= c.g; sumB -= c.b; sumA -= c.a;
                        a++;
                    }
                    while (b < nb)
                    {
                        b++;
                        var c = src[WinIndex(ww, wx0, wy0, x, b)];
                        sumR += c.r; sumG += c.g; sumB += c.b; sumA += c.a;
                    }
                }
            }
        }

        static void BoxBlur(Color32[] px, int width, int height, int x0, int y0, int x1, int y1, double sigma)
        {
            if (x1 <= x0 || y1 <= y0) return;
            int[] radii = BoxRadii(sigma);
            var tmp = new Color32[px.Length];
            for (int pass = 0; pass < radii.Length; pass++)
            {
                BoxH(px, tmp, width, height, x0, y0, x1, y1, radii[pass]);
                BoxV(tmp, px, width, height, x0, y0, x1, y1, radii[pass]);
            }
        }

        static int[] BoxRadii(double sigma)
        {
            const int n = 3;
            double wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
            int wl = (int)Math.Floor(wIdeal);
            if ((wl & 1) == 0) wl--;
            if (wl < 1) wl = 1;
            int wu = wl + 2;
            double m = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4.0 * wl - 4);
            int mI = (int)Math.Round(m);
            if (mI < 0) mI = 0;
            if (mI > n) mI = n;
            var r = new int[n];
            for (int i = 0; i < n; i++) r[i] = Math.Max(0, ((i < mI ? wl : wu) - 1) / 2);
            return r;
        }

        static void BoxH(Color32[] src, Color32[] dst, int width, int height, int x0, int y0, int x1, int y1, int r)
        {
            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                bool hot = y >= y0 && y < y1 && r > 0;
                if (!hot)
                {
                    Array.Copy(src, row, dst, row, width);
                    continue;
                }
                int i0 = Math.Max(0, x0 - r), i1 = Math.Min(width - 1, x1 - 1 + r);
                int sumR = 0, sumG = 0, sumB = 0, sumA = 0, count = 0;
                for (int i = i0; i <= Math.Min(width - 1, i0 + 2 * r); i++)
                {
                    var c = src[row + i];
                    sumR += c.r; sumG += c.g; sumB += c.b; sumA += c.a; count++;
                }
                for (int x = 0; x < width; x++)
                {
                    if (x < x0 || x >= x1) { dst[row + x] = src[row + x]; continue; }
                    // rebuild window centered on x, clamped
                    int a = Math.Max(0, x - r), b = Math.Min(width - 1, x + r);
                    sumR = sumG = sumB = sumA = 0; count = b - a + 1;
                    for (int i = a; i <= b; i++)
                    {
                        var c = src[row + i];
                        sumR += c.r; sumG += c.g; sumB += c.b; sumA += c.a;
                    }
                    dst[row + x] = new Color32((byte)(sumR / count), (byte)(sumG / count), (byte)(sumB / count), (byte)(sumA / count));
                }
            }
        }

        static void BoxV(Color32[] src, Color32[] dst, int width, int height, int x0, int y0, int x1, int y1, int r)
        {
            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                if (y < y0 || y >= y1 || r == 0)
                {
                    Array.Copy(src, row, dst, row, width);
                    continue;
                }
                int a = Math.Max(0, y - r), b = Math.Min(height - 1, y + r);
                int count = b - a + 1;
                for (int x = x0; x < x1; x++)
                {
                    int sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                    for (int i = a; i <= b; i++)
                    {
                        var c = src[i * width + x];
                        sumR += c.r; sumG += c.g; sumB += c.b; sumA += c.a;
                    }
                    dst[row + x] = new Color32((byte)(sumR / count), (byte)(sumG / count), (byte)(sumB / count), (byte)(sumA / count));
                }
                for (int x = 0; x < x0; x++) dst[row + x] = src[row + x];
                for (int x = x1; x < width; x++) dst[row + x] = src[row + x];
            }
        }

        public void Replay()
        {
            if (SkipSubmit)
                throw new InvalidOperationException("Canvas2D.SkipSubmit refuses Replay: it would execute the command buffer.");
            ExecuteRecorded();
        }

        void ExecuteRecorded()
        {
            Graphics.ExecuteCommandBuffer(cmd);
            ExecuteCount++;
            RenderStats.Submits++;
            RenderStats.Commands += pendingDraws;
            pendingDraws = 0;
        }

        static void EnsureBuffer(ref GraphicsBuffer buf, int count, int stride)
        {
            if (buf != null && buf.count >= Math.Max(count, 1)) return;
            buf?.Dispose();
            int n = 1024;
            while (n < count) n *= 2;
            buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, n, stride);
        }

        int NewParams()
        {
            if (prmCount + ParamStride > prm.Length) Array.Resize(ref prm, prm.Length * 2);
            int b = prmCount;
            Array.Clear(prm, b, ParamStride);
            prmCount += ParamStride;
            return b;
        }

        void Draw(Material m, int pass, int verts, int pb)
        {
            cmd.SetGlobalFloat(IdParamBase, pb);
            cmd.DrawProcedural(Matrix4x4.identity, m, pass, MeshTopology.Triangles, verts);
            DrawCount++;
            pendingDraws++;
            RenderStats.Draws++;
            PaintProfile.CountDraw(verts);
        }

        void SetRect(int pb, int x0, int y0, int x1, int y1) { prm[pb + IdxRect] = new Vector4(x0, y0, x1, y1); }

        // ---- edges ----------------------------------------------------------------------------

        bool snapY;

        bool EmitEdges(PolygonSet ps, double dx, double dy, out PixelRect rect, out int ebase, out int ecount, bool snap = true)
        {
            snapY = snap;
            rect = default; ebase = edgeCount; ecount = 0;
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            var pts = ps.pts;
            if (pts.Count == 0) return false;
            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                double x = p.x + dx, y = p.y + dy;
                if (double.IsNaN(x) || double.IsNaN(y)) return false;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
            var r = new PixelRect((int)Math.Floor(Math.Max(minX, -1)), (int)Math.Floor(Math.Max(minY, -1)),
                (int)Math.Ceiling(Math.Min(maxX, Width + 1)), (int)Math.Ceiling(Math.Min(maxY, Height + 1)));
            r = r.Intersect(new PixelRect(0, 0, Width, Height));
            if (r.IsEmpty) return false;
            rect = r;
            for (int i = 0; i < ps.PolygonCount; i++)
            {
                int s = ps.Start(i), e = ps.End(i);
                int n = e - s;
                if (n < 2) continue;
                for (int j = 0; j < n; j++)
                {
                    var a = pts[s + j];
                    var b = pts[s + (j + 1) % n];
                    AddEdge(a.x + dx, a.y + dy, b.x + dx, b.y + dy);
                }
            }
            ecount = edgeCount - ebase;
            return ecount > 0;
        }

        void Push(double x0, double y0, double x1, double y1)
        {
            if ((float)y0 == (float)y1) return;
            if (edgeCount == edges.Length) Array.Resize(ref edges, edges.Length * 2);
            edges[edgeCount++] = new Vector4((float)x0, (float)y0, (float)x1, (float)y1);
        }

        void AddEdge(double x0, double y0, double x1, double y1)
        {
            if (snapY)
            {
                double sy0 = Math.Floor(y0 * 4 + 0.5) * 0.25, sy1 = Math.Floor(y1 * 4 + 0.5) * 0.25;
                if (sy0 == sy1) return;
                double slope = (x1 - x0) / (y1 - y0);
                double nx0 = x0 + (sy0 - y0) * slope, nx1 = x0 + (sy1 - y0) * slope;
                x0 = nx0; x1 = nx1; y0 = sy0; y1 = sy1;
            }
            if (y0 == y1) return;
            double H = Height, W = Width;
            if (Math.Max(y0, y1) <= 0 || Math.Min(y0, y1) >= H) return;
            double dyv = y1 - y0;
            if (y0 < 0) { x0 += (x1 - x0) * (0 - y0) / dyv; y0 = 0; }
            else if (y0 > H) { x0 += (x1 - x0) * (H - y0) / dyv; y0 = H; }
            if (y1 < 0) { x1 = x0 + (x1 - x0) * (0 - y0) / (y1 - y0); y1 = 0; }
            else if (y1 > H) { x1 = x0 + (x1 - x0) * (H - y0) / (y1 - y0); y1 = H; }
            if (y0 == y1) return;

            if (x0 >= 0 && x0 <= W && x1 >= 0 && x1 <= W) { Push(x0, y0, x1, y1); return; }
            double ta = 0, tb = 1;
            double dxv = x1 - x0;
            Span<double> cuts = stackalloc double[4];
            int nc = 0;
            cuts[nc++] = 0;
            double c0 = 2, c1 = 2;
            if (dxv != 0)
            {
                double t0 = (0 - x0) / dxv, tw = (W - x0) / dxv;
                if (t0 > 0 && t0 < 1) c0 = t0;
                if (tw > 0 && tw < 1) c1 = tw;
            }
            if (c0 > c1) { double t = c0; c0 = c1; c1 = t; }
            if (c0 < 2) cuts[nc++] = c0;
            if (c1 < 2) cuts[nc++] = c1;
            cuts[nc++] = 1;
            for (int i = 0; i + 1 < nc; i++)
            {
                ta = cuts[i]; tb = cuts[i + 1];
                double xa = x0 + dxv * ta, xb = x0 + dxv * tb;
                double ya = y0 + (y1 - y0) * ta, yb = y0 + (y1 - y0) * tb;
                double xm = (xa + xb) * 0.5;
                if (xm < 0) Push(0, ya, 0, yb);
                else if (xm > W) continue;
                else Push(Math.Min(Math.Max(xa, 0), W), ya, Math.Min(Math.Max(xb, 0), W), yb);
            }
        }

        // ---- paint -----------------------------------------------------------------------------

        bool WritePaint(int pb, bool stroke, float alpha)
        {
            var st = state;
            var grad = stroke ? st.strokeGrad : st.fillGrad;
            var col = stroke ? st.strokeColor : st.fillColor;
            prm[pb + IdxAlpha] = new Vector4(alpha, 0, 0, 0);
            if (grad == null)
            {
                prm[pb + IdxColor] = new Vector4(col.r, col.g, col.b, col.a);
                return true;
            }
            if (!st.ctm.IsInvertible) return false;
            var inv = st.ctm.Inverse();
            prm[pb + IdxGx0] = new Vector4((float)inv.a, (float)inv.c, (float)inv.e, 0);
            prm[pb + IdxGx1] = new Vector4((float)inv.b, (float)inv.d, (float)inv.f, 0);
            int n = Math.Min(grad.stops.Count, 8);
            float kind = 3;
            if (n > 0)
            {
                if (!grad.radial)
                {
                    double dx = grad.x1 - grad.x0, dy = grad.y1 - grad.y0;
                    if (dx != 0 || dy != 0)
                    {
                        kind = 1;
                        prm[pb + IdxGa] = new Vector4((float)grad.x0, (float)grad.y0, (float)dx, (float)dy);
                    }
                }
                else
                {
                    double dx = grad.x1 - grad.x0, dy = grad.y1 - grad.y0, dr = grad.r1 - grad.r0;
                    if (dx != 0 || dy != 0 || dr != 0)
                    {
                        kind = 2;
                        prm[pb + IdxGa] = new Vector4((float)grad.x0, (float)grad.y0, (float)grad.r0, 0);
                        prm[pb + IdxGb] = new Vector4((float)dx, (float)dy, (float)dr, 0);
                    }
                }
            }
            prm[pb + IdxAlpha] = new Vector4(alpha, kind, GradientInterpolatesPremultiplied ? 1f : 0f, n);
            Array.Clear(gradOff, 0, 8);
            for (int i = 0; i < n; i++)
            {
                var s = grad.stops[i];
                prm[pb + IdxGcol + i] = new Vector4(s.Value.r, s.Value.g, s.Value.b, s.Value.a);
                gradOff[i] = s.Key;
            }
            prm[pb + IdxGoff] = new Vector4(gradOff[0], gradOff[1], gradOff[2], gradOff[3]);
            prm[pb + IdxGoff + 1] = new Vector4(gradOff[4], gradOff[5], gradOff[6], gradOff[7]);
            return true;
        }

        void BindClip(int pb)
        {
            if (state.clip != null)
            {
                prm[pb + IdxFlags].z = 1;
                cmd.SetGlobalTexture(IdClipTex, state.clip);
            }
        }

        // ---- shapes ----------------------------------------------------------------------------

        void AccumulateTo(int pb, PixelRect rect, int ebase, int ecount)
        {
            SetRect(pb, rect.x0, rect.y0, rect.x1, rect.y1);
            prm[pb + IdxEdge] = new Vector4(ebase, rect.x1, 0, 0);
            cmd.SetRenderTarget(accum);
            Draw(matAccum, AccClearPass, 6, pb);
            Draw(matAccum, AccPass, 6 * ecount, pb);
        }

        void RenderShape(PolygonSet ps, bool evenOdd, bool stroke, float alphaMul, bool snap = true)
        {
            PaintProfile.Begin(PaintProfile.RenderShape);
            var st = state;
            if (st.globalAlpha <= 0 || !st.ctm.IsInvertible) { PaintProfile.End(PaintProfile.RenderShape); return; }
            float alpha = (float)(st.globalAlpha * alphaMul);
            bool shadow = st.shadowColor.a > 0 && (st.shadowBlur > 0 || st.shadowOffsetX != 0 || st.shadowOffsetY != 0);
            if (shadow) RenderShadow(ps, evenOdd, stroke, alpha, snap);

            if (!EmitEdges(ps, 0, 0, out var rect, out int ebase, out int ecount, snap)) { PaintProfile.End(PaintProfile.RenderShape); return; }
            int pb = NewParams();
            if (!WritePaint(pb, stroke, alpha)) { PaintProfile.End(PaintProfile.RenderShape); return; }
            prm[pb + IdxFlags] = new Vector4(0, evenOdd ? 1 : 0, 0, 0);
            BindClip(pb);
            if (glyphMaskFill) WriteGlyphPreblend(pb);
            AccumulateTo(pb, rect, ebase, ecount);
            cmd.SetRenderTarget(target);
            Draw(matFill, FillSourceOver, 6, pb);
            PaintProfile.End(PaintProfile.RenderShape);
        }

        void RenderShadow(PolygonSet ps, bool evenOdd, bool stroke, float alpha, bool snap)
        {
            PaintProfile.Begin(PaintProfile.Shadow);
            var st = state;
            double dx = st.shadowOffsetX * ShadowScale, dy = st.shadowOffsetY * ShadowScale;
            double blur = st.shadowBlur * ShadowScale;
            double sigma = blur * 0.5;
            int radius = sigma > 0 ? (int)Math.Ceiling(sigma * 3) : 0;
            if (radius > 0 && (SkipBlurredShadows || SkipBlurredFx))
            {
                PaintProfile.End(PaintProfile.Shadow);
                return;
            }
            PaintProfile.CountShadow();
            RenderStats.Shadows++;
            if (!EmitEdges(ps, dx, dy, out var rect, out int ebase, out int ecount, snap)) { PaintProfile.End(PaintProfile.Shadow); return; }
            var full = new PixelRect(0, 0, Width, Height);
            int pbA = NewParams();
            if (!WritePaint(pbA, stroke, alpha)) { PaintProfile.End(PaintProfile.Shadow); return; }
            prm[pbA + IdxFlags] = new Vector4(0, evenOdd ? 1 : 0, 0, 1);
            prm[pbA + IdxShadow] = new Vector4(st.shadowColor.r, st.shadowColor.g, st.shadowColor.b, st.shadowColor.a);
            BindClip(pbA);
            AccumulateTo(pbA, rect, ebase, ecount);
            if (radius == 0)
            {
                cmd.SetRenderTarget(target);
                Draw(matFill, FillSourceOver, 6, pbA);
                PaintProfile.End(PaintProfile.Shadow);
                return;
            }
            var big = rect.Expand(radius).Intersect(full);

            cmd.SetRenderTarget(maskA);
            Draw(matMask, MaskCoverage, 6, pbA);

            int pbH = NewParams();
            SetRect(pbH, big.x0, big.y0, big.x1, big.y1);
            prm[pbH + IdxMaskRect] = new Vector4(rect.x0, rect.y0, rect.x1, rect.y1);
            prm[pbH + IdxBlur] = new Vector4((float)sigma, radius, 0, 0);
            cmd.SetRenderTarget(maskB);
            cmd.SetGlobalTexture(IdMask, maskA);
            Draw(matMask, MaskBlurH, 6, pbH);

            int pbV = NewParams();
            SetRect(pbV, big.x0, big.y0, big.x1, big.y1);
            prm[pbV + IdxMaskRect] = new Vector4(big.x0, big.y0, big.x1, big.y1);
            prm[pbV + IdxBlur] = new Vector4((float)sigma, radius, 0, 0);
            cmd.SetRenderTarget(maskA);
            cmd.SetGlobalTexture(IdMask, maskB);
            Draw(matMask, MaskBlurV, 6, pbV);

            int pbF = NewParams();
            WritePaint(pbF, stroke, alpha);
            SetRect(pbF, big.x0, big.y0, big.x1, big.y1);
            prm[pbF + IdxFlags] = new Vector4(1, 0, 0, 1);
            prm[pbF + IdxShadow] = prm[pbA + IdxShadow];
            BindClip(pbF);
            cmd.SetRenderTarget(target);
            cmd.SetGlobalTexture(IdMask, maskA);
            Draw(matFill, FillSourceOver, 6, pbF);
            PaintProfile.End(PaintProfile.Shadow);
        }

        void RenderClear(PolygonSet ps, bool snap)
        {
            if (!EmitEdges(ps, 0, 0, out var rect, out int ebase, out int ecount, snap)) return;
            int pb = NewParams();
            prm[pb + IdxFlags] = new Vector4(0, 0, 0, 0);
            BindClip(pb);
            AccumulateTo(pb, rect, ebase, ecount);
            cmd.SetRenderTarget(target);
            Draw(matFill, FillClear, 6, pb);
        }

        RenderTexture NextClipRT()
        {
            if (clipUsed == clipPool.Count) clipPool.Add(MakeRT(RenderTextureFormat.R8, "Canvas2D.Clip" + clipPool.Count));
            return clipPool[clipUsed++];
        }

        void ApplyClip(PolygonSet ps, bool evenOdd)
        {
            var full = new PixelRect(0, 0, Width, Height);
            var next = NextClipRT();
            int pb = NewParams();
            SetRect(pb, 0, 0, Width, Height);
            bool any = EmitEdges(ps, 0, 0, out var rect, out int ebase, out int ecount, true);
            prm[pb + IdxFlags] = new Vector4(0, evenOdd ? 1 : 0, state.clip != null ? 1f : 0f, 0);
            cmd.SetRenderTarget(accum);
            cmd.ClearRenderTarget(false, true, Color.clear);
            if (any)
            {
                int pbA = NewParams();
                AccumulateTo(pbA, rect, ebase, ecount);
            }
            cmd.SetRenderTarget(next);
            if (state.clip != null) cmd.SetGlobalTexture(IdClipTex, state.clip);
            Draw(matMask, MaskClip, 6, pb);
            state.clip = next;
        }
    }
}
