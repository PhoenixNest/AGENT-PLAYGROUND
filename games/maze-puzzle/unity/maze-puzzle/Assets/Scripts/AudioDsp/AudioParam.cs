using System;
using System.Collections.Generic;

namespace OrbitalSalvage.AudioDsp
{
    public sealed class AudioParam
    {
        enum EventType
        {
            SetValue,
            LinearRamp,
            ExponentialRamp,
            SetTarget,
            None,
        }

        sealed class ParamEvent
        {
            public EventType Type;
            public float Value;
            public double Time;
            public double TimeConstant;

            public ParamEvent(EventType type, float value, double time, double timeConstant = 0)
            {
                Type = type;
                Value = value;
                Time = time;
                TimeConstant = timeConstant;
            }
        }

        const float TimeConstantsToConverge = 10.0f;
        const float SetTargetThreshold = 4.539992976248485e-05f;

        readonly AudioContextDsp ctx;
        readonly List<ParamEvent> events = new List<ParamEvent>();
        readonly List<ParamEvent> newEvents = new List<ParamEvent>();
        readonly float[] scratch1 = new float[1];
        float intrinsic;

        public readonly string Name;
        public readonly float DefaultValue;
        public readonly float MinValue;
        public readonly float MaxValue;
        public readonly bool IsAudioRate;
        internal int NodeId;

        public float ControlValue { get; private set; }

        public float Value
        {
            get { return intrinsic; }
        }

        internal AudioParam(AudioContextDsp ctx, int nodeId, string name, float defaultValue, float min, float max, bool audioRate)
        {
            this.ctx = ctx;
            NodeId = nodeId;
            Name = name;
            DefaultValue = defaultValue;
            MinValue = min;
            MaxValue = max;
            IsAudioRate = audioRate;
            intrinsic = defaultValue;
            ControlValue = defaultValue;
        }

        public AudioParam SetValueAtTime(double raw, double time)
        {
            ctx.Trace?.Event("param.setValueAtTime", NodeId, Name, raw, time);
            float value = (float)raw;
            ControlValue = value;
            ctx.Mutate(() => InsertEvent(new ParamEvent(EventType.SetValue, value, ClampedToCurrentTime(time)), 0f));
            return this;
        }

        public AudioParam LinearRampToValueAtTime(double raw, double time)
        {
            ctx.Trace?.Event("param.linearRampToValueAtTime", NodeId, Name, raw, time);
            float value = (float)raw;
            float initial = intrinsic;
            ctx.Mutate(() => InsertEvent(new ParamEvent(EventType.LinearRamp, value, ClampedToCurrentTime(time)), initial));
            return this;
        }

        public AudioParam ExponentialRampToValueAtTime(double raw, double time)
        {
            float value = (float)raw;
            if (value == 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(raw), "exponential ramp target must not be zero");
            }

            ctx.Trace?.Event("param.exponentialRampToValueAtTime", NodeId, Name, raw, time);
            float initial = intrinsic;
            ctx.Mutate(() => InsertEvent(new ParamEvent(EventType.ExponentialRamp, value, ClampedToCurrentTime(time)), initial));
            return this;
        }

        public AudioParam SetTargetAtTime(double rawTarget, double time, double timeConstant)
        {
            ctx.Trace?.Event("param.setTargetAtTime", NodeId, Name, rawTarget, time, timeConstant);
            float target = (float)rawTarget;
            ctx.Mutate(() =>
            {
                double t = ClampedToCurrentTime(time);
                if (timeConstant == 0)
                {
                    InsertEvent(new ParamEvent(EventType.SetValue, target, t), 0f);
                }
                else
                {
                    InsertEvent(new ParamEvent(EventType.SetTarget, target, t, timeConstant), 0f);
                }
            });
            return this;
        }

        public AudioParam CancelScheduledValues(double cancelTime)
        {
            ctx.Trace?.Event("param.cancelScheduledValues", NodeId, Name, cancelTime);
            ctx.Mutate(() =>
            {
                double t = ClampedToCurrentTime(cancelTime);
                for (int i = 0; i < events.Count; i++)
                {
                    if (events[i].Time >= t)
                    {
                        events.RemoveRange(i, events.Count - i);
                        newEvents.RemoveAll(e => !events.Contains(e));
                        break;
                    }
                }
            });
            return this;
        }

        double ClampedToCurrentTime(double time)
        {
            return Math.Max(time, ctx.CurrentTime);
        }

        void SetIntrinsic(float value)
        {
            intrinsic = AudioMath.Clamp(value, MinValue, MaxValue);
        }

        void InsertEvent(ParamEvent ev, float initialValue)
        {
            double insertTime = ev.Time;

            int numberOfEvents = events.Count;
            if (numberOfEvents > 0)
            {
                long currentFrame = ctx.CurrentSampleFrame;
                double sr = ctx.SampleRate;
                int skipped = 0;
                for (int i = 0; i < numberOfEvents; i++)
                {
                    ParamEvent thisEvent = events[i];
                    ParamEvent nextEvent = i < numberOfEvents - 1 ? events[i + 1] : null;
                    if (IsEventCurrent(thisEvent, nextEvent, currentFrame, sr))
                    {
                        break;
                    }

                    skipped++;
                }

                if (skipped > 0)
                {
                    RemoveOldEvents(skipped);
                }
            }

            if (events.Count == 0 && (ev.Type == EventType.LinearRamp || ev.Type == EventType.ExponentialRamp))
            {
                ParamEvent seed = new ParamEvent(EventType.SetValue, initialValue, ctx.CurrentTime);
                events.Insert(0, seed);
                newEvents.Add(seed);
            }

            if (events.Count == 0)
            {
                events.Insert(0, ev);
                newEvents.Add(ev);
                return;
            }

            int insertionIndex = events.Count;
            if (events[events.Count - 1].Time > insertTime)
            {
                int lo = 0;
                int hi = events.Count;
                while (lo < hi)
                {
                    int mid = (lo + hi) >> 1;
                    if (insertTime < events[mid].Time)
                    {
                        hi = mid;
                    }
                    else
                    {
                        lo = mid + 1;
                    }
                }

                insertionIndex = lo;
            }

            events.Insert(insertionIndex, ev);
            newEvents.Add(ev);
        }

        void RemoveOldEvents(int count)
        {
            events.RemoveRange(0, count);
        }

        public bool HasSampleAccurateValues()
        {
            int n = events.Count;
            if (n == 0)
            {
                return false;
            }

            long currentFrame = ctx.CurrentSampleFrame;
            double sr = ctx.SampleRate;

            if (events[0].Time > (currentFrame + AudioMath.RenderQuantum) / sr)
            {
                switch (events[0].Type)
                {
                    case EventType.SetTarget:
                    case EventType.SetValue:
                        return false;
                }
            }

            if (n >= 2)
            {
                return true;
            }

            switch (events[0].Type)
            {
                case EventType.SetTarget:
                    return events[0].Time <= (currentFrame + AudioMath.RenderQuantum) / sr;
                default:
                    return !(events[0].Time < currentFrame / sr);
            }
        }

        public float FinalValue()
        {
            float[] v = scratch1;
            v[0] = intrinsic;
            CalculateFinalValues(v, 1, false);
            return v[0];
        }

        internal float ValueNow()
        {
            return FinalValue();
        }

        public void CalculateSampleAccurateValues(float[] values, int count)
        {
            CalculateFinalValues(values, count, IsAudioRate);
        }

        void CalculateFinalValues(float[] values, int count, bool sampleAccurate)
        {
            if (sampleAccurate)
            {
                double sr = ctx.SampleRate;
                long start = ctx.CurrentSampleFrame;
                SetIntrinsic(ValuesForFrameRange(start, start + count, intrinsic, values, count, sr, sr));
            }
            else
            {
                float value = intrinsic;
                if (events.Count > 0 && ctx.CurrentTime >= events[0].Time)
                {
                    double sr = ctx.SampleRate;
                    double controlRate = sr / AudioMath.RenderQuantum;
                    long start = ctx.CurrentSampleFrame;
                    value = ValuesForFrameRange(start, start + 1, value, scratchOne(values), 1, sr, controlRate);
                }

                for (int i = 0; i < count; i++)
                {
                    values[i] = value;
                }

                SetIntrinsic(value);
            }

            float lo = MinValue;
            float hi = MaxValue;
            for (int i = 0; i < count; i++)
            {
                values[i] = AudioMath.Clamp(values[i], lo, hi);
            }
        }

        float[] scratchOne(float[] values)
        {
            return values.Length >= 1 ? values : scratch1;
        }

        float ValuesForFrameRange(long startFrame, long endFrame, float defaultValue, float[] values, int count, double sampleRate, double controlRate)
        {
            ValuesForFrameRangeImpl(startFrame, endFrame, defaultValue, values, count, sampleRate, controlRate);
            for (int i = 0; i < count; i++)
            {
                values[i] = AudioMath.Clamp(values[i], MinValue, MaxValue);
            }

            return values[count - 1];
        }

        static void Fill(float[] values, int from, int to, float v)
        {
            for (int i = from; i < to; i++)
            {
                values[i] = v;
            }
        }

        void ValuesForFrameRangeImpl(long startFrame, long endFrame, float defaultValue, float[] values, int n, double sampleRate, double controlRate)
        {
            if (events.Count == 0 || endFrame / sampleRate <= events[0].Time)
            {
                Fill(values, 0, n, defaultValue);
                return;
            }

            int numberOfEvents = events.Count;

            if (newEvents.Count > 0)
            {
                ClampNewEventsToCurrentTime(startFrame / sampleRate);
            }

            double currentTime = startFrame / sampleRate;
            if (HandleAllEventsInThePast(currentTime, sampleRate, ref defaultValue, values, n))
            {
                return;
            }

            long currentFrame = startFrame;
            int writeIndex = 0;
            HandleFirstEvent(values, n, defaultValue, startFrame, endFrame, sampleRate, ref currentFrame, ref writeIndex);

            float value = defaultValue;
            int skippedEventCount = 0;
            for (int i = 0; i < numberOfEvents && writeIndex < n; ++i)
            {
                ParamEvent ev = events[i];
                ParamEvent nextEvent = i < numberOfEvents - 1 ? events[i + 1] : null;

                if (!IsEventCurrent(ev, nextEvent, currentFrame, sampleRate))
                {
                    skippedEventCount++;
                    continue;
                }

                ev = ProcessSetTargetFollowedByRamp(i, ev, nextEvent != null ? nextEvent.Type : EventType.None, currentFrame, sampleRate, controlRate, ref value);

                float value1 = ev.Value;
                double time1 = ev.Time;
                float value2 = nextEvent != null ? nextEvent.Value : value1;
                double time2 = nextEvent != null ? nextEvent.Time : endFrame / sampleRate + 1;
                EventType nextType = nextEvent != null ? nextEvent.Type : EventType.None;

                long fillToEndFrame = endFrame > time2 * sampleRate ? (long)Math.Ceiling(time2 * sampleRate) : endFrame;
                int fillToFrame = (int)Math.Min(fillToEndFrame - startFrame, (long)n);

                if (nextType == EventType.LinearRamp)
                {
                    ProcessLinearRamp(fillToFrame, time1, time2, value1, value2, sampleRate, values, ref currentFrame, ref value, ref writeIndex);
                }
                else if (nextType == EventType.ExponentialRamp)
                {
                    ProcessExponentialRamp(fillToFrame, time1, time2, value1, value2, sampleRate, values, ref currentFrame, ref value, ref writeIndex);
                }
                else
                {
                    switch (ev.Type)
                    {
                        case EventType.SetValue:
                        case EventType.LinearRamp:
                        case EventType.ExponentialRamp:
                            currentFrame = fillToEndFrame;
                            value = ev.Value;
                            Fill(values, writeIndex, fillToFrame, value);
                            writeIndex = fillToFrame;
                            break;
                        case EventType.SetTarget:
                            ProcessSetTarget(fillToFrame, time1, value1, sampleRate, controlRate, fillToEndFrame, ev, values, ref currentFrame, ref value, ref writeIndex);
                            break;
                    }
                }
            }

            if (skippedEventCount > 0)
            {
                RemoveOldEvents(skippedEventCount);
            }

            Fill(values, writeIndex, n, value);
        }

        void HandleFirstEvent(float[] values, int n, float defaultValue, long startFrame, long endFrame, double sampleRate, ref long currentFrame, ref int writeIndex)
        {
            double firstEventTime = events[0].Time;
            if (firstEventTime > startFrame / sampleRate)
            {
                long fillToEndFrame = endFrame;
                double firstEventFrame = Math.Ceiling(firstEventTime * sampleRate);
                if (endFrame > firstEventFrame)
                {
                    fillToEndFrame = (long)firstEventFrame;
                }

                int fillToFrame = (int)Math.Min(fillToEndFrame - startFrame, (long)n);
                Fill(values, writeIndex, fillToFrame, defaultValue);
                writeIndex = fillToFrame;
                currentFrame += fillToFrame;
            }
        }

        static bool IsEventCurrent(ParamEvent ev, ParamEvent nextEvent, long currentFrame, double sampleRate)
        {
            if (nextEvent == null)
            {
                return true;
            }

            if (nextEvent.Time * sampleRate >= currentFrame)
            {
                return true;
            }

            double eventFrame = ev.Time * sampleRate;
            return ev.Type == EventType.SetValue && eventFrame <= currentFrame && currentFrame < eventFrame + 1;
        }

        void ClampNewEventsToCurrentTime(double currentTime)
        {
            bool clamped = false;
            foreach (ParamEvent e in newEvents)
            {
                if (e.Time < currentTime)
                {
                    e.Time = currentTime;
                    clamped = true;
                }
            }

            if (clamped)
            {
                List<ParamEvent> sorted = new List<ParamEvent>(events.Count);
                foreach (ParamEvent e in events)
                {
                    int at = sorted.Count;
                    while (at > 0 && sorted[at - 1].Time > e.Time)
                    {
                        at--;
                    }

                    sorted.Insert(at, e);
                }

                events.Clear();
                events.AddRange(sorted);
            }

            newEvents.Clear();
        }

        static bool HasSetTargetConverged(float value, float target, double currentTime, double startTime, double timeConstant)
        {
            if (currentTime > startTime + TimeConstantsToConverge * timeConstant)
            {
                return true;
            }

            if (target == 0 && Math.Abs(value) < SetTargetThreshold)
            {
                return true;
            }

            if (target != 0 && Math.Abs(target - value) < SetTargetThreshold * Math.Abs(value))
            {
                return true;
            }

            return false;
        }

        bool HandleAllEventsInThePast(double currentTime, double sampleRate, ref float defaultValue, float[] values, int n)
        {
            ParamEvent last = events[events.Count - 1];
            if (last.Time + 1.5 * AudioMath.RenderQuantum / sampleRate < currentTime)
            {
                if (last.Type == EventType.SetTarget)
                {
                    if (HasSetTargetConverged(defaultValue, last.Value, currentTime, last.Time, last.TimeConstant))
                    {
                        defaultValue = last.Value;
                    }
                    else
                    {
                        return false;
                    }
                }

                Fill(values, 0, n, defaultValue);
                RemoveOldEvents(events.Count);
                return true;
            }

            return false;
        }

        ParamEvent ProcessSetTargetFollowedByRamp(int eventIndex, ParamEvent ev, EventType nextType, long currentFrame, double sampleRate, double controlRate, ref float value)
        {
            if (ev.Type == EventType.SetTarget && (nextType == EventType.LinearRamp || nextType == EventType.ExponentialRamp))
            {
                if (Math.Abs(2 * sampleRate * ev.Time - 2 * currentFrame + 1) <= 1)
                {
                    value = (float)(ev.Value + (value - ev.Value) * Libm.Exp(-(currentFrame / sampleRate - ev.Time) / ev.TimeConstant));
                }
                else
                {
                    float discrete = (float)AudioMath.DiscreteTimeConstant(ev.TimeConstant, controlRate);
                    value += (ev.Value - value) * discrete;
                }

                ParamEvent replacement = new ParamEvent(EventType.SetValue, value, currentFrame / sampleRate);
                events[eventIndex] = replacement;
                return replacement;
            }

            return ev;
        }

        static void ProcessLinearRamp(int fillToFrame, double time1, double time2, float value1, float value2, double sampleRate, float[] values, ref long currentFrame, ref float value, ref int writeIndex)
        {
            double deltaTime = time2 - time1;
            float k = deltaTime <= 1.17549435e-38f ? 0f : (float)(1 / deltaTime);
            float valueDelta = value2 - value1;

            if (fillToFrame > writeIndex)
            {
                float invSr = (float)(1 / sampleRate);
                float offset = (float)(currentFrame / sampleRate - time1);
                float kvd = k * valueDelta;
                float inc = (float)(4 / sampleRate * k * valueDelta);
                float v0 = (invSr * 0f + offset) * kvd + value1;
                float v1 = (invSr * 1f + offset) * kvd + value1;
                float v2 = (invSr * 2f + offset) * kvd + value1;
                float v3 = (invSr * 3f + offset) * kvd + value1;

                int truncated = writeIndex + ((fillToFrame - writeIndex) / 4) * 4;
                currentFrame += truncated - writeIndex;
                for (; writeIndex < truncated; writeIndex += 4)
                {
                    values[writeIndex] = v0;
                    values[writeIndex + 1] = v1;
                    values[writeIndex + 2] = v2;
                    values[writeIndex + 3] = v3;
                    v0 += inc;
                    v1 += inc;
                    v2 += inc;
                    v3 += inc;
                }
            }

            if (writeIndex >= 1)
            {
                value = values[writeIndex - 1];
            }

            for (; writeIndex < fillToFrame; writeIndex++)
            {
                float x = (float)((currentFrame / sampleRate - time1) * k);
                value = value1 + x * valueDelta;
                currentFrame++;
                values[writeIndex] = value;
            }
        }

        static void ProcessExponentialRamp(int fillToFrame, double time1, double time2, float value1, float value2, double sampleRate, float[] values, ref long currentFrame, ref float value, ref int writeIndex)
        {
            if (value1 * value2 <= 0 || time1 >= time2)
            {
                value = value1;
                Fill(values, writeIndex, fillToFrame, value);
                writeIndex = fillToFrame;
                return;
            }

            double deltaTime = time2 - time1;
            double numSampleFrames = deltaTime * sampleRate;
            double multiplier = Libm.Pow(value2 / (double)value1, 1.0 / numSampleFrames);
            value = (float)(value1 * Libm.Pow(value2 / (double)value1, (currentFrame / sampleRate - time1) / deltaTime));
            double accumulator = value;
            for (; writeIndex < fillToFrame; writeIndex++)
            {
                value = (float)accumulator;
                accumulator *= multiplier;
                currentFrame++;
                values[writeIndex] = value;
            }

            if (currentFrame > time2 * sampleRate - 0.5)
            {
                value = value2;
            }
        }

        static void ProcessSetTarget(int fillToFrame, double time1, float value1, double sampleRate, double controlRate, long fillToEndFrame, ParamEvent ev, float[] values, ref long currentFrame, ref float value, ref int writeIndex)
        {
            float target = value1;
            float timeConstant = (float)ev.TimeConstant;
            float discrete = (float)AudioMath.DiscreteTimeConstant(timeConstant, controlRate);

            double rampStartFrame = time1 * sampleRate;
            if (rampStartFrame <= currentFrame && currentFrame < rampStartFrame + 1)
            {
                value = (float)(target + (value - target) * Libm.Exp(-(currentFrame / sampleRate - time1) / timeConstant));
            }
            else
            {
                value += (target - value) * discrete;
            }

            if (HasSetTargetConverged(value, target, currentFrame / sampleRate, time1, timeConstant))
            {
                currentFrame += fillToFrame - writeIndex;
                Fill(values, writeIndex, fillToFrame, target);
                writeIndex = fillToFrame;
                return;
            }

            for (; writeIndex < fillToFrame; writeIndex++)
            {
                float v = value;
                value += (target - value) * discrete;
                values[writeIndex] = v;
            }

            if (writeIndex >= 1)
            {
                value = values[writeIndex - 1];
            }

            currentFrame = fillToEndFrame;
        }
    }
}
