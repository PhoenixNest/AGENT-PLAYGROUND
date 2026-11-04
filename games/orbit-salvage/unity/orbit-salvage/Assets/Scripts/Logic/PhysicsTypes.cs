using System.Collections.Generic;

namespace OrbitalSalvage.Logic
{
    public enum DebrisState
    {
        InRing = 0,
        Falling = 1,
        Ingesting = 2,
        Removed = 3,
    }

    public struct Vec2
    {
        public double X;
        public double Y;

        public Vec2(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public sealed class BlackHole
    {
        public double X = 400;
        public double Y = 575;
        public double EventHorizonRadius = 28;
        public double GravityRadius = 240;
        public double GravityStrength = 480000;
    }

    public sealed class Debris
    {
        public int Id;
        public double X;
        public double Y;
        public double Vx;
        public double Vy;
        public double Radius;
        public double Mass;
        public string Type;
        public string Color;
        public double Weight;
        public DebrisState State;
        public double IngestProgress;
        public double GateCooldown;
    }

    public sealed class GateRuntime
    {
        public GateConfig Config;
        public int Id;
        public string Type;
        public double Value;
        public double Angle;
        public double RMin;
        public double RMax;
        public bool IsInner;
        public string Color;
        public HashSet<int> ProcessedDebris = new HashSet<int>();
    }

    public sealed class DividerRuntime
    {
        public double RMin;
        public double RMax;
        public double Angle;
        public bool IsInner;
    }

    public sealed class FlapRuntime
    {
        public int Id;
        public double RMin;
        public double RMax;
        public double Angle;
        public double Swing;
        public double SwingVel;
        public double MaxSwing;
        public double NormalSign;
        public bool IsInner;
    }

    public sealed class PinRuntime
    {
        public int Id;
        public double R;
        public double Angle;
        public double Radius;
        public double Restitution;
        public bool IsInner;
        public double HitTimer;
        public string Color;
        public string Accent;
    }

    public sealed class TrackWallRuntime
    {
        public double R;
        public double GapAngle;
        public double GapSize;
    }

    public sealed class BumperHit
    {
        public double X;
        public double Y;
        public PinRuntime Pin;
        public Debris Debris;
    }
}
