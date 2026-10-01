using System;

namespace OrbitalSalvage.AudioDsp
{
    public sealed class AudioBufferDsp
    {
        public readonly float[][] Data;
        public readonly float SampleRate;
        public readonly int Length;

        public int NumberOfChannels
        {
            get { return Data.Length; }
        }

        public double Duration
        {
            get { return Length / (double)SampleRate; }
        }

        public AudioBufferDsp(int channels, int length, float sampleRate)
        {
            Data = new float[channels][];
            for (int c = 0; c < channels; c++)
            {
                Data[c] = new float[length];
            }

            Length = length;
            SampleRate = sampleRate;
        }

        public float[] GetChannelData(int channel)
        {
            return Data[channel];
        }
    }

    /// <summary>
    /// Port of Chromium AudioBufferSourceHandler (modules/webaudio/audio_buffer_source_handler.cc).
    /// </summary>
    public sealed class AudioBufferSourceNode : AudioScheduledSourceNode
    {
        const double MaxRate = 1024.0;

        AudioBufferDsp buffer;
        double virtualReadIndex;
        double bufferPlayedFrames;
        double virtualLoopStartFrame;
        double virtualLoopEndFrame;
        bool isLooping;
        double loopStart;
        double loopEnd;
        double grainOffset;
        double grainDuration = double.PositiveInfinity;
        bool isDurationGiven;

        public readonly AudioParam PlaybackRate;
        public readonly AudioParam Detune;

        public AudioBufferDsp Buffer
        {
            get { return buffer; }
            set
            {
                if (value != null)
                {
                    Ctx.Trace?.Event("node.set", Id, "buffer", value.Length, (double)value.SampleRate, value.NumberOfChannels);
                }

                Ctx.Mutate(() =>
                {
                    buffer = value;
                    if (value != null)
                    {
                        Out.SetChannelCount(value.NumberOfChannels);
                    }

                    virtualReadIndex = 0;
                    bufferPlayedFrames = 0;
                    UpdateEffectiveLoopPoints();
                    if (State != PlaybackState.Unscheduled && value != null)
                    {
                        ClampGrainParameters();
                    }
                });
            }
        }

        public bool Loop
        {
            get { return isLooping; }
            set
            {
                Ctx.Trace?.Event("node.set", Id, "loop", value ? 1 : 0);
                Ctx.Mutate(() =>
                {
                    isLooping = value;
                    UpdateEffectiveLoopPoints();
                });
            }
        }

        public double LoopStart
        {
            get { return loopStart; }
            set
            {
                Ctx.Mutate(() =>
                {
                    loopStart = value;
                    UpdateEffectiveLoopPoints();
                });
            }
        }

        public double LoopEnd
        {
            get { return loopEnd; }
            set
            {
                Ctx.Mutate(() =>
                {
                    loopEnd = value;
                    UpdateEffectiveLoopPoints();
                });
            }
        }

        internal AudioBufferSourceNode(AudioContextDsp ctx, int id)
            : base(ctx, "bufferSource", id, 1)
        {
            PlaybackRate = new AudioParam(ctx, id, "playbackRate", 1f, -float.MaxValue, float.MaxValue, false);
            Detune = new AudioParam(ctx, id, "detune", 0f, -float.MaxValue, float.MaxValue, false);
        }

        protected override bool PropagatesSilence()
        {
            return !IsPlayingOrScheduled;
        }

        public void Start(double when, double offset, double duration)
        {
            Ctx.Trace?.Event("node.start", Id, when, offset, duration);
            Ctx.Mutate(() =>
            {
                if (State != PlaybackState.Unscheduled)
                {
                    throw new InvalidOperationException("cannot call start more than once");
                }

                isDurationGiven = true;
                grainOffset = offset;
                grainDuration = duration;
                StartTime = Math.Max(when, Ctx.CurrentTime);
                if (buffer != null)
                {
                    ClampGrainParameters();
                }

                State = PlaybackState.Scheduled;
            });
        }

        protected override void OnStart()
        {
            if (buffer != null)
            {
                ClampGrainParameters();
            }
        }

        void ClampGrainParameters()
        {
            double bufferDuration = buffer.Duration;
            grainOffset = Math.Min(Math.Max(grainOffset, 0.0), bufferDuration);

            if (!isDurationGiven)
            {
                grainDuration = double.PositiveInfinity;
            }
            else
            {
                grainDuration = Math.Max(grainDuration, 0.0);
            }

            if (PlaybackRate.Value == 1.0 && Detune.Value == 0.0)
            {
                virtualReadIndex = AudioMath.TimeToSampleFrameNearest(grainOffset, buffer.SampleRate);
            }
            else
            {
                virtualReadIndex = grainOffset * buffer.SampleRate;
            }
        }

        void UpdateEffectiveLoopPoints()
        {
            if (buffer == null)
            {
                virtualLoopStartFrame = 0.0;
                virtualLoopEndFrame = 0.0;
                return;
            }

            double bufferDuration = buffer.Duration;
            double sampleRate = buffer.SampleRate;
            double length = buffer.Length;

            virtualLoopStartFrame = 0.0;
            virtualLoopEndFrame = length;

            if (!isLooping)
            {
                return;
            }

            double start = Math.Min(Math.Max(loopStart, 0.0), bufferDuration);
            double end = loopEnd == 0.0 ? bufferDuration : Math.Min(Math.Max(loopEnd, 0.0), bufferDuration);

            if (start < end)
            {
                if (start > 0.0)
                {
                    virtualLoopStartFrame = start * sampleRate;
                }

                if (end < bufferDuration)
                {
                    virtualLoopEndFrame = end * sampleRate;
                }
            }

            if (virtualLoopStartFrame >= virtualLoopEndFrame)
            {
                virtualLoopStartFrame = 0.0;
                virtualLoopEndFrame = length;
            }
        }

        double ComputePlaybackRate()
        {
            double sampleRateFactor = buffer != null ? buffer.SampleRate / (double)Ctx.SampleRate : 1.0;
            double baseRate = PlaybackRate.FinalValue();
            double rate = sampleRateFactor * baseRate;
            rate *= Libm.Pow(2, Detune.FinalValue() / 1200);
            return Math.Min(Math.Max(rate, -MaxRate), MaxRate);
        }

        void RenderSilenceAndFinish(int index, int frames)
        {
            if (frames > 0)
            {
                for (int c = 0; c < Out.ChannelCount; c++)
                {
                    Array.Clear(Out.Channels[c], index, frames);
                }
            }

            Finish();
        }

        protected override void Process()
        {
            if (buffer == null)
            {
                Out.Zero();
                if (State != PlaybackState.Unscheduled)
                {
                    Finish();
                }

                return;
            }

            if (Out.ChannelCount != buffer.NumberOfChannels)
            {
                Out.Zero();
                return;
            }

            int quantumFrameOffset;
            int bufferFramesToProcess;
            double startTimeOffset;
            UpdateSchedulingInfo(Out, out quantumFrameOffset, out bufferFramesToProcess, out startTimeOffset);

            if (bufferFramesToProcess == 0)
            {
                Out.Zero();
                return;
            }

            if (!RenderFromBuffer(quantumFrameOffset, bufferFramesToProcess, startTimeOffset))
            {
                Out.Zero();
                return;
            }

            Out.Silent = false;
        }

        bool RenderFromBuffer(int destinationFrameOffset, int numberOfFrames, double startTimeOffset)
        {
            int channels = buffer.NumberOfChannels;

            if (destinationFrameOffset > 0)
            {
                for (int c = 0; c < channels; c++)
                {
                    Array.Clear(Out.Channels[c], 0, destinationFrameOffset);
                }
            }

            int writeIndex = destinationFrameOffset;
            int bufferLength = buffer.Length;

            double virtualStartFrame = virtualLoopStartFrame;
            double virtualEndFrame = virtualLoopEndFrame;
            double virtualDeltaFrames = virtualEndFrame - virtualStartFrame;

            double rate = ComputePlaybackRate();
            if (Math.Abs(rate) > virtualDeltaFrames)
            {
                return false;
            }

            double vri = virtualReadIndex;
            int framesToProcess = numberOfFrames;

            if (rate >= 0)
            {
                if (isLooping && vri >= virtualEndFrame)
                {
                    vri = virtualStartFrame;
                    vri = Math.Min(vri, (double)(bufferLength - 1));
                    virtualReadIndex = vri;
                }

                if (startTimeOffset < 0 && rate != 0)
                {
                    double skipped = Math.Abs(startTimeOffset * rate);
                    vri += skipped;
                    bufferPlayedFrames += skipped;
                }
            }
            else
            {
                if (isLooping && vri < virtualStartFrame)
                {
                    vri = virtualStartFrame;
                    virtualReadIndex = vri;
                }

                if (startTimeOffset < 0)
                {
                    double skipped = Math.Abs(startTimeOffset * rate);
                    vri -= skipped;
                    bufferPlayedFrames += skipped;
                }

                while (framesToProcess > 0 && vri >= bufferLength)
                {
                    for (int c = 0; c < channels; c++)
                    {
                        Out.Channels[c][writeIndex] = 0f;
                    }

                    writeIndex++;
                    vri += rate;
                    framesToProcess--;
                }
            }

            bool isStoppingThisQuantum = false;

            if (isDurationGiven)
            {
                double maxSourceFrames = grainDuration * buffer.SampleRate;
                double sourceFramesLeft = maxSourceFrames - bufferPlayedFrames;
                if (sourceFramesLeft <= 0.0)
                {
                    framesToProcess = 0;
                    isStoppingThisQuantum = true;
                }
                else if (rate != 0.0)
                {
                    double framesUntilLimit = Math.Ceiling(sourceFramesLeft / Math.Abs(rate));
                    if (framesUntilLimit < framesToProcess)
                    {
                        framesToProcess = (int)framesUntilLimit;
                        isStoppingThisQuantum = true;
                    }
                }
            }

            if (!isLooping && ((rate >= 0 && vri >= bufferLength) || (rate < 0 && vri < 0)))
            {
                vri = Math.Min(Math.Max(vri, 0.0), (double)bufferLength);
                framesToProcess = 0;
                isStoppingThisQuantum = true;
            }

            if (rate == 1 && vri == Math.Floor(vri) && virtualDeltaFrames == Math.Floor(virtualDeltaFrames)
                && virtualEndFrame == Math.Floor(virtualEndFrame))
            {
                ProcessFastPath(virtualDeltaFrames, virtualEndFrame, bufferLength, channels, framesToProcess, ref writeIndex, ref vri);
            }
            else
            {
                ProcessInterpolatedPath(virtualStartFrame, virtualDeltaFrames, virtualEndFrame, bufferLength, channels, rate, framesToProcess, ref writeIndex, ref vri);
            }

            if (rate != 0)
            {
                int framesProcessed = writeIndex - destinationFrameOffset;
                bufferPlayedFrames += framesProcessed * Math.Abs(rate);
            }

            if (isStoppingThisQuantum)
            {
                int remaining = destinationFrameOffset + numberOfFrames - writeIndex;
                for (int c = 0; c < channels; c++)
                {
                    if (remaining > 0)
                    {
                        Array.Clear(Out.Channels[c], writeIndex, remaining);
                    }
                }

                Finish();
            }

            virtualReadIndex = vri;
            return true;
        }

        void ProcessFastPath(double virtualDeltaFrames, double virtualEndFrame, int bufferLength, int channels, int framesToProcess, ref int writeIndex, ref double virtualReadIndexRef)
        {
            uint readIndex = (uint)virtualReadIndexRef;
            uint deltaFrames = (uint)virtualDeltaFrames;
            uint endFrame = (uint)virtualEndFrame;

            while (framesToProcess > 0)
            {
                int framesToEnd = (int)(endFrame - readIndex);
                int framesThisTime = Math.Min(Math.Max(framesToEnd, 0), framesToProcess);

                for (int c = 0; c < channels; c++)
                {
                    Array.Copy(buffer.Data[c], (int)readIndex, Out.Channels[c], writeIndex, framesThisTime);
                }

                writeIndex += framesThisTime;
                readIndex += (uint)framesThisTime;
                framesToProcess -= framesThisTime;

                double tempReadIndex = readIndex;

                if (deltaFrames <= 0)
                {
                    Finish();
                    break;
                }

                if (tempReadIndex >= endFrame)
                {
                    if (!isLooping)
                    {
                        RenderSilenceAndFinish(writeIndex, framesToProcess);
                        readIndex = Math.Min(readIndex, (uint)bufferLength);
                        break;
                    }

                    double overflow = tempReadIndex - endFrame;
                    tempReadIndex = endFrame - deltaFrames + FMod(overflow, deltaFrames);
                }

                readIndex = (uint)tempReadIndex;
            }

            virtualReadIndexRef = readIndex;
        }

        static double FMod(double a, double b)
        {
            return a % b;
        }

        void ProcessInterpolatedPath(double virtualStartFrame, double virtualDeltaFrames, double virtualEndFrame, int bufferLength, int channels, double rate, int framesToProcess, ref int writeIndex, ref double vriRef)
        {
            double vri = vriRef;
            uint maxIndex = (uint)(bufferLength - 1);

            for (int i = 0; i < framesToProcess; ++i)
            {
                int framesRemaining = framesToProcess - i - 1;
                uint readIndex = (uint)vri;
                double interpolationFactor = vri - readIndex;
                uint readIndex2 = readIndex + 1;

                if (isLooping && readIndex < virtualEndFrame && readIndex2 >= virtualEndFrame)
                {
                    double wrapped;
                    if (virtualDeltaFrames >= 1.0)
                    {
                        wrapped = vri + 1.0 - virtualDeltaFrames;
                    }
                    else if (virtualDeltaFrames > 0)
                    {
                        wrapped = virtualStartFrame + FMod(vri + 1.0 - virtualStartFrame, virtualDeltaFrames);
                    }
                    else
                    {
                        wrapped = virtualStartFrame;
                    }

                    readIndex2 = wrapped >= 0 ? (uint)wrapped : 0;
                }
                else if (readIndex2 >= bufferLength)
                {
                    readIndex2 = readIndex;
                }

                readIndex = Math.Min(readIndex, maxIndex);
                readIndex2 = Math.Min(readIndex2, maxIndex);

                for (int c = 0; c < channels; c++)
                {
                    float[] source = buffer.Data[c];
                    double sample;
                    if (readIndex == readIndex2 && readIndex >= 1)
                    {
                        double s1 = source[readIndex - 1];
                        double s2 = source[readIndex];
                        sample = s2 + (s2 - s1) * interpolationFactor;
                    }
                    else
                    {
                        double s1 = source[readIndex];
                        double s2 = source[readIndex2];
                        sample = (1.0 - interpolationFactor) * s1 + interpolationFactor * s2;
                    }

                    Out.Channels[c][writeIndex] = (float)Math.Min(Math.Max(sample, -float.MaxValue), float.MaxValue);
                }

                ++writeIndex;
                vri += rate;

                if (virtualDeltaFrames <= 0)
                {
                    Finish();
                    break;
                }

                if (rate >= 0 && vri >= virtualEndFrame)
                {
                    if (!isLooping)
                    {
                        RenderSilenceAndFinish(writeIndex, framesRemaining);
                        vri = Math.Min(vri, (double)bufferLength);
                        break;
                    }

                    double overflow = vri - virtualEndFrame;
                    vri = virtualEndFrame - virtualDeltaFrames + FMod(overflow, virtualDeltaFrames);
                }
                else if (rate < 0 && vri < virtualStartFrame)
                {
                    if (!isLooping)
                    {
                        RenderSilenceAndFinish(writeIndex, framesRemaining);
                        vri = Math.Max(0.0, vri);
                        break;
                    }

                    double overflow = virtualStartFrame - vri;
                    vri = virtualStartFrame + virtualDeltaFrames - FMod(overflow, virtualDeltaFrames);
                }
            }

            vriRef = vri;
        }
    }
}
