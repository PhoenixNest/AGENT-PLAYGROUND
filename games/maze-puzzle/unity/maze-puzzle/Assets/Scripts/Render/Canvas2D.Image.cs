using UnityEngine;

namespace OrbitalSalvage.Render
{
    public sealed class CanvasImage
    {
        public readonly Texture texture;
        public readonly int width, height;

        public CanvasImage(Texture texture)
        {
            this.texture = texture;
            width = texture.width;
            height = texture.height;
        }
    }

    public sealed partial class Canvas2D
    {
        static readonly int IdImage = Shader.PropertyToID("_C2D_Image");
        Material matImage;

        public void DrawImage(CanvasImage img, double dx, double dy) { DrawImage(img, 0, 0, img.width, img.height, dx, dy, img.width, img.height); }
        public void DrawImage(CanvasImage img, double dx, double dy, double dw, double dh) { DrawImage(img, 0, 0, img.width, img.height, dx, dy, dw, dh); }

        public void DrawImage(CanvasImage img, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh)
        {
            if (img == null) return;
            DrawTexture(img.texture, sx, sy, sw, sh, dx, dy, dw, dh);
        }

        public void DrawTexture(Texture tex, double sx, double sy, double sw, double sh, double dx, double dy, double dw, double dh)
        {
            if (tex == null || !state.ctm.IsInvertible || state.globalAlpha <= 0) return;
            if (!Finite(sx, sy) || !Finite(sw, sh) || !Finite(dx, dy) || !Finite(dw, dh) || sw == 0 || sh == 0 || dw == 0 || dh == 0) return;
            if (sw < 0) { sx += sw; sw = -sw; }
            if (sh < 0) { sy += sh; sh = -sh; }
            if (dw < 0) { dx += dw; dw = -dw; }
            if (dh < 0) { dy += dh; dh = -dh; }
            if (matImage == null) matImage = MakeMaterial("Hidden/Canvas2D/Image");

            tmpPath.Reset();
            tmpPath.Rect(state.ctm, dx, dy, dw, dh);
            BuildFillPolygons(tmpPath, polys);
            if (!EmitEdges(polys, 0, 0, out var rect, out int ebase, out int ecount, false)) return;

            int pb = NewParams();
            prm[pb + IdxAlpha] = new Vector4((float)state.globalAlpha, 0, 0, 0);
            prm[pb + IdxFlags] = new Vector4(0, 0, 0, 0);
            BindClip(pb);
            var inv = state.ctm.Inverse();
            prm[pb + IdxImgA] = new Vector4((float)inv.a, (float)inv.c, (float)inv.e, state.imageSmoothing ? 1f : 0f);
            prm[pb + IdxImgB] = new Vector4((float)inv.b, (float)inv.d, (float)inv.f, 0);
            prm[pb + IdxImgC] = new Vector4((float)sx, (float)sy, (float)sw, (float)sh);
            prm[pb + IdxImgD] = new Vector4((float)dx, (float)dy, (float)dw, (float)dh);
            AccumulateTo(pb, rect, ebase, ecount);
            SetTarget(target);
            cmd.SetGlobalTexture(IdImage, tex);
            Draw(matImage, 0, 6, pb);
        }
    }
}
