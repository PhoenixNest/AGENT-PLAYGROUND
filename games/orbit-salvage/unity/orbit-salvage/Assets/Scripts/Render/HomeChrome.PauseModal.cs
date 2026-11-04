using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitalSalvage.Render
{
    public sealed partial class HomeChrome
    {
        // .modal-card is overflow:hidden. The spec has no overflow key, so a square header
        // paints over the rounded corners. Clip descendants after the card's own shadow and fill.
        readonly int[] overflowClipStack = new int[8];
        int overflowClipN;

        Texture2D playfieldBlur;
        RenderTexture blurSrc;
        int blurSrcW, blurSrcH, blurSigmaQ;

        void SyncOverflowClips(int k)
        {
            while (overflowClipN > 0 && !UnderClip(overflowClipStack[overflowClipN - 1], k))
            {
                canvas.Restore();
                overflowClipN--;
            }
        }

        void PushModalCardClip(Dictionary<string, object> e, int k)
        {
            if (overflowClipN >= overflowClipStack.Length) return;
            if (!HasToken(S(e, "c"), "modal-card")) return;
            var rad = Strs(e, "rad");
            if (rad.Count == 0) return;
            var rect = ElementRect(e, k, true);
            double x = rect[0], y = rect[1], w = rect[2], h = rect[3];
            if (w < 1 || h < 1) return;
            var tl = Radius(rad[0], w, h);
            var tr = rad.Count > 1 ? Radius(rad[1], w, h) : tl;
            var br = rad.Count > 2 ? Radius(rad[2], w, h) : tl;
            var bl = rad.Count > 3 ? Radius(rad[3], w, h) : tl;
            if (tl.x <= 0.5 && tl.y <= 0.5 && tr.x <= 0.5 && tr.y <= 0.5
                && br.x <= 0.5 && br.y <= 0.5 && bl.x <= 0.5 && bl.y <= 0.5)
                return;
            canvas.Save();
            canvas.BeginPath();
            canvas.RoundRect(x, y, w, h, tl, tr, br, bl);
            canvas.Clip();
            overflowClipStack[overflowClipN++] = k;
        }

        void ClearOverflowClips()
        {
            while (overflowClipN > 0)
            {
                canvas.Restore();
                overflowClipN--;
            }
        }

        bool UnderClip(int clipK, int k)
        {
            EnsureParents();
            int n = elements.Count;
            for (int p = k, hops = 0; p >= 0 && hops < n; p = parentOf[p], hops++)
                if (p == clipK) return true;
            return false;
        }

        /// <summary>
        /// One-shot blur of <see cref="ExternalPlayfield"/> for a modal overlay. The product draws that
        /// texture under this chrome, so a backdrop readback cannot see it. The game is paused, so the
        /// snapshot is reused until the texture identity or size changes.
        /// </summary>
        bool TryDrawPlayfieldBlur(double x, double y, double w, double h, double sigmaCss, double alpha)
        {
            var src = ExternalPlayfield;
            if (src == null || !src.IsCreated() || sigmaCss <= 0 || alpha <= 0 || w == 0 || h == 0)
            {
                if (src == null) ReleasePlayfieldBlur();
                return false;
            }
            int sigmaQ = Math.Max(1, (int)Math.Round(sigmaCss * 4));
            if (playfieldBlur != null && (blurSrc != src || blurSrcW != src.width || blurSrcH != src.height || blurSigmaQ != sigmaQ))
                ReleasePlayfieldBlur();
            if (playfieldBlur == null)
            {
                if (!BuildPlayfieldBlur(src, sigmaCss)) return false;
                blurSrc = src;
                blurSrcW = src.width;
                blurSrcH = src.height;
                blurSigmaQ = sigmaQ;
                PlayfieldBlurBuilds++;
            }
            canvas.Save();
            canvas.GlobalAlpha *= alpha;
            canvas.SetTransform(scale, 0, 0, scale, 0, 0);
            canvas.ImageSmoothingEnabled = true;
            canvas.DrawTexture(playfieldBlur, 0, 0, playfieldBlur.width, playfieldBlur.height, x, y, w, h);
            canvas.Restore();
            return true;
        }

        bool BuildPlayfieldBlur(RenderTexture src, double sigmaCss)
        {
            int dw = Math.Min(240, src.width);
            int dh = Math.Max(1, (int)((long)src.height * dw / Math.Max(1, src.width)));
            var small = NewBlurTarget(dw, dh);
            Graphics.Blit(src, small);
            var prev = RenderTexture.active;
            RenderTexture.active = small;
            var tex = new Texture2D(dw, dh, TextureFormat.RGBA32, false, true)
            {
                name = "HomeChrome.PlayfieldBlur",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.ReadPixels(new Rect(0, 0, dw, dh), 0, 0, false);
            tex.Apply(false, false);
            RenderTexture.active = prev == small ? null : prev;
            ReleaseBlurTarget(small);
            // The separable shader's texel size stayed zero under Graphics.Blit, so the snapshot stayed sharp.
            double sigmaTex = sigmaCss * dw / Math.Max(1, cssW);
            BlurPixels(tex, BlurRadii(sigmaTex));
            playfieldBlur = tex;
            return true;
        }

        static void BlurPixels(Texture2D tex, int[] radii)
        {
            int w = tex.width, h = tex.height;
            var px = tex.GetPixels32();
            var tmp = new Color32[px.Length];
            for (int pass = 0; pass < radii.Length; pass++)
            {
                int r = radii[pass];
                if (r <= 0) continue;
                BoxH(px, tmp, w, h, r);
                BoxV(tmp, px, w, h, r);
            }
            tex.SetPixels32(px);
            tex.Apply(false, false);
        }

        static void BoxH(Color32[] src, Color32[] dst, int w, int h, int r)
        {
            int span = r * 2 + 1;
            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                int sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                for (int i = -r; i <= r; i++)
                {
                    var c = src[row + Clampi(i, 0, w - 1)];
                    sumR += c.r; sumG += c.g; sumB += c.b; sumA += c.a;
                }
                for (int x = 0; x < w; x++)
                {
                    dst[row + x] = new Color32((byte)(sumR / span), (byte)(sumG / span), (byte)(sumB / span), (byte)(sumA / span));
                    var add = src[row + Clampi(x + r + 1, 0, w - 1)];
                    var rem = src[row + Clampi(x - r, 0, w - 1)];
                    sumR += add.r - rem.r;
                    sumG += add.g - rem.g;
                    sumB += add.b - rem.b;
                    sumA += add.a - rem.a;
                }
            }
        }

        static void BoxV(Color32[] src, Color32[] dst, int w, int h, int r)
        {
            int span = r * 2 + 1;
            for (int x = 0; x < w; x++)
            {
                int sumR = 0, sumG = 0, sumB = 0, sumA = 0;
                for (int i = -r; i <= r; i++)
                {
                    var c = src[Clampi(i, 0, h - 1) * w + x];
                    sumR += c.r; sumG += c.g; sumB += c.b; sumA += c.a;
                }
                for (int y = 0; y < h; y++)
                {
                    dst[y * w + x] = new Color32((byte)(sumR / span), (byte)(sumG / span), (byte)(sumB / span), (byte)(sumA / span));
                    var add = src[Clampi(y + r + 1, 0, h - 1) * w + x];
                    var rem = src[Clampi(y - r, 0, h - 1) * w + x];
                    sumR += add.r - rem.r;
                    sumG += add.g - rem.g;
                    sumB += add.b - rem.b;
                    sumA += add.a - rem.a;
                }
            }
        }

        static int Clampi(int v, int lo, int hi) => v < lo ? lo : (v > hi ? hi : v);

        void ReleasePlayfieldBlur()
        {
            if (playfieldBlur != null) DestroyObject(playfieldBlur);
            playfieldBlur = null;
            blurSrc = null;
        }

        static RenderTexture NewBlurTarget(int w, int h)
        {
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = "HomeChrome.PlayfieldBlur",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                useMipMap = false,
                autoGenerateMips = false,
                hideFlags = HideFlags.HideAndDontSave
            };
            rt.Create();
            return rt;
        }

        static void ReleaseBlurTarget(RenderTexture rt)
        {
            if (rt == null) return;
            if (RenderTexture.active == rt) RenderTexture.active = null;
            rt.Release();
            DestroyObject(rt);
        }

        static int[] BlurRadii(double sigma)
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
    }
}
