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

        readonly RenderTexture target;
        readonly RenderTexture accum;
        readonly RenderTexture maskA, maskB;
        readonly List<RenderTexture> clipPool = new List<RenderTexture>();
        int clipUsed;
        readonly CommandBuffer cmd = new CommandBuffer { name = "Canvas2D" };
        GraphicsBuffer edgeBuffer, paramBuffer;
        Vector4[] edges = new Vector4[4096];
        int edgeCount;
        Vector4[] prm = new Vector4[ParamStride * 256];
        int prmCount;
        Material matAccum, matFill, matMask;

        public RenderTexture Target => target;
        public CommandBuffer Commands => cmd;
        public int EdgeCount => edgeCount;
        public int DrawCount { get; private set; }

        public Canvas2D(int width, int height)
        {
            Width = width;
            Height = height;
            target = MakeRT(RenderTextureFormat.ARGB32, "Canvas2D.Target");
            accum = MakeRT(RenderTextureFormat.RFloat, "Canvas2D.Accum");
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
            if (sh == null) throw new InvalidOperationException("Shader not found: " + shaderName);
            return new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
        }

        public void Dispose()
        {
            DisposeText();
            edgeBuffer?.Dispose();
            paramBuffer?.Dispose();
            cmd.Release();
            foreach (var c in clipPool) c.Release();
            target.Release(); accum.Release(); maskA.Release(); maskB.Release();
        }

        public void BeginFrame()
        {
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

        public void EndFrame()
        {
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
            Graphics.ExecuteCommandBuffer(cmd);
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
        }

        void SetRect(int pb, int x0, int y0, int x1, int y1) { prm[pb + IdxRect] = new Vector4(x0, y0, x1, y1); }

        // ---- edges ----------------------------------------------------------------------------

        bool EmitEdges(PolygonSet ps, double dx, double dy, out PixelRect rect, out int ebase, out int ecount)
        {
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
            var offs = new float[8];
            for (int i = 0; i < n; i++)
            {
                var s = grad.stops[i];
                prm[pb + IdxGcol + i] = new Vector4(s.Value.r, s.Value.g, s.Value.b, s.Value.a);
                offs[i] = s.Key;
            }
            prm[pb + IdxGoff] = new Vector4(offs[0], offs[1], offs[2], offs[3]);
            prm[pb + IdxGoff + 1] = new Vector4(offs[4], offs[5], offs[6], offs[7]);
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

        void RenderShape(PolygonSet ps, bool evenOdd, bool stroke, float alphaMul)
        {
            var st = state;
            if (st.globalAlpha <= 0) return;
            if (!st.ctm.IsInvertible) return;
            float alpha = (float)(st.globalAlpha * alphaMul);
            bool shadow = st.shadowColor.a > 0 && (st.shadowBlur > 0 || st.shadowOffsetX != 0 || st.shadowOffsetY != 0);
            if (shadow) RenderShadow(ps, evenOdd, stroke, alpha);

            if (!EmitEdges(ps, 0, 0, out var rect, out int ebase, out int ecount)) return;
            int pb = NewParams();
            if (!WritePaint(pb, stroke, alpha)) return;
            prm[pb + IdxFlags] = new Vector4(0, evenOdd ? 1 : 0, 0, 0);
            BindClip(pb);
            AccumulateTo(pb, rect, ebase, ecount);
            cmd.SetRenderTarget(target);
            Draw(matFill, FillSourceOver, 6, pb);
        }

        void RenderShadow(PolygonSet ps, bool evenOdd, bool stroke, float alpha)
        {
            var st = state;
            double dx = st.shadowOffsetX * ShadowScale, dy = st.shadowOffsetY * ShadowScale;
            double blur = st.shadowBlur * ShadowScale;
            double sigma = blur * 0.5;
            int radius = sigma > 0 ? (int)Math.Ceiling(sigma * 3) : 0;
            if (!EmitEdges(ps, dx, dy, out var rect, out int ebase, out int ecount)) return;
            var full = new PixelRect(0, 0, Width, Height);
            int pbA = NewParams();
            if (!WritePaint(pbA, stroke, alpha)) return;
            prm[pbA + IdxFlags] = new Vector4(0, evenOdd ? 1 : 0, 0, 1);
            prm[pbA + IdxShadow] = new Vector4(st.shadowColor.r, st.shadowColor.g, st.shadowColor.b, st.shadowColor.a);
            BindClip(pbA);
            AccumulateTo(pbA, rect, ebase, ecount);
            if (radius == 0)
            {
                cmd.SetRenderTarget(target);
                Draw(matFill, FillSourceOver, 6, pbA);
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
        }

        void RenderClear(PolygonSet ps)
        {
            if (!EmitEdges(ps, 0, 0, out var rect, out int ebase, out int ecount)) return;
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
            bool any = EmitEdges(ps, 0, 0, out var rect, out int ebase, out int ecount);
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
