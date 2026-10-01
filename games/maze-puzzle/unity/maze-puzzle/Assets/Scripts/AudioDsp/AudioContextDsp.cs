using System;

namespace OrbitalSalvage.AudioDsp
{
    public enum AudioContextState
    {
        Suspended,
        Running,
    }

    public sealed class DestinationNode : AudioNodeDsp
    {
        protected override ChannelCountMode InputMode
        {
            get { return ChannelCountMode.Explicit; }
        }

        internal DestinationNode(AudioContextDsp ctx)
            : base(ctx, "destination", 0, 2)
        {
        }

        protected override bool PropagatesSilence()
        {
            return false;
        }

        protected override void Process()
        {
            Out.SetChannelCount(2);
            for (int c = 0; c < 2; c++)
            {
                Array.Copy(In.Channels[c], Out.Channels[c], AudioMath.RenderQuantum);
            }

            Out.Silent = In.Silent;
        }
    }

    /// <summary>
    /// Web Audio compatible rendering graph. Not thread-safe: graph edits and rendering must run on one thread.
    /// </summary>
    public sealed class AudioContextDsp
    {
        int nextNodeId = 1;

        public readonly float SampleRate;
        public readonly DestinationNode Destination;
        public IAudioTraceSink Trace;

        internal long QuantumCounter;

        public long CurrentSampleFrame { get; private set; }

        public double CurrentTime
        {
            get { return CurrentSampleFrame / (double)SampleRate; }
        }

        public AudioContextState State { get; private set; } = AudioContextState.Suspended;

        public AudioContextDsp(float sampleRate, IAudioTraceSink trace = null)
        {
            SampleRate = sampleRate;
            Trace = trace;
            Destination = new DestinationNode(this);
            Trace?.Event("ctx.create", (double)sampleRate);
        }

        public void Resume()
        {
            State = AudioContextState.Running;
            Trace?.Event("ctx.resume");
        }

        internal void Mutate(Action action)
        {
            action();
        }

        public OscillatorNode CreateOscillator()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "oscillator");
            return new OscillatorNode(this, id);
        }

        public GainNode CreateGain()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "gain");
            return new GainNode(this, id);
        }

        public BiquadFilterNode CreateBiquadFilter()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "biquad");
            return new BiquadFilterNode(this, id);
        }

        public AudioBufferSourceNode CreateBufferSource()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "bufferSource");
            return new AudioBufferSourceNode(this, id);
        }

        public DynamicsCompressorNode CreateDynamicsCompressor()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "compressor");
            return new DynamicsCompressorNode(this, id);
        }

        public AudioBufferDsp CreateBuffer(int channels, int length, float sampleRate)
        {
            return new AudioBufferDsp(channels, length, sampleRate);
        }

        public void RenderQuantum(float[] left, float[] right, int offset)
        {
            QuantumCounter++;
            Destination.ProcessIfNecessary();
            Array.Copy(Destination.Out.Channels[0], 0, left, offset, AudioMath.RenderQuantum);
            Array.Copy(Destination.Out.Channels[1], 0, right, offset, AudioMath.RenderQuantum);
            CurrentSampleFrame += AudioMath.RenderQuantum;
        }
    }
}
