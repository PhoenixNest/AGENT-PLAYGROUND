using System;
using System.Collections.Generic;

namespace OrbitalSalvage.AudioDsp
{
    public enum ChannelCountMode
    {
        Max,
        ClampedMax,
        Explicit,
    }

    public abstract class AudioNodeDsp
    {
        internal readonly AudioContextDsp Ctx;
        internal readonly List<AudioNodeDsp> Sources = new List<AudioNodeDsp>();
        internal AudioBus In;
        internal AudioBus Out;
        long lastQuantum = -1;
        protected double LastNonSilentTime;

        public readonly int Id;
        public readonly string Kind;

        protected virtual bool HasInput
        {
            get { return true; }
        }

        protected virtual bool OutputFollowsInput
        {
            get { return false; }
        }

        protected virtual ChannelCountMode InputMode
        {
            get { return ChannelCountMode.Max; }
        }

        protected virtual int InputChannelCount
        {
            get { return 2; }
        }

        protected virtual double TailTime
        {
            get { return 0; }
        }

        protected virtual double LatencyTime
        {
            get { return 0; }
        }

        protected AudioNodeDsp(AudioContextDsp ctx, string kind, int id, int outputChannels)
        {
            Ctx = ctx;
            Kind = kind;
            Id = id;
            In = new AudioBus(1, AudioMath.RenderQuantum);
            Out = new AudioBus(outputChannels, AudioMath.RenderQuantum);
        }

        public AudioNodeDsp Connect(AudioNodeDsp destination)
        {
            Ctx.Trace?.Event("node.connect", Id, destination.Id);
            Ctx.Mutate(() => destination.Sources.Add(this));
            return destination;
        }

        internal void ProcessIfNecessary()
        {
            if (lastQuantum == Ctx.QuantumCounter)
            {
                return;
            }

            lastQuantum = Ctx.QuantumCounter;

            bool silentInputs = true;
            if (HasInput)
            {
                PullInputs();
                silentInputs = In.Silent;
                if (OutputFollowsInput)
                {
                    Out.SetChannelCount(In.ChannelCount);
                }
            }

            if (silentInputs && PropagatesSilence())
            {
                Out.Zero();
                ProcessOnlyAudioParams();
            }
            else
            {
                Out.Silent = false;
                Process();
            }

            if (!silentInputs)
            {
                LastNonSilentTime = (Ctx.CurrentSampleFrame + AudioMath.RenderQuantum) / (double)Ctx.SampleRate;
            }
        }

        void PullInputs()
        {
            int maxChannels = 0;
            for (int i = 0; i < Sources.Count; i++)
            {
                AudioNodeDsp s = Sources[i];
                s.ProcessIfNecessary();
                maxChannels = Math.Max(maxChannels, s.Out.ChannelCount);
            }

            int channels;
            if (Sources.Count == 0)
            {
                channels = InputMode == ChannelCountMode.Explicit ? InputChannelCount : 1;
            }
            else if (InputMode == ChannelCountMode.Explicit)
            {
                channels = InputChannelCount;
            }
            else if (InputMode == ChannelCountMode.ClampedMax)
            {
                channels = Math.Min(maxChannels, InputChannelCount);
            }
            else
            {
                channels = maxChannels;
            }

            In.SetChannelCount(channels);
            In.Zero();
            for (int i = 0; i < Sources.Count; i++)
            {
                In.SumFrom(Sources[i].Out);
            }
        }

        protected virtual bool PropagatesSilence()
        {
            return LastNonSilentTime + LatencyTime + TailTime < Ctx.CurrentTime;
        }

        protected virtual void ProcessOnlyAudioParams()
        {
        }

        protected abstract void Process();
    }

    public enum PlaybackState
    {
        Unscheduled,
        Scheduled,
        Playing,
        Finished,
    }

    public abstract class AudioScheduledSourceNode : AudioNodeDsp
    {
        protected const double UnknownTime = -1;

        protected PlaybackState State = PlaybackState.Unscheduled;
        protected double StartTime;
        protected double EndTime = UnknownTime;

        protected override bool HasInput
        {
            get { return false; }
        }

        protected AudioScheduledSourceNode(AudioContextDsp ctx, string kind, int id, int channels)
            : base(ctx, kind, id, channels)
        {
        }

        protected bool IsPlayingOrScheduled
        {
            get { return State == PlaybackState.Playing || State == PlaybackState.Scheduled; }
        }

        public void Start(double when)
        {
            Ctx.Trace?.Event("node.start", Id, when);
            Ctx.Mutate(() =>
            {
                if (State != PlaybackState.Unscheduled)
                {
                    throw new InvalidOperationException("cannot call start more than once");
                }

                StartTime = Math.Max(when, Ctx.CurrentTime);
                OnStart();
                State = PlaybackState.Scheduled;
            });
        }

        public void Stop(double when)
        {
            Ctx.Trace?.Event("node.stop", Id, when);
            Ctx.Mutate(() =>
            {
                if (State == PlaybackState.Unscheduled)
                {
                    throw new InvalidOperationException("cannot call stop without calling start first");
                }

                EndTime = Math.Max(0.0, when);
            });
        }

        protected virtual void OnStart()
        {
        }

        protected void Finish()
        {
            State = PlaybackState.Finished;
        }

        protected void UpdateSchedulingInfo(AudioBus output, out int quantumFrameOffset, out int nonSilentFramesToProcess, out double startFrameOffset)
        {
            quantumFrameOffset = 0;
            nonSilentFramesToProcess = 0;
            startFrameOffset = 0;

            int quantumFrameSize = AudioMath.RenderQuantum;
            double sampleRate = Ctx.SampleRate;
            long quantumStartFrame = Ctx.CurrentSampleFrame;
            long quantumEndFrame = quantumStartFrame + quantumFrameSize;

            long startFrame = AudioMath.TimeToSampleFrameCeil(StartTime, sampleRate);
            long endFrame = 0;
            if (EndTime != UnknownTime)
            {
                endFrame = AudioMath.TimeToSampleFrameCeil(EndTime, sampleRate);
            }

            if (EndTime != UnknownTime && endFrame <= quantumStartFrame)
            {
                Finish();
            }

            if (State == PlaybackState.Unscheduled || State == PlaybackState.Finished || startFrame >= quantumEndFrame)
            {
                output.Zero();
                nonSilentFramesToProcess = 0;
                return;
            }

            if (State == PlaybackState.Scheduled)
            {
                State = PlaybackState.Playing;
                startFrameOffset = StartTime * sampleRate - startFrame;
            }
            else
            {
                startFrameOffset = 0;
            }

            long offset = startFrame > quantumStartFrame ? startFrame - quantumStartFrame : 0;
            quantumFrameOffset = (int)Math.Min(offset, (long)quantumFrameSize);
            nonSilentFramesToProcess = quantumFrameSize - quantumFrameOffset;

            if (nonSilentFramesToProcess == 0)
            {
                output.Zero();
                return;
            }

            if (quantumFrameOffset > 0)
            {
                for (int c = 0; c < output.ChannelCount; c++)
                {
                    Array.Clear(output.Channels[c], 0, quantumFrameOffset);
                }
            }

            if (EndTime != UnknownTime && endFrame >= quantumStartFrame && endFrame <= quantumEndFrame)
            {
                if (endFrame < quantumEndFrame)
                {
                    int zeroStart = (int)(endFrame - quantumStartFrame);
                    int framesToZero = quantumFrameSize - zeroStart;
                    if (framesToZero > nonSilentFramesToProcess)
                    {
                        nonSilentFramesToProcess = 0;
                    }
                    else
                    {
                        nonSilentFramesToProcess -= framesToZero;
                    }

                    for (int c = 0; c < output.ChannelCount; c++)
                    {
                        Array.Clear(output.Channels[c], zeroStart, framesToZero);
                    }
                }

                Finish();
            }
        }
    }
}
