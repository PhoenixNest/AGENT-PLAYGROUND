using System;
using System.Collections.Generic;

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
    /// With ReuseNodes enabled, finished source chains are detached after each quantum and their nodes, parameter
    /// events and buffers are recycled, so steady-state rendering allocates nothing.
    /// </summary>
    public sealed class AudioContextDsp
    {
        int nextNodeId = 1;

        readonly Stack<ParamEvent> eventPool = new Stack<ParamEvent>();
        readonly Stack<OscillatorNode> oscillatorPool = new Stack<OscillatorNode>();
        readonly Stack<GainNode> gainPool = new Stack<GainNode>();
        readonly Stack<BiquadFilterNode> biquadPool = new Stack<BiquadFilterNode>();
        readonly Stack<AudioBufferSourceNode> bufferSourcePool = new Stack<AudioBufferSourceNode>();
        readonly List<AudioBufferDsp> freeBuffers = new List<AudioBufferDsp>();
        readonly List<AudioNodeDsp> liveNodes = new List<AudioNodeDsp>(256);

        public readonly float SampleRate;
        public readonly DestinationNode Destination;
        public IAudioTraceSink Trace;

        internal long QuantumCounter;

        public bool ReuseNodes { get; set; }

        public int ActiveNodeCount
        {
            get { return liveNodes.Count; }
        }

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

        internal ParamEvent AcquireEvent(ParamEventType type, float value, double time, double timeConstant)
        {
            ParamEvent ev = eventPool.Count > 0 ? eventPool.Pop() : new ParamEvent();
            ev.Set(type, value, time, timeConstant);
            return ev;
        }

        internal void ReleaseEvent(ParamEvent ev)
        {
            eventPool.Push(ev);
        }

        public void Prewarm(int oscillators, int gains, int biquads, int bufferSources, int events, int bufferLength)
        {
            for (int i = 0; i < events; i++)
            {
                eventPool.Push(new ParamEvent());
            }

            for (int i = 0; i < oscillators; i++)
            {
                oscillatorPool.Push(new OscillatorNode(this, 0));
            }

            for (int i = 0; i < gains; i++)
            {
                gainPool.Push(new GainNode(this, 0));
            }

            for (int i = 0; i < biquads; i++)
            {
                biquadPool.Push(new BiquadFilterNode(this, 0));
            }

            for (int i = 0; i < bufferSources; i++)
            {
                bufferSourcePool.Push(new AudioBufferSourceNode(this, 0));
                if (bufferLength > 0)
                {
                    freeBuffers.Add(new AudioBufferDsp(1, bufferLength, SampleRate, true));
                }
            }

            for (int i = 0; i < 4; i++)
            {
                PeriodicWave.Get(SampleRate, (OscillatorType)i);
            }
        }

        void Track(AudioNodeDsp node)
        {
            if (ReuseNodes)
            {
                liveNodes.Add(node);
            }
        }

        public OscillatorNode CreateOscillator()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "oscillator");
            OscillatorNode node;
            if (ReuseNodes && oscillatorPool.Count > 0)
            {
                node = oscillatorPool.Pop();
                node.ResetForReuse(id);
            }
            else
            {
                node = new OscillatorNode(this, id);
            }

            Track(node);
            return node;
        }

        public GainNode CreateGain()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "gain");
            GainNode node;
            if (ReuseNodes && gainPool.Count > 0)
            {
                node = gainPool.Pop();
                node.ResetForReuse(id);
            }
            else
            {
                node = new GainNode(this, id);
            }

            Track(node);
            return node;
        }

        public BiquadFilterNode CreateBiquadFilter()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "biquad");
            BiquadFilterNode node;
            if (ReuseNodes && biquadPool.Count > 0)
            {
                node = biquadPool.Pop();
                node.ResetForReuse(id);
            }
            else
            {
                node = new BiquadFilterNode(this, id);
            }

            Track(node);
            return node;
        }

        public AudioBufferSourceNode CreateBufferSource()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "bufferSource");
            AudioBufferSourceNode node;
            if (ReuseNodes && bufferSourcePool.Count > 0)
            {
                node = bufferSourcePool.Pop();
                node.ResetForReuse(id);
            }
            else
            {
                node = new AudioBufferSourceNode(this, id);
            }

            Track(node);
            return node;
        }

        public DynamicsCompressorNode CreateDynamicsCompressor()
        {
            int id = nextNodeId++;
            Trace?.Event("node.create", id, "compressor");
            return new DynamicsCompressorNode(this, id);
        }

        public AudioBufferDsp CreateBuffer(int channels, int length, float sampleRate)
        {
            return new AudioBufferDsp(channels, length, sampleRate, false);
        }

        public AudioBufferDsp AcquireBuffer(int channels, int length, float sampleRate)
        {
            for (int i = freeBuffers.Count - 1; i >= 0; i--)
            {
                AudioBufferDsp b = freeBuffers[i];
                if (b.NumberOfChannels == channels && b.Length == length && b.SampleRate == sampleRate)
                {
                    freeBuffers.RemoveAt(i);
                    return b;
                }
            }

            return new AudioBufferDsp(channels, length, sampleRate, true);
        }

        internal void ReleaseBuffer(AudioBufferDsp buffer)
        {
            if (buffer.Pooled)
            {
                freeBuffers.Add(buffer);
            }
        }

        internal void ReturnToPool(AudioNodeDsp node)
        {
            OscillatorNode osc = node as OscillatorNode;
            if (osc != null)
            {
                oscillatorPool.Push(osc);
                return;
            }

            GainNode gain = node as GainNode;
            if (gain != null)
            {
                gainPool.Push(gain);
                return;
            }

            BiquadFilterNode biquad = node as BiquadFilterNode;
            if (biquad != null)
            {
                biquadPool.Push(biquad);
                return;
            }

            AudioBufferSourceNode source = node as AudioBufferSourceNode;
            if (source != null)
            {
                bufferSourcePool.Push(source);
            }
        }

        void ReclaimFinishedNodes()
        {
            for (int i = liveNodes.Count - 1; i >= 0; i--)
            {
                AudioNodeDsp node = liveNodes[i];
                if (!node.CanReclaim())
                {
                    continue;
                }

                node.Detach();
                int last = liveNodes.Count - 1;
                liveNodes[i] = liveNodes[last];
                liveNodes.RemoveAt(last);
                node.ReleaseResources();
                ReturnToPool(node);
            }
        }

        public void RenderQuantum(float[] left, float[] right, int offset)
        {
            QuantumCounter++;
            Destination.ProcessIfNecessary();
            Array.Copy(Destination.Out.Channels[0], 0, left, offset, AudioMath.RenderQuantum);
            Array.Copy(Destination.Out.Channels[1], 0, right, offset, AudioMath.RenderQuantum);
            CurrentSampleFrame += AudioMath.RenderQuantum;
            if (ReuseNodes)
            {
                ReclaimFinishedNodes();
            }
        }
    }
}