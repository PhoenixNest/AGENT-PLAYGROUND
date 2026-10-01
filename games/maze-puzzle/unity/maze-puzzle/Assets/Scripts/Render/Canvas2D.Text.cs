namespace OrbitalSalvage.Render
{
    public sealed partial class Canvas2D
    {
        public string Font { set { if (!string.IsNullOrWhiteSpace(value)) state.font = value; } }
        public string TextAlign { set { state.textAlign = value; } }
        public string TextBaseline { set { state.textBaseline = value; } }

        void InitText() { }
        void DisposeText() { }
        void BeginFrameText() { }
        void UploadText() { }

        public void FillText(string text, double x, double y, double maxWidth = double.NaN) { }
        public void StrokeText(string text, double x, double y, double maxWidth = double.NaN) { }
    }
}
