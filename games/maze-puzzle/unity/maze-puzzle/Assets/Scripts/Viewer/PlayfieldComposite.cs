using System;
using UnityEngine;

namespace OrbitalSalvage.Viewer
{
    /// <summary>
    /// Premultiplied source-over of a playfield under a chrome target whose canvas hole is clear.
    /// The canvas stores premultiplied color (Fill blends One, OneMinusSrcAlpha onto a cleared target).
    /// </summary>
    public static class PlayfieldComposite
    {
        public static byte[] Png(RenderTexture under, RenderTexture over, Func<RenderTexture, byte[]> encode)
        {
            if (over == null) throw new ArgumentNullException(nameof(over));
            if (under == null || under.width != over.width || under.height != over.height)
                return encode(over);
            int w = over.width, h = over.height;
            var a = Read(under);
            var b = Read(over);
            int n = a.Length;
            var dst = new Color32[n];
            for (int i = 0; i < n; i++)
            {
                byte oa = b[i].a;
                int ia = 255 - oa;
                int r = b[i].r + ((a[i].r * ia + 127) / 255);
                int g = b[i].g + ((a[i].g * ia + 127) / 255);
                int bl = b[i].b + ((a[i].b * ia + 127) / 255);
                if (r > 255) r = 255;
                if (g > 255) g = 255;
                if (bl > 255) bl = 255;
                dst[i] = new Color32((byte)r, (byte)g, (byte)bl, 255);
            }
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.SetPixels32(dst);
            tex.Apply(false, false);
            var rt = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = "PlayfieldComposite",
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };
            rt.Create();
            Graphics.CopyTexture(tex, rt);
            UnityEngine.Object.Destroy(tex);
            byte[] png = encode(rt);
            rt.Release();
            UnityEngine.Object.Destroy(rt);
            return png;
        }

        static Color32[] Read(RenderTexture source)
        {
            var rt = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                name = "PlayfieldComposite.Read",
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };
            rt.Create();
            Graphics.CopyTexture(source, rt);
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            tex.Apply(false);
            RenderTexture.active = prev;
            var px = tex.GetPixels32();
            UnityEngine.Object.Destroy(tex);
            rt.Release();
            UnityEngine.Object.Destroy(rt);
            return px;
        }
    }
}
