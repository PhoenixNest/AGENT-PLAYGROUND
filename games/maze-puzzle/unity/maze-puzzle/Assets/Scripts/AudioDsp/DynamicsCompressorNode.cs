using System;

namespace OrbitalSalvage.AudioDsp
{
    /// <summary>
    /// Port of Chromium DynamicsCompressor (platform/audio/dynamics_compressor.cc) and
    /// DynamicsCompressorHandler (modules/webaudio/dynamics_compressor_handler.cc).
    /// </summary>
    public sealed class DynamicsCompressorNode : AudioNodeDsp
    {
        const float MeteringReleaseTimeConstant = 0.325f;
        const float UninitializedValue = -1;
        const float PreDelay = 0.006f;
        const int MaxPreDelayFrames = 1024;
        const int MaxPreDelayFramesMask = MaxPreDelayFrames - 1;
        const int DefaultPreDelayFrames = 256;
        const float ReleaseZone1 = 0.09f;
        const float ReleaseZone2 = 0.16f;
        const float ReleaseZone3 = 0.42f;
        const float ReleaseZone4 = 0.98f;
        const float SatReleaseTime = 0.0025f;
        const int NumberOfDivisionFrames = 32;

        static readonly float PiOverTwo = (float)(Math.PI / 2);

        static readonly float ABase = F(0.9999999999999998f) * ReleaseZone1 + F(1.8432219684323923e-16f) * ReleaseZone2
            - F(1.9373394351676423e-16f) * ReleaseZone3 + F(8.824516011816245e-18f) * ReleaseZone4;

        static readonly float BBase = F(-1.5788320352845888f) * ReleaseZone1 + F(2.3305837032074286f) * ReleaseZone2
            - F(0.9141194204840429f) * ReleaseZone3 + F(0.1623677525612032f) * ReleaseZone4;

        static readonly float CBase = F(0.5334142869106424f) * ReleaseZone1 - F(1.272736789213631f) * ReleaseZone2
            + F(0.9258856042207512f) * ReleaseZone3 - F(0.18656310191776226f) * ReleaseZone4;

        static readonly float DBase = F(0.08783463138207234f) * ReleaseZone1 - F(0.1694162967925622f) * ReleaseZone2
            + F(0.08588057951595272f) * ReleaseZone3 - F(0.00429891410546283f) * ReleaseZone4;

        static readonly float EBase = F(-0.042416883008123074f) * ReleaseZone1 + F(0.1115693827987602f) * ReleaseZone2
            - F(0.09764676325265872f) * ReleaseZone3 + F(0.028494263462021576f) * ReleaseZone4;

        readonly float[][] preDelayBuffers = { new float[MaxPreDelayFrames], new float[MaxPreDelayFrames] };
        readonly float sampleRate;
        readonly float meteringReleaseK;
        readonly float[] paramValues = new float[AudioMath.RenderQuantum];

        float ratio = UninitializedValue;
        float slope = UninitializedValue;
        float linearThreshold = UninitializedValue;
        float dbThreshold = UninitializedValue;
        float dbKnee = UninitializedValue;
        float kneeThreshold = UninitializedValue;
        float dbKneeThreshold = UninitializedValue;
        float dbYkneeThreshold = UninitializedValue;
        float knee = UninitializedValue;

        float detectorAverage;
        float compressorGain = 1;
        float meteringGain = 1;
        float dbMaxAttackCompressionDiff = -1;
        int preDelayReadIndex;
        int preDelayWriteIndex = DefaultPreDelayFrames;
        int lastPreDelayFrames = DefaultPreDelayFrames;

        public readonly AudioParam Threshold;
        public readonly AudioParam Knee;
        public readonly AudioParam Ratio;
        public readonly AudioParam Attack;
        public readonly AudioParam Release;

        public float Reduction { get; private set; }

        protected override ChannelCountMode InputMode
        {
            get { return ChannelCountMode.ClampedMax; }
        }

        protected override double TailTime
        {
            get { return 5 * MeteringReleaseTimeConstant; }
        }

        protected override double LatencyTime
        {
            get { return lastPreDelayFrames / (double)sampleRate; }
        }

        internal DynamicsCompressorNode(AudioContextDsp ctx, int id)
            : base(ctx, "compressor", id, 2)
        {
            sampleRate = ctx.SampleRate;
            Threshold = new AudioParam(ctx, id, "threshold", -24f, -100f, 0f, false);
            Knee = new AudioParam(ctx, id, "knee", 30f, 0f, 40f, false);
            Ratio = new AudioParam(ctx, id, "ratio", 12f, 1f, 20f, false);
            Attack = new AudioParam(ctx, id, "attack", 0.003f, 0f, 1f, false);
            Release = new AudioParam(ctx, id, "release", 0.25f, 0f, 1f, false);
            meteringReleaseK = (float)AudioMath.DiscreteTimeConstant(MeteringReleaseTimeConstant, sampleRate);
        }

        static float F(float v)
        {
            return v;
        }

        protected override void ProcessOnlyAudioParams()
        {
            Threshold.CalculateSampleAccurateValues(paramValues, AudioMath.RenderQuantum);
            Knee.CalculateSampleAccurateValues(paramValues, AudioMath.RenderQuantum);
            Ratio.CalculateSampleAccurateValues(paramValues, AudioMath.RenderQuantum);
            Attack.CalculateSampleAccurateValues(paramValues, AudioMath.RenderQuantum);
            Release.CalculateSampleAccurateValues(paramValues, AudioMath.RenderQuantum);
        }

        float KneeCurve(float x, float k)
        {
            if (x < linearThreshold)
            {
                return x;
            }

            float offset = (float)(x - linearThreshold);
            float exponent = (float)(-k * offset);
            float decay = (float)(1 - (float)Libm.Exp((double)exponent));
            return (float)(linearThreshold + (float)(decay / k));
        }

        float Saturate(float x, float k)
        {
            if (x < kneeThreshold)
            {
                return KneeCurve(x, k);
            }

            float dbX = AudioMath.LinearToDecibels(x);
            float dbY = (float)(dbYkneeThreshold + (float)(slope * (float)(dbX - dbKneeThreshold)));
            return AudioMath.DecibelsToLinear(dbY);
        }

        float KAtSlope(float desiredSlope)
        {
            float dbX = (float)(dbThreshold + dbKnee);
            float x = AudioMath.DecibelsToLinear(dbX);
            float x2 = 1;
            float dbX2 = 0;

            if (!(x < linearThreshold))
            {
                x2 = (float)(x * 1.001);
                dbX2 = AudioMath.LinearToDecibels(x2);
            }

            float minK = 0.1f;
            float maxK = 10000f;
            float k = 5f;
            float s = 1f;

            for (int i = 0; i < 15; ++i)
            {
                if (!(x < linearThreshold))
                {
                    float dbY = AudioMath.LinearToDecibels(KneeCurve(x, k));
                    float dbY2 = AudioMath.LinearToDecibels(KneeCurve(x2, k));
                    s = (float)((float)(dbY2 - dbY) / (float)(dbX2 - dbX));
                }

                if (s < desiredSlope)
                {
                    maxK = k;
                }
                else
                {
                    minK = k;
                }

                k = MathF.Sqrt((float)(minK * maxK));
            }

            return k;
        }

        float UpdateStaticCurveParameters(float threshold, float kneeDb, float ratioIn)
        {
            if (threshold != dbThreshold || kneeDb != dbKnee || ratioIn != ratio)
            {
                dbThreshold = threshold;
                linearThreshold = AudioMath.DecibelsToLinear(threshold);
                dbKnee = kneeDb;

                ratio = ratioIn;
                slope = (float)(1 / ratio);

                float k = KAtSlope((float)(1 / ratio));

                dbKneeThreshold = (float)(threshold + kneeDb);
                kneeThreshold = AudioMath.DecibelsToLinear(dbKneeThreshold);
                dbYkneeThreshold = AudioMath.LinearToDecibels(KneeCurve(kneeThreshold, k));

                knee = k;
            }

            return knee;
        }

        static float EnsureFinite(float x, float defaultValue)
        {
            return float.IsFinite(x) ? x : defaultValue;
        }

        void SetPreDelayTime(float preDelayTime)
        {
            uint frames = (uint)(preDelayTime * sampleRate);
            if (frames > MaxPreDelayFrames - 1)
            {
                frames = MaxPreDelayFrames - 1;
            }

            if (lastPreDelayFrames != frames)
            {
                lastPreDelayFrames = (int)frames;
                Array.Clear(preDelayBuffers[0], 0, MaxPreDelayFrames);
                Array.Clear(preDelayBuffers[1], 0, MaxPreDelayFrames);
                preDelayReadIndex = 0;
                preDelayWriteIndex = (int)frames;
            }
        }

        protected override void Process()
        {
            float dbThresholdParam = Threshold.FinalValue();
            float dbKneeParam = Knee.FinalValue();
            float ratioParam = Ratio.FinalValue();
            float attackTime = Attack.FinalValue();
            float releaseTime = Release.FinalValue();

            AudioBus source = In;
            AudioBus destination = Out;
            int frames = AudioMath.RenderQuantum;
            int numberOfChannels = destination.ChannelCount;
            int numberOfSourceChannels = source.ChannelCount;

            float k = UpdateStaticCurveParameters(dbThresholdParam, dbKneeParam, ratioParam);

            float linearPostGain = AudioMath.PowF((float)(1 / Saturate(1, k)), 0.6f);

            float attackFrames = Math.Max(0.001f, attackTime) * sampleRate;
            float releaseFrames = sampleRate * releaseTime;
            float satReleaseFrames = SatReleaseTime * sampleRate;

            float a = releaseFrames * ABase;
            float b = releaseFrames * BBase;
            float c = releaseFrames * CBase;
            float d = releaseFrames * DBase;
            float e = releaseFrames * EBase;

            SetPreDelayTime(PreDelay);

            int frameIndex = 0;
            while (frameIndex < frames)
            {
                detectorAverage = EnsureFinite(detectorAverage, 1);

                float desiredGain = detectorAverage;
                float scaledDesiredGain = AudioMath.AsinF(desiredGain) / PiOverTwo;

                float envelopeRate;
                bool isReleasing = scaledDesiredGain > compressorGain;
                float dbCompressionDiff;

                if (scaledDesiredGain == 0)
                {
                    dbCompressionDiff = isReleasing ? -1 : 1;
                }
                else
                {
                    dbCompressionDiff = AudioMath.LinearToDecibels(compressorGain / scaledDesiredGain);
                }

                if (isReleasing)
                {
                    dbMaxAttackCompressionDiff = -1;
                    dbCompressionDiff = EnsureFinite(dbCompressionDiff, -1);

                    float x = dbCompressionDiff;
                    x = AudioMath.Clamp(x, -12.0f, 0.0f);
                    x = 0.25f * (x + 12);

                    float x2 = x * x;
                    float x3 = x2 * x;
                    float x4 = x2 * x2;
                    float calcReleaseFrames = a + b * x + c * x2 + d * x3 + e * x4;

                    const float dbSpacing = 5;
                    float dbPerFrame = dbSpacing / calcReleaseFrames;
                    envelopeRate = AudioMath.DecibelsToLinear(dbPerFrame);
                }
                else
                {
                    dbCompressionDiff = EnsureFinite(dbCompressionDiff, 1);

                    if (dbMaxAttackCompressionDiff == -1 || dbMaxAttackCompressionDiff < dbCompressionDiff)
                    {
                        dbMaxAttackCompressionDiff = dbCompressionDiff;
                    }

                    float dbEffAttenDiff = Math.Max(0.5f, dbMaxAttackCompressionDiff);
                    float x = 0.25f / dbEffAttenDiff;
                    envelopeRate = 1 - AudioMath.PowF(x, 1 / attackFrames);
                }

                int readIndex = preDelayReadIndex;
                int writeIndex = preDelayWriteIndex;
                float detector = detectorAverage;
                float gain = compressorGain;

                int loopFrames = Math.Min(NumberOfDivisionFrames, frames - frameIndex);
                for (int division = 0; division < loopFrames; ++division)
                {
                    float compressorInput = 0;

                    for (int j = 0; j < numberOfChannels; ++j)
                    {
                        float undelayed = (numberOfSourceChannels > j ? source.Channels[j] : source.Channels[0])[frameIndex];
                        preDelayBuffers[j][writeIndex] = undelayed;

                        float absUndelayed = undelayed > 0 ? undelayed : -undelayed;
                        if (compressorInput < absUndelayed)
                        {
                            compressorInput = absUndelayed;
                        }
                    }

                    float scaledInput = compressorInput;
                    float absInput = scaledInput > 0 ? scaledInput : -scaledInput;

                    float shapedInput = Saturate(absInput, k);
                    float attenuation = absInput <= 0.0001f ? 1 : shapedInput / absInput;

                    float dbAttenuation = Math.Max(2.0f, -AudioMath.LinearToDecibels(attenuation));
                    float dbPerFrameSat = dbAttenuation / satReleaseFrames;
                    float satReleaseRate = AudioMath.DecibelsToLinear(dbPerFrameSat) - 1;

                    bool isRelease = attenuation > detector;
                    float rate = isRelease ? satReleaseRate : 1;

                    detector += (attenuation - detector) * rate;
                    detector = Math.Min(1.0f, detector);
                    detector = EnsureFinite(detector, 1);

                    if (envelopeRate < 1)
                    {
                        gain += (scaledDesiredGain - gain) * envelopeRate;
                    }
                    else
                    {
                        gain *= envelopeRate;
                        gain = Math.Min(1.0f, gain);
                    }

                    float postWarpGain = (float)Libm.Sin((double)(PiOverTwo * gain));
                    float totalGain = linearPostGain * postWarpGain;

                    float dbRealGain = AudioMath.LinearToDecibels(postWarpGain);
                    if (dbRealGain < meteringGain)
                    {
                        meteringGain = dbRealGain;
                    }
                    else
                    {
                        meteringGain += (dbRealGain - meteringGain) * meteringReleaseK;
                    }

                    for (int j = 0; j < numberOfChannels; ++j)
                    {
                        destination.Channels[j][frameIndex] = preDelayBuffers[j][readIndex] * totalGain;
                    }

                    frameIndex++;
                    readIndex = (readIndex + 1) & MaxPreDelayFramesMask;
                    writeIndex = (writeIndex + 1) & MaxPreDelayFramesMask;
                }

                preDelayReadIndex = readIndex;
                preDelayWriteIndex = writeIndex;
                detectorAverage = AudioMath.FlushDenormal(detector);
                compressorGain = AudioMath.FlushDenormal(gain);
            }

            Reduction = meteringGain;
        }
    }
}
