using UnityEngine;

namespace AssetKits
{
    public static class Enumerations
    {
        public enum Simulation { Local = 0, World = 1 }
        public enum TimeScale { Unscaled = 0, Normal = 1 }
        public enum EmitterShape { Point = 0, Circle = 1, Rectangle = 2, Line = 3, Directional = 4 }
        public enum SpreadType { Random = 0, Uniform = 1 }
        public enum AttractorType { Pivot = 0, Surface = 1 }
        public enum PlayMode { None = 0, OnEnable = 1, OnAwake = 2 }
        public enum SheetType { Lifetime = 0, Speed = 1, FPS = 2 }
    }

    [System.Serializable]
    public class Module
    {
        public bool enabled;
    }

    [System.Serializable]
    public class SpeedRange
    {
        public float from;
        public float to;
    }

    [System.Serializable]
    public class Burst
    {
        public float time;
        public int count;
        public bool used;
    }

    [System.Serializable]
    public class SeparatedMinMaxCurve
    {
        public bool separable;
        public bool separated;
        public ParticleSystem.MinMaxCurve mainCurve;
        public ParticleSystem.MinMaxCurve xCurve;
        public ParticleSystem.MinMaxCurve yCurve;
        public ParticleSystem.MinMaxCurve zCurve;

        public float Evaluate(float time, float lerp)
        {
            return mainCurve.Evaluate(time, lerp);
        }
    }

    [System.Serializable]
    public class ParticleTrailRenderer
    {
    }

    internal class Particle
    {
        public Vector2 position;
        public Vector2 velocity;
        public float age;
        public float lifetime;
        public float size;
        public float rotation;
        public float random;
        public Color color;
    }
}
