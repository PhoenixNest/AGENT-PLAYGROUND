using System.Collections.Generic;
using UnityEngine;

namespace OrbitalSalvage.Parity
{
    public sealed class ParityRenderRig
    {
        public const int UiLayer = 5;

        public RenderTexture Target { get; private set; }
        public Camera UiCamera { get; private set; }
        public int Width => ParityProfiles.PixelW;
        public int Height => ParityProfiles.PixelH;

        readonly List<Transform> transformBuffer = new List<Transform>(2048);
        Texture2D readTexture;
        GameObject cameraObject;

        public void Create()
        {
            Target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default)
            {
                name = "ParityTarget",
                antiAliasing = 1,
                filterMode = FilterMode.Point
            };
            Target.Create();

            cameraObject = new GameObject("ParityUICamera");
            Object.DontDestroyOnLoad(cameraObject);
            UiCamera = cameraObject.AddComponent<Camera>();
            UiCamera.orthographic = true;
            UiCamera.orthographicSize = Height * 0.5f;
            UiCamera.clearFlags = CameraClearFlags.Depth;
            UiCamera.cullingMask = 1 << UiLayer;
            UiCamera.depth = 1000f;
            UiCamera.nearClipPlane = 0.1f;
            UiCamera.farClipPlane = 1000f;
            UiCamera.allowHDR = false;
            UiCamera.allowMSAA = false;
            UiCamera.useOcclusionCulling = false;
            UiCamera.targetTexture = Target;
        }

        public void Refresh()
        {
            if (Target == null) return;

            var cameras = Camera.allCameras;
            for (int i = 0; i < cameras.Length; i++)
            {
                var cam = cameras[i];
                if (cam == UiCamera) continue;
                if (cam.targetTexture != Target) cam.targetTexture = Target;
                int mask = cam.cullingMask & ~(1 << UiLayer);
                if (cam.cullingMask != mask) cam.cullingMask = mask;
            }

            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                var canvas = canvases[i];
                if (!canvas.isRootCanvas) continue;
                if (canvas.renderMode != RenderMode.ScreenSpaceCamera) canvas.renderMode = RenderMode.ScreenSpaceCamera;
                if (canvas.worldCamera != UiCamera) canvas.worldCamera = UiCamera;
                if (!Mathf.Approximately(canvas.planeDistance, 10f)) canvas.planeDistance = 10f;

                transformBuffer.Clear();
                canvas.GetComponentsInChildren(true, transformBuffer);
                for (int t = 0; t < transformBuffer.Count; t++)
                {
                    var go = transformBuffer[t].gameObject;
                    if (go.layer != UiLayer) go.layer = UiLayer;
                }
            }
        }

        public Vector2 ScreenToCss(Vector2 pixel)
        {
            return new Vector2(pixel.x / ParityProfiles.Scale, (Height - pixel.y) / ParityProfiles.Scale);
        }

        public Vector2 CssToScreen(float x, float y)
        {
            return new Vector2(x * ParityProfiles.Scale, Height - y * ParityProfiles.Scale);
        }

        public byte[] ReadPng()
        {
            var previous = RenderTexture.active;
            RenderTexture.active = Target;
            if (readTexture == null) readTexture = new Texture2D(Width, Height, TextureFormat.RGB24, false, true);
            readTexture.ReadPixels(new Rect(0, 0, Width, Height), 0, 0, false);
            readTexture.Apply(false);
            RenderTexture.active = previous;
            return ImageConversion.EncodeToPNG(readTexture);
        }

        public void Dispose()
        {
            if (Target != null) Target.Release();
            if (cameraObject != null) Object.Destroy(cameraObject);
        }
    }
}
