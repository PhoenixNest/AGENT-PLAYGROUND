using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace OrbitalSalvage.Render
{
    public sealed partial class Canvas2D
    {
        sealed class ShadowSlot
        {
            public int w, h;
            public RenderTexture accum, maskA, maskB, color;
        }

        sealed class ShadowBitmap
        {
            public int hash, w, h;
            public RenderTexture tex;
        }

        sealed class BackdropSlot
        {
            public int id, div, w, h, age;
            public double x, y, rw, rh;
            public RenderTexture tex;
        }

        readonly ShadowSlot[] shadowSlots = new ShadowSlot[4];
        readonly ShadowBitmap[] shadowBitmaps = new ShadowBitmap[24];
        readonly BackdropSlot[] backdropSlots = new BackdropSlot[4];
        readonly int[] optRadii = new int[3];
        RenderTexture backdropLoA, backdropLoB;
        int backdropLoW, backdropLoH;
        Material matShadowBlit;
        Material matRound;
        Material matFast;
        GraphicsBuffer fastBuf;
        uint[] fastBase = new uint[64];
        int fastN;
        int fastFlushed;
        bool fastFlushing;
        static readonly int IdFastBase = Shader.PropertyToID("_C2D_FastBase");
        static readonly int IdFastOffset = Shader.PropertyToID("_C2D_FastOffset");

        void ClearKeepRect(int x0, int y0, int x1, int y1)
        {
            int pb = NewParams();
            SetRect(pb, x0, y0, x1, y1);
            cmd.SetRenderTarget(target);
            Draw(matAccum, AccClearPass, 6, pb);
        }

        void EnableLiveScissor()
        {
            if (!ScissorOn || ScissorW <= 0 || ScissorH <= 0) return;
            cmd.EnableScissorRect(new Rect(ScissorX, ScissorY, ScissorW, ScissorH));
            scissorLive = true;
        }

        void ReleaseOptBuffers()
        {
            for (int i = 0; i < shadowSlots.Length; i++) ReleaseSlot(shadowSlots[i]);
            for (int i = 0; i < shadowBitmaps.Length; i++) ReleaseBitmap(shadowBitmaps[i]);
            for (int i = 0; i < backdropSlots.Length; i++)
            {
                if (backdropSlots[i] == null) continue;
                ReleaseRt(backdropSlots[i].tex);
                backdropSlots[i] = null;
            }
            ReleaseRt(backdropLoA);
            ReleaseRt(backdropLoB);
            backdropLoA = backdropLoB = null;
            DestroyOwned(matShadowBlit);
            matShadowBlit = null;
            DestroyOwned(matRound);
            matRound = null;
            DestroyOwned(matFast);
            matFast = null;
            fastBuf?.Dispose();
            fastBuf = null;
            fastN = 0;
            fastFlushed = 0;
        }

        static void ReleaseSlot(ShadowSlot slot)
        {
            if (slot == null) return;
            ReleaseRt(slot.accum);
            ReleaseRt(slot.maskA);
            ReleaseRt(slot.maskB);
            ReleaseRt(slot.color);
        }

        static void ReleaseBitmap(ShadowBitmap bitmap)
        {
            if (bitmap == null) return;
            ReleaseRt(bitmap.tex);
        }

        static void ReleaseRt(RenderTexture rt)
        {
            if (rt == null) return;
            rt.Release();
            DestroyOwned(rt);
        }

        static RenderTexture NewRt(int w, int h, RenderTextureFormat fmt, FilterMode filter, string name)
        {
            var rt = new RenderTexture(w, h, 0, fmt, RenderTextureReadWrite.Linear)
            {
                name = name,
                filterMode = filter,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false,
                hideFlags = HideFlags.HideAndDontSave,
            };
            rt.Create();
            return rt;
        }

        static bool PickMask(bool low, out RenderTextureFormat fmt)
        {
            if (low && SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8))
            {
                fmt = RenderTextureFormat.R8;
                return true;
            }
            if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RHalf))
            {
                fmt = RenderTextureFormat.RHalf;
                return true;
            }
            fmt = RenderTextureFormat.RHalf;
            return false;
        }

        ShadowSlot SlotFor(int w, int h, bool low)
        {
            if (!PickMask(low, out var maskFmt)) return null;
            var accumFmt = accum.format;
            for (int i = 0; i < shadowSlots.Length; i++)
            {
                var s = shadowSlots[i];
                if (s != null && s.w == w && s.h == h && s.maskA != null && s.maskA.format == maskFmt)
                    return s;
            }
            int at = 0;
            for (int i = 0; i < shadowSlots.Length; i++)
                if (shadowSlots[i] == null) { at = i; break; }
            if (shadowSlots[at] != null) ReleaseSlot(shadowSlots[at]);
            var slot = new ShadowSlot { w = w, h = h };
            slot.accum = NewRt(w, h, accumFmt, FilterMode.Point, "Canvas2D.ShadowAccum");
            slot.maskA = NewRt(w, h, maskFmt, FilterMode.Point, "Canvas2D.ShadowMaskA");
            slot.maskB = NewRt(w, h, maskFmt, FilterMode.Point, "Canvas2D.ShadowMaskB");
            slot.color = NewRt(w, h, RenderTextureFormat.ARGB32, FilterMode.Bilinear, "Canvas2D.ShadowColor");
            shadowSlots[at] = slot;
            return slot;
        }

        void BindFullTarget()
        {
            cmd.SetGlobalVector(IdTarget, new Vector4(Width, Height, 1f / Width, 1f / Height));
            cmd.SetGlobalVector(IdMaskOrigin, Vector4.zero);
            cmd.SetGlobalTexture(IdAccum, accum);
        }

        bool TryShadowOpt(PixelRect rect, int ebase, int ecount, bool evenOdd, bool stroke, float alpha, double sigma, int radius)
        {
            var big = rect.Expand(radius).Intersect(new PixelRect(0, 0, Width, Height));
            if (big.Width < 2 || big.Height < 2 || ecount < 1)
            {
                RenderStats.ShadowMissSmall++;
                return false;
            }
            int div = 1;
            if (RenderOpts.ShadowLow) div = radius >= 8 ? 4 : 2;
            int lw = Math.Max(1, big.Width / div);
            int lh = Math.Max(1, big.Height / div);
            float sx = lw / (float)big.Width;
            float sy = lh / (float)big.Height;
            float sigmaL = (float)sigma * (sx + sy) * 0.5f;
            int radiusL = div == 1 ? radius : Math.Max(1, (int)Math.Ceiling(sigmaL * 3.0));
            var col = state.shadowColor;
            var need = rect.Expand(radius);
            bool cacheable = RenderOpts.ShadowLow && need.x0 >= 0 && need.y0 >= 0 && need.x1 <= Width && need.y1 <= Height;
            int hash = 0;
            if (cacheable)
            {
                hash = HashShadow(ebase, ecount, rect.x0, rect.y0, radius, div, col);
                for (int i = 0; i < shadowBitmaps.Length; i++)
                {
                    var b = shadowBitmaps[i];
                    if (b != null && b.hash == hash && b.w == lw && b.h == lh)
                    {
                        RenderStats.ShadowCacheHit++;
                        BlitShadow(b.tex, big, true);
                        return true;
                    }
                }
            }

            var slot = SlotFor(lw, lh, div > 1);
            if (slot == null)
            {
                RenderStats.ShadowMissFmt++;
                return false;
            }
            if (ScissorOn) cmd.DisableScissorRect();

            int lb = edgeCount;
            for (int i = 0; i < ecount; i++)
            {
                var e = edges[ebase + i];
                if (edgeCount == edges.Length) Array.Resize(ref edges, edges.Length * 2);
                edges[edgeCount++] = new Vector4((e.x - big.x0) * sx, (e.y - big.y0) * sy, (e.z - big.x0) * sx, (e.w - big.y0) * sy);
            }
            int lc = edgeCount - lb;
            int lx0 = Clamp((int)Math.Floor((rect.x0 - big.x0) * sx), 0, lw);
            int ly0 = Clamp((int)Math.Floor((rect.y0 - big.y0) * sy), 0, lh);
            int lx1 = Clamp((int)Math.Ceiling((rect.x1 - big.x0) * sx), 0, lw);
            int ly1 = Clamp((int)Math.Ceiling((rect.y1 - big.y0) * sy), 0, lh);
            if (lx1 <= lx0) lx1 = Math.Min(lw, lx0 + 1);
            if (ly1 <= ly0) ly1 = Math.Min(lh, ly0 + 1);

            // The batch belongs on the canvas. Flushing after this target switch
            // would paint the coverage pass onto the canvas and leave the slot empty.
            FlushFastBatch();
            cmd.SetGlobalVector(IdTarget, new Vector4(lw, lh, 1f / lw, 1f / lh));
            cmd.SetGlobalVector(IdMaskOrigin, Vector4.zero);
            cmd.SetGlobalTexture(IdAccum, slot.accum);
            cmd.SetRenderTarget(slot.accum, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            int pbClear = NewParams();
            SetRect(pbClear, 0, 0, lw, lh);
            Draw(matAccum, AccClearPass, 6, pbClear);
            cmd.SetRenderTarget(slot.accum, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);

            int pbA = NewParams();
            if (!WritePaint(pbA, stroke, alpha))
            {
                BindFullTarget();
                if (ScissorOn) EnableLiveScissor();
                return true;
            }
            prm[pbA + IdxFlags] = new Vector4(0, evenOdd ? 1 : 0, 0, 1);
            prm[pbA + IdxShadow] = new Vector4(col.r, col.g, col.b, col.a);
            SetRect(pbA, lx0, ly0, lx1, ly1);
            prm[pbA + IdxEdge] = new Vector4(lb, lx1, 0, 0);
            Draw(matAccum, AccPass, 6 * lc, pbA);

            cmd.SetRenderTarget(slot.maskA, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            Draw(matMask, MaskCoverage, 6, pbA);

            int pbH = NewParams();
            SetRect(pbH, 0, 0, lw, lh);
            prm[pbH + IdxMaskRect] = new Vector4(lx0, ly0, lx1, ly1);
            prm[pbH + IdxBlur] = new Vector4(sigmaL, radiusL, 0, 0);
            cmd.SetRenderTarget(slot.maskB, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.SetGlobalTexture(IdMask, slot.maskA);
            Draw(matMask, MaskBlurH, 6, pbH);

            int pbV = NewParams();
            SetRect(pbV, 0, 0, lw, lh);
            prm[pbV + IdxMaskRect] = new Vector4(0, 0, lw, lh);
            prm[pbV + IdxBlur] = new Vector4(sigmaL, radiusL, 0, 0);
            cmd.SetRenderTarget(slot.maskA, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.SetGlobalTexture(IdMask, slot.maskB);
            Draw(matMask, MaskBlurV, 6, pbV);

            if (div == 1)
            {
                BindFullTarget();
                cmd.SetGlobalVector(IdMaskOrigin, new Vector4(big.x0, big.y0, 1f, lh));
                cmd.SetGlobalTexture(IdMask, slot.maskA);
                cmd.SetRenderTarget(target);
                if (ScissorOn) EnableLiveScissor();
                int pbF = NewParams();
                WritePaint(pbF, stroke, alpha);
                SetRect(pbF, big.x0, big.y0, big.x1, big.y1);
                prm[pbF + IdxFlags] = new Vector4(1, 0, 0, 1);
                prm[pbF + IdxShadow] = new Vector4(col.r, col.g, col.b, col.a);
                Draw(matFill, FillSourceOver, 6, pbF);
                cmd.SetGlobalVector(IdMaskOrigin, Vector4.zero);
                RenderStats.ShadowScopeHit++;
                return true;
            }

            cmd.SetRenderTarget(slot.color, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.SetGlobalTexture(IdMask, slot.maskA);
            int pbT = NewParams();
            WritePaint(pbT, stroke, alpha);
            SetRect(pbT, 0, 0, lw, lh);
            prm[pbT + IdxFlags] = new Vector4(1, 0, 0, 1);
            prm[pbT + IdxShadow] = new Vector4(col.r, col.g, col.b, col.a);
            Draw(matFill, FillSourceOver, 6, pbT);

            if (cacheable) RememberShadow(hash, lw, lh, slot.color);
            BindFullTarget();
            BlitShadow(slot.color, big, true);
            RenderStats.ShadowLowHit++;
            return true;
        }

        void BlitShadow(RenderTexture tex, PixelRect big, bool bilinear)
        {
            if (tex == null) return;
            tex.filterMode = bilinear ? FilterMode.Bilinear : FilterMode.Point;
            if (matShadowBlit == null) matShadowBlit = MakeMaterial("Hidden/Canvas2D/ShadowBlit");
            int pb = NewParams();
            SetRect(pb, big.x0, big.y0, big.x1, big.y1);
            prm[pb + IdxAlpha] = new Vector4(1f, 0, 0, 0);
            cmd.SetGlobalVector(IdTarget, new Vector4(Width, Height, 1f / Width, 1f / Height));
            cmd.SetGlobalVector(IdMaskOrigin, Vector4.zero);
            cmd.SetRenderTarget(target);
            if (ScissorOn) EnableLiveScissor();
            cmd.SetGlobalTexture(IdLayerTex, tex);
            Draw(matShadowBlit, 0, 6, pb);
        }

        void RememberShadow(int hash, int w, int h, RenderTexture src)
        {
            int at = 0;
            for (int i = 0; i < shadowBitmaps.Length; i++)
            {
                if (shadowBitmaps[i] == null) { at = i; break; }
                at = i;
            }
            if (shadowBitmaps[at] != null && (shadowBitmaps[at].w != w || shadowBitmaps[at].h != h))
            {
                ReleaseRt(shadowBitmaps[at].tex);
                shadowBitmaps[at].tex = null;
            }
            if (shadowBitmaps[at] == null) shadowBitmaps[at] = new ShadowBitmap();
            var b = shadowBitmaps[at];
            if (b.tex == null) b.tex = NewRt(w, h, RenderTextureFormat.ARGB32, FilterMode.Bilinear, "Canvas2D.ShadowCache");
            b.hash = hash;
            b.w = w;
            b.h = h;
            cmd.CopyTexture(src, b.tex);
        }

        static int HashShadow(int ebase, int ecount, int ox, int oy, int radius, int div, Rgba col, Vector4[] edges)
        {
            unchecked
            {
                int h = (int)2166136261;
                h = (h ^ ecount) * 16777619;
                h = (h ^ radius) * 16777619;
                h = (h ^ div) * 16777619;
                h = (h ^ (int)Math.Round(col.r * 255f)) * 16777619;
                h = (h ^ (int)Math.Round(col.g * 255f)) * 16777619;
                h = (h ^ (int)Math.Round(col.b * 255f)) * 16777619;
                h = (h ^ (int)Math.Round(col.a * 255f)) * 16777619;
                for (int i = 0; i < ecount; i++)
                {
                    var e = edges[ebase + i];
                    h = (h ^ (int)Math.Round((e.x - ox) * 4f)) * 16777619;
                    h = (h ^ (int)Math.Round((e.y - oy) * 4f)) * 16777619;
                    h = (h ^ (int)Math.Round((e.z - ox) * 4f)) * 16777619;
                    h = (h ^ (int)Math.Round((e.w - oy) * 4f)) * 16777619;
                }
                return h;
            }
        }

        int HashShadow(int ebase, int ecount, int ox, int oy, int radius, int div, Rgba col)
        {
            return HashShadow(ebase, ecount, ox, oy, radius, div, col, edges);
        }

        bool BackdropOpt(int[] radii, double sx, double sy, double sw, double sh, double x, double y, double w, double h, int cx0, int cy0, int cw, int ch)
        {
            int id = BackdropReuseId;
            BackdropReuseId = 0;
            int div = RenderOpts.BackdropLow ? 4 : 1;
            int bw = Math.Max(1, cw / div);
            int bh = Math.Max(1, ch / div);
            if (id > 0)
            {
                for (int i = 0; i < backdropSlots.Length; i++)
                {
                    var slot = backdropSlots[i];
                    if (slot == null || slot.tex == null || slot.id != id || slot.div != div) continue;
                    if (Math.Abs(slot.x - x) > 0.51 || Math.Abs(slot.y - y) > 0.51 || Math.Abs(slot.rw - w) > 0.51 || Math.Abs(slot.rh - h) > 0.51)
                        continue;
                    if (RenderOpts.BackdropHold)
                    {
                        slot.age++;
                        if (slot.age % 4 == 0) continue;
                    }
                    bool prev = state.imageSmoothing;
                    state.imageSmoothing = div > 1;
                    double scale = slot.w / (double)cw;
                    double localSx = (sx - cx0) * scale;
                    double localSy = (sy - (Height - (cy0 + ch))) * scale;
                    DrawTexture(slot.tex, localSx, localSy, sw * scale, sh * scale, x, y, w, h);
                    state.imageSmoothing = prev;
                    RenderStats.BackdropCacheHit++;
                    return true;
                }
            }

            if (ScissorOn) cmd.DisableScissorRect();
            FlushFastBatch();
            BlurPair hi = BlurPairFor(cw, ch);
            cmd.CopyTexture(target, 0, 0, cx0, cy0, cw, ch, hi.a, 0, 0, 0, 0);
            RenderTexture result = hi.a;
            int n = Math.Min(radii.Length, optRadii.Length);
            for (int i = 0; i < n; i++)
            {
                int r = radii[i] / div;
                if (r < 1 && radii[i] > 0) r = 1;
                optRadii[i] = r;
            }
            if (div > 1)
            {
                EnsureBackdropLo(bw, bh);
                cmd.Blit(hi.a, backdropLoA);
                result = backdropLoA;
                for (int pass = 0; pass < n; pass++)
                {
                    matBlur.SetFloat("_Radius", optRadii[pass]);
                    matBlur.SetVector("_Axis", new Vector4(1f, 0f, 0f, 0f));
                    cmd.Blit(backdropLoA, backdropLoB, matBlur);
                    matBlur.SetVector("_Axis", new Vector4(0f, 1f, 0f, 0f));
                    cmd.Blit(backdropLoB, backdropLoA, matBlur);
                }
            }
            else
            {
                for (int pass = 0; pass < radii.Length; pass++)
                {
                    matBlur.SetFloat("_Radius", radii[pass]);
                    matBlur.SetVector("_Axis", new Vector4(1f, 0f, 0f, 0f));
                    cmd.Blit(hi.a, hi.b, matBlur);
                    matBlur.SetVector("_Axis", new Vector4(0f, 1f, 0f, 0f));
                    cmd.Blit(hi.b, hi.a, matBlur);
                }
            }
            if (ScissorOn) EnableLiveScissor();
            bool smooth = state.imageSmoothing;
            state.imageSmoothing = div > 1;
            double s = bw / (double)cw;
            DrawTexture(result, (sx - cx0) * s, (sy - (Height - (cy0 + ch))) * s, sw * s, sh * s, x, y, w, h);
            state.imageSmoothing = smooth;
            if (id > 0) StoreBackdrop(id, div, result, bw, bh, x, y, w, h);
            RenderStats.BackdropCacheMiss++;
            return true;
        }

        void EnsureBackdropLo(int w, int h)
        {
            if (backdropLoA != null && backdropLoW == w && backdropLoH == h) return;
            ReleaseRt(backdropLoA);
            ReleaseRt(backdropLoB);
            backdropLoA = NewRt(w, h, RenderTextureFormat.ARGB32, FilterMode.Bilinear, "Canvas2D.BackdropLoA");
            backdropLoB = NewRt(w, h, RenderTextureFormat.ARGB32, FilterMode.Bilinear, "Canvas2D.BackdropLoB");
            backdropLoW = w;
            backdropLoH = h;
        }

        void StoreBackdrop(int id, int div, RenderTexture src, int w, int h, double x, double y, double rw, double rh)
        {
            int at = 0;
            for (int i = 0; i < backdropSlots.Length; i++)
            {
                if (backdropSlots[i] == null || backdropSlots[i].id == id) { at = i; break; }
                at = i;
            }
            if (backdropSlots[at] != null && (backdropSlots[at].w != w || backdropSlots[at].h != h))
            {
                ReleaseRt(backdropSlots[at].tex);
                backdropSlots[at].tex = null;
            }
            if (backdropSlots[at] == null) backdropSlots[at] = new BackdropSlot();
            var slot = backdropSlots[at];
            if (slot.tex == null) slot.tex = NewRt(w, h, RenderTextureFormat.ARGB32, div > 1 ? FilterMode.Bilinear : FilterMode.Point, "Canvas2D.BackdropCache");
            slot.id = id;
            slot.div = div;
            slot.w = w;
            slot.h = h;
            slot.x = x;
            slot.y = y;
            slot.rw = rw;
            slot.rh = rh;
            cmd.CopyTexture(src, slot.tex);
        }

        /// <summary>Clear one canvas-pixel rect (y down) without touching the rest of the target.</summary>
        public void ClearDirty(int x0, int y0, int x1, int y1)
        {
            if (x1 <= x0 || y1 <= y0) return;
            ClearKeepRect(x0, y0, x1, y1);
        }

        /// <summary>Scissor in canvas pixels, y down. A zero rect disables it.</summary>
        public void SetScissorPx(int x0, int y0, int x1, int y1)
        {
            if (x1 <= x0 || y1 <= y0)
            {
                ScissorOn = false;
                if (scissorLive)
                {
                    cmd.DisableScissorRect();
                    scissorLive = false;
                }
                return;
            }
            ScissorOn = true;
            ScissorX = x0;
            ScissorY = Height - y1;
            ScissorW = x1 - x0;
            ScissorH = y1 - y0;
            cmd.EnableScissorRect(new Rect(ScissorX, ScissorY, ScissorW, ScissorH));
            scissorLive = true;
        }

        /// <summary>
        /// One-quad gaussian of an axis-aligned rounded rect. Returns false when the option is off,
        /// the transform is rotated, or the shader did not load, so the caller keeps the mask blur.
        /// </summary>
        public bool TryAnalyticInset(double x, double y, double w, double h, Vec2d[] radii, Rgba color, double blurCss)
        {
            if (!RenderOpts.RoundPlus)
            {
                if (RenderOpts.RoundShadow) RenderStats.RoundMissInset++;
                return false;
            }
            return TryRoundRectShadow(x, y, w, h, radii, color, blurCss, true);
        }

        public bool TryRoundRectShadow(double x, double y, double w, double h, Vec2d[] radii, Rgba color, double blurCss)
        {
            return TryRoundRectShadow(x, y, w, h, radii, color, blurCss, false);
        }

        bool TryRoundRectShadow(double x, double y, double w, double h, Vec2d[] radii, Rgba color, double blurCss, bool inset)
        {
            bool want = inset ? RenderOpts.RoundPlus : RenderOpts.RoundShadow || RenderOpts.RoundPlus;
            if (!want || w <= 0 || h <= 0 || blurCss <= 0 || color.a <= 0) return false;
            var m = state.ctm;
            bool rotated = Math.Abs(m.b) > 1e-3 || Math.Abs(m.c) > 1e-3 || m.a <= 1e-6 || m.d <= 1e-6;
            if (rotated || !m.IsInvertible)
            {
                if (!RenderOpts.RoundPlus || !m.IsInvertible) { RenderStats.RoundMissXform++; return false; }
                return DrawLocalShadow(m, x, y, w, h, radii, color, blurCss, inset);
            }
            if (matRound == null)
            {
                var sh = Resources.Load<Shader>("Canvas2D/RoundShadow");
                if (sh == null) sh = Shader.Find("Hidden/Canvas2D/RoundShadow");
                if (sh == null) { RenderStats.RoundMissShader++; return false; }
                matRound = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            }
            float bx = (float)(m.a * x + m.e);
            float by = (float)(m.d * y + m.f);
            float bw = (float)(m.a * w);
            float bh = (float)(m.d * h);
            float blurScale = (float)Math.Sqrt(m.a * m.a + m.b * m.b);
            float sigma = (float)(blurCss * blurScale * 0.5);
            if (sigma < 0.35f) sigma = 0.35f;
            float pad = sigma * 3f + 2f;
            int x0 = Clamp((int)Math.Floor(bx - pad), 0, Width);
            int y0 = Clamp((int)Math.Floor(by - pad), 0, Height);
            int x1 = Clamp((int)Math.Ceiling(bx + bw + pad), 0, Width);
            int y1 = Clamp((int)Math.Ceiling(by + bh + pad), 0, Height);
            if (x1 <= x0 || y1 <= y0) return false;
            float Rad(int i)
            {
                if (radii == null || i >= radii.Length) return 0f;
                float rx = (float)(radii[i].x * m.a);
                float ry = (float)(radii[i].y * m.d);
                return Math.Max(0f, (rx + ry) * 0.5f);
            }
            int pb = NewParams();
            SetRect(pb, x0, y0, x1, y1);
            prm[pb + IdxAlpha] = new Vector4((float)state.globalAlpha, 0, 0, 0);
            prm[pb + IdxShadow] = new Vector4(color.r, color.g, color.b, color.a);
            prm[pb + IdxGa] = new Vector4(bx, by, bw, bh);
            prm[pb + IdxGb] = new Vector4(Rad(0), Rad(1), Rad(2), Rad(3));
            prm[pb + IdxBlur] = new Vector4(sigma, 0, 0, 0);
            if (inset || state.clip != null)
            {
                prm[pb + IdxFlags] = new Vector4(0, inset ? 1f : 0f, state.clip != null ? 1f : 0f, 0);
                if (state.clip != null) cmd.SetGlobalTexture(IdClipTex, state.clip);
            }
            cmd.SetRenderTarget(target);
            if (ScissorOn) EnableLiveScissor();
            Draw(matRound, 0, 6, pb);
            RenderStats.RoundShadow++;
            return true;
        }

        bool DrawLocalShadow(Matrix2D m, double x, double y, double w, double h, Vec2d[] radii, Rgba color, double blurCss, bool inset)
        {
            if (matRound == null)
            {
                var sh = Resources.Load<Shader>("Canvas2D/RoundShadow");
                if (sh == null) sh = Shader.Find("Hidden/Canvas2D/RoundShadow");
                if (sh == null) { RenderStats.RoundMissShader++; return false; }
                matRound = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            }
            float sigma = (float)(blurCss * 0.5);
            if (sigma < 0.2f) sigma = 0.2f;
            float pad = sigma * 3f + 2f;
            float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                double lx = (i & 1) == 0 ? x - pad : x + w + pad;
                double ly = (i & 2) == 0 ? y - pad : y + h + pad;
                var p = m.Apply(lx, ly);
                if (p.x < minX) minX = (float)p.x;
                if (p.y < minY) minY = (float)p.y;
                if (p.x > maxX) maxX = (float)p.x;
                if (p.y > maxY) maxY = (float)p.y;
            }
            int x0 = Clamp((int)Math.Floor(minX), 0, Width);
            int y0 = Clamp((int)Math.Floor(minY), 0, Height);
            int x1 = Clamp((int)Math.Ceiling(maxX), 0, Width);
            int y1 = Clamp((int)Math.Ceiling(maxY), 0, Height);
            if (x1 <= x0 || y1 <= y0) return false;
            float Rad(int i)
            {
                if (radii == null || i >= radii.Length) return 0f;
                return Math.Max(0f, (float)((radii[i].x + radii[i].y) * 0.5));
            }
            var inv = m.Inverse();
            int pb = NewParams();
            SetRect(pb, x0, y0, x1, y1);
            prm[pb + IdxAlpha] = new Vector4((float)state.globalAlpha, 0, 0, 0);
            prm[pb + IdxShadow] = new Vector4(color.r, color.g, color.b, color.a);
            prm[pb + IdxGa] = new Vector4((float)x, (float)y, (float)w, (float)h);
            prm[pb + IdxGb] = new Vector4(Rad(0), Rad(1), Rad(2), Rad(3));
            prm[pb + IdxBlur] = new Vector4(sigma, 0, 0, 0);
            prm[pb + IdxImgA] = new Vector4((float)inv.a, (float)inv.c, (float)inv.e, 1f);
            prm[pb + IdxImgB] = new Vector4((float)inv.b, (float)inv.d, (float)inv.f, 0f);
            prm[pb + IdxFlags] = new Vector4(0, inset ? 1f : 0f, state.clip != null ? 1f : 0f, 0);
            if (state.clip != null) cmd.SetGlobalTexture(IdClipTex, state.clip);
            cmd.SetRenderTarget(target);
            if (ScissorOn) EnableLiveScissor();
            Draw(matRound, 0, 6, pb);
            RenderStats.RoundShadow++;
            return true;
        }

        bool EnsureFast()
        {
            if (matFast != null) return true;
            var sh = Resources.Load<Shader>("Canvas2D/FastShape");
            if (sh == null) sh = Shader.Find("Hidden/Canvas2D/FastShape");
            if (sh == null) return false;
            matFast = new Material(sh) { hideFlags = HideFlags.HideAndDontSave };
            return true;
        }

        bool AxisScale(out Matrix2D m)
        {
            m = state.ctm;
            return Math.Abs(m.b) <= 1e-3 && Math.Abs(m.c) <= 1e-3 && m.a > 1e-6 && m.d > 1e-6;
        }

        bool FastSimilarity(out Matrix2D m)
        {
            m = state.ctm;
            double l0 = m.a * m.a + m.b * m.b;
            double l1 = m.c * m.c + m.d * m.d;
            double dot = m.a * m.c + m.b * m.d;
            if (l0 < 1e-8 || m.Determinant <= 1e-8) return false;
            if (Math.Abs(l0 - l1) > 1e-3 * l0) return false;
            if (Math.Abs(dot) > 1e-3 * l0) return false;
            return true;
        }

        bool TryFastFill(PathData src)
        {
            if (src == null || src.Simple == 0 || src.Simple == 5 || state.fillGrad != null) return false;
            if (state.shadowColor.a > 0 && (state.shadowBlur > 0 || state.shadowOffsetX != 0 || state.shadowOffsetY != 0)) return false;
            bool axis = AxisScale(out var m);
            if (!axis)
            {
                if (!RenderOpts.FastArc || (src.Simple != 1 && src.Simple != 2)) return false;
                if (!FastSimilarity(out m)) return false;
            }
            else if (!RenderOpts.FastShape) return false;
            if (!EnsureFast()) return false;
            if (!FastGeom(src, m, false, out int mode, out var ga, out var gb, out int x0, out int y0, out int x1, out int y1)) return false;
            DrawFast(mode, ga, gb, x0, y0, x1, y1, state.fillColor);
            if (src.Simple == 1) RenderStats.FastCirc++;
            else if (src.Simple == 2) RenderStats.FastEll++;
            else if (src.Simple == 3) RenderStats.FastRr++;
            else RenderStats.FastRect++;
            return true;
        }

        bool TryFastStroke(PathData src)
        {
            if (src == null) return false;
            bool arc = src.Simple == 5;
            bool ell = src.Simple == 1 || src.Simple == 2;
            if (!arc && !ell) return false;
            if (arc && !RenderOpts.FastArc) return false;
            if (state.strokeGrad != null || state.lineWidth <= 0) return false;
            if (state.lineDash != null && state.lineDash.Length > 0) return false;
            if (state.shadowColor.a > 0 && (state.shadowBlur > 0 || state.shadowOffsetX != 0 || state.shadowOffsetY != 0)) return false;
            bool axis = AxisScale(out var m);
            if (axis)
            {
                if (!arc && !RenderOpts.FastShape) return false;
                if (src.Simple == 2 && Math.Abs(m.a - m.d) > 1e-3 && Math.Abs(src.SimRot) > 1e-3) return false;
            }
            else if (!RenderOpts.FastArc || !FastSimilarity(out m)) return false;
            if (!EnsureFast()) return false;
            if (!FastGeom(src, m, true, out int mode, out var ga, out var gb, out int x0, out int y0, out int x1, out int y1)) return false;
            DrawFast(mode, ga, gb, x0, y0, x1, y1, state.strokeColor);
            if (arc) RenderStats.FastArc++;
            else RenderStats.FastLine++;
            return true;
        }

        bool FastGeom(PathData src, Matrix2D m, bool stroke, out int mode, out Vector4 ga, out Vector4 gb, out int x0, out int y0, out int x1, out int y1)
        {
            mode = 0;
            ga = gb = default;
            x0 = y0 = x1 = y1 = 0;
            float half = stroke ? (float)(state.lineWidth * m.MaxScale() * 0.5) : 0f;
            if (src.Simple == 4)
            {
                float bx = (float)(m.a * src.SimX + m.e);
                float by = (float)(m.d * src.SimY + m.f);
                float bw = (float)(m.a * src.SimW);
                float bh = (float)(m.d * src.SimH);
                if (bw < 0) { bx += bw; bw = -bw; }
                if (bh < 0) { by += bh; bh = -bh; }
                mode = 0;
                x0 = Clamp((int)Math.Floor(bx), 0, Width);
                y0 = Clamp((int)Math.Floor(by), 0, Height);
                x1 = Clamp((int)Math.Ceiling(bx + bw), 0, Width);
                y1 = Clamp((int)Math.Ceiling(by + bh), 0, Height);
                return x1 > x0 && y1 > y0;
            }
            if (src.Simple == 1 || src.Simple == 2 || src.Simple == 5)
            {
                if (src.Simple == 2 && Math.Abs(m.b) <= 1e-3 && Math.Abs(m.c) <= 1e-3 && Math.Abs(m.a - m.d) > 1e-3 && Math.Abs(src.SimRot) > 1e-3) return false;
                bool turned = Math.Abs(m.b) > 1e-3 || Math.Abs(m.c) > 1e-3;
                float cx = (float)(m.a * src.SimX + m.c * src.SimY + m.e);
                float cy = (float)(m.b * src.SimX + m.d * src.SimY + m.f);
                float rx, ry, rot;
                if (turned)
                {
                    float scale = (float)Math.Sqrt(m.a * m.a + m.b * m.b);
                    rx = Math.Abs((float)src.SimRx * scale);
                    ry = Math.Abs((float)src.SimRy * scale);
                    rot = (float)(src.SimRot + Math.Atan2(m.b, m.a));
                }
                else
                {
                    rx = Math.Abs((float)(src.SimRx * m.a));
                    ry = Math.Abs((float)(src.SimRy * m.d));
                    rot = (float)src.SimRot;
                }
                float padR = Math.Max(rx, ry);
                if (src.Simple == 5)
                {
                    float hw = half;
                    mode = 5;
                    ga = new Vector4(cx, cy, rx, rx);
                    float a0 = turned ? (float)(src.SimA0 + Math.Atan2(m.b, m.a)) : (float)src.SimA0;
                    gb = new Vector4(a0, (float)src.SimSweep, hw, (float)state.lineCap);
                    padR = rx + hw + 1f;
                    x0 = Clamp((int)Math.Floor(cx - padR), 0, Width);
                    y0 = Clamp((int)Math.Floor(cy - padR), 0, Height);
                    x1 = Clamp((int)Math.Ceiling(cx + padR), 0, Width);
                    y1 = Clamp((int)Math.Ceiling(cy + padR), 0, Height);
                    return x1 > x0 && y1 > y0 && rx > 0.2f && hw > 0f;
                }
                mode = stroke ? 4 : 1;
                ga = new Vector4(cx, cy, rx, ry);
                gb = new Vector4(rot, half, 0, 0);
                float pad = padR + half + 1f;
                x0 = Clamp((int)Math.Floor(cx - pad), 0, Width);
                y0 = Clamp((int)Math.Floor(cy - pad), 0, Height);
                x1 = Clamp((int)Math.Ceiling(cx + pad), 0, Width);
                y1 = Clamp((int)Math.Ceiling(cy + pad), 0, Height);
                return x1 > x0 && y1 > y0 && rx > 0.2f && ry > 0.2f;
            }
            if (src.Simple == 3 && !stroke)
            {
                float bx = (float)(m.a * src.SimX + m.e);
                float by = (float)(m.d * src.SimY + m.f);
                float bw = Math.Abs((float)(m.a * src.SimW));
                float bh = Math.Abs((float)(m.d * src.SimH));
                float s = Math.Abs((float)m.a);
                mode = 2;
                ga = new Vector4(bx, by, bw, bh);
                gb = new Vector4((float)src.SimR0 * s, (float)src.SimR1 * s, (float)src.SimR2 * s, (float)src.SimR3 * s);
                x0 = Clamp((int)Math.Floor(bx), 0, Width);
                y0 = Clamp((int)Math.Floor(by), 0, Height);
                x1 = Clamp((int)Math.Ceiling(bx + bw), 0, Width);
                y1 = Clamp((int)Math.Ceiling(by + bh), 0, Height);
                return x1 > x0 && y1 > y0;
            }
            return false;
        }

        void DrawFast(int mode, Vector4 ga, Vector4 gb, int x0, int y0, int x1, int y1, Rgba color)
        {
            int pb = NewParams();
            SetRect(pb, x0, y0, x1, y1);
            prm[pb + IdxAlpha] = new Vector4((float)state.globalAlpha, 0, 0, 0);
            prm[pb + IdxColor] = new Vector4(color.r, color.g, color.b, color.a);
            prm[pb + IdxFlags] = new Vector4(mode, 0, state.clip != null ? 1f : 0f, 0);
            prm[pb + IdxGa] = ga;
            prm[pb + IdxGb] = gb;
            if (state.clip != null) cmd.SetGlobalTexture(IdClipTex, state.clip);
            if (RenderOpts.PlayBatch && state.clip == null && !ScissorOn)
            {
                if (fastN == fastBase.Length) System.Array.Resize(ref fastBase, fastBase.Length * 2);
                fastBase[fastN++] = (uint)pb;
                return;
            }
            cmd.SetRenderTarget(target);
            if (ScissorOn) EnableLiveScissor();
            Draw(matFast, 0, 6, pb);
        }

        /// <summary>
        /// One draw for the analytic quads queued since the previous flush.
        /// Indices append for the whole submit. The GPU buffer is filled once, in
        /// <see cref="UploadFastIndices"/>, before the command buffer runs.
        /// </summary>
        public void FlushFastBatch()
        {
            if (fastN <= fastFlushed || fastFlushing) return;
            fastFlushing = true;
            int start = fastFlushed;
            int n = fastN - start;
            fastFlushed = fastN;
            // Per-draw uniform on the command buffer. One shared value updated with
            // SetGlobalFloat before execute would still be the last batch's offset.
            cmd.SetGlobalFloat(IdFastOffset, start);
            cmd.SetRenderTarget(target);
            if (ScissorOn) EnableLiveScissor();
            EmitDraw(matFast, 1, 6 * n, 1);
            RenderStats.FastBatch++;
            RenderStats.FastBatchInst += n;
            RenderStats.FastBatchOffsetSum += start;
            fastFlushing = false;
        }

        /// <summary>
        /// Copy the whole index list now. A recorded SetBufferData would read this array
        /// again at execute time, so a later batch's rewrite of the same prefix became
        /// every earlier draw on the A11.
        /// </summary>
        internal void UploadFastIndices()
        {
            if (fastN <= 0) return;
            if (fastBuf == null || fastBuf.count < fastN)
            {
                fastBuf?.Dispose();
                int cap = 64;
                while (cap < fastN) cap *= 2;
                fastBuf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, cap, 4);
                RenderStats.GpuAlloc++;
            }
            fastBuf.SetData(fastBase, 0, 0, fastN);
            Shader.SetGlobalBuffer(IdFastBase, fastBuf);
        }
    }
}
