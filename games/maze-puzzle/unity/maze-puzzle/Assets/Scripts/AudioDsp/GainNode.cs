namespace OrbitalSalvage.AudioDsp
{
    /// <summary>
    /// Port of Chromium GainHandler (modules/webaudio/gain_handler.cc).
    /// </summary>
    public sealed class GainNode : AudioNodeDsp
    {
        readonly float[] sampleAccurateGains = new float[AudioMath.RenderQuantum];

        public readonly AudioParam Gain;

        internal GainNode(AudioContextDsp ctx, int id)
            : base(ctx, "gain", id, 1)
        {
            Gain = new AudioParam(ctx, id, "gain", 1f, -float.MaxValue, float.MaxValue, true);
        }

        internal void ResetForReuse(int id)
        {
            ResetBase(id);
            Gain.ResetForReuse(id);
        }

        protected override bool OutputFollowsInput
        {
            get { return true; }
        }

        protected override void ProcessOnlyAudioParams()
        {
            Gain.CalculateSampleAccurateValues(sampleAccurateGains, AudioMath.RenderQuantum);
        }

        protected override void Process()
        {
            Out.SetChannelCount(In.ChannelCount);

            if (Sources.Count == 0)
            {
                Out.Zero();
                return;
            }

            bool sampleAccurate = Gain.HasSampleAccurateValues();
            if (sampleAccurate && Gain.IsAudioRate)
            {
                Gain.CalculateSampleAccurateValues(sampleAccurateGains, AudioMath.RenderQuantum);
                Out.CopyWithSampleAccurateGainFrom(In, sampleAccurateGains);
                return;
            }

            float gain = Gain.FinalValue();
            if (gain == 0f)
            {
                Out.Zero();
            }
            else
            {
                Out.CopyWithGainFrom(In, gain);
            }
        }
    }
}
