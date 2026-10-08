using System;
using System.Collections.Generic;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic
{
    public sealed class PhysicsEngine
    {
        private readonly IAudioCues _audio;

        public List<Debris> DebrisList = new List<Debris>();
        public List<GateRuntime> Gates = new List<GateRuntime>();
        public List<DividerRuntime> Dividers = new List<DividerRuntime>();
        public List<TrackWallRuntime> TrackWalls = new List<TrackWallRuntime>();
        public int NextDebrisId = 1;

        public Vec2 Center = new Vec2(400, 320);
        public double RingRadius = 180;
        public double CorridorWidth = 40;
        public double InnerRadius = 100;
        public double ApertureAngle = Math.PI * 0.28;

        public double RingAngle;
        public double AngularVelocity;
        public double DragSensitivity = 0.005;
        public double Damping = 0.94;
        public bool RcsThrusting;
        public double RcsDirection;
        public double RcsBoostTimer;
        public double LastRatchetAngle;

        public Vec2 Gravity = new Vec2(0, 360);

        public BlackHole BlackHole = new BlackHole { X = 400, Y = 575, EventHorizonRadius = 28, GravityRadius = 240, GravityStrength = 480000 };

        public int MaxDebrisLimit = 320;

        public List<PinRuntime> DeflectorPins = new List<PinRuntime>();
        public List<FlapRuntime> OneWayFlaps = new List<FlapRuntime>();

        public bool ConcentricRings;
        public double GearRatio = -1.0;
        public double InnerRingAngle;
        public double TrackRadius = 130;

        public Action<Debris> OnDebrisSwallowed;
        public Action<Debris> OnDebrisExploded;
        public Action<GateRuntime, Debris> OnGatePassed;
        public Action<BumperHit> OnBumperHit;
        public Action<double> OnBoostTriggered;
        public Action<Debris> OnDebrisSpilled;

        public PhysicsEngine(IAudioCues audio)
        {
            _audio = audio;
        }

        public IAudioCues Audio
        {
            get { return _audio; }
        }

        private static double Rand()
        {
            return JsRandom.Global.NextDouble();
        }

        private static bool Truthy(double v)
        {
            return v != 0 && !double.IsNaN(v);
        }

        private static double OrDefault(double v, double fallback)
        {
            return Truthy(v) ? v : fallback;
        }

        public void InitSector(SectorConfig cfg, Vec2 centerPos, Vec2? blackHolePos)
        {
            Center = centerPos;
            RingRadius = cfg.RingRadius;
            CorridorWidth = cfg.CorridorWidth;
            InnerRadius = (cfg.InnerRadius.HasValue && Truthy(cfg.InnerRadius.Value))
                ? cfg.InnerRadius.Value
                : RingRadius - CorridorWidth * 2;
            ApertureAngle = cfg.ApertureAngle;

            if (blackHolePos.HasValue)
            {
                BlackHole.X = blackHolePos.Value.X;
                BlackHole.Y = blackHolePos.Value.Y;
            }

            RingAngle = 0;
            AngularVelocity = 0;
            LastRatchetAngle = 0;
            DebrisList = new List<Debris>();

            ConcentricRings = cfg.ConcentricRings == true;
            GearRatio = cfg.GearRatio.HasValue ? cfg.GearRatio.Value : -1.0;
            InnerRingAngle = 0;
            TrackRadius = (cfg.TrackWalls != null && cfg.TrackWalls.Count > 0)
                ? cfg.TrackWalls[0].R
                : (InnerRadius + RingRadius) * 0.5;

            OneWayFlaps = new List<FlapRuntime>();
            if (cfg.OneWayFlaps != null)
            {
                for (int idx = 0; idx < cfg.OneWayFlaps.Count; idx++)
                {
                    OneWayFlapConfig f = cfg.OneWayFlaps[idx];
                    OneWayFlaps.Add(new FlapRuntime
                    {
                        Id = idx,
                        RMin = f.RMin.HasValue ? f.RMin.Value : InnerRadius + 5,
                        RMax = f.RMax.HasValue ? f.RMax.Value : RingRadius - 5,
                        Angle = OrDefault(f.Angle, 0),
                        Swing = 0,
                        SwingVel = 0,
                        MaxSwing = OrDefault(f.MaxSwing, Math.PI * 0.35),
                        NormalSign = f.NormalSign.HasValue ? f.NormalSign.Value : 1,
                        IsInner = f.IsInner == true,
                    });
                }
            }

            Gates = new List<GateRuntime>();
            for (int idx = 0; idx < cfg.Gates.Count; idx++)
            {
                GateConfig g = cfg.Gates[idx];
                double gr = (g.R.HasValue && Truthy(g.R.Value)) ? g.R.Value : 120;
                double gl = (g.Length.HasValue && Truthy(g.Length.Value)) ? g.Length.Value : 34;
                Gates.Add(new GateRuntime
                {
                    Config = g,
                    Id = idx,
                    Type = g.Type,
                    Value = g.Value,
                    Angle = g.Angle,
                    Color = g.Color,
                    RMin = g.RMin.HasValue ? g.RMin.Value : gr - gl * 0.5,
                    RMax = g.RMax.HasValue ? g.RMax.Value : gr + gl * 0.5,
                    IsInner = g.IsInner.HasValue
                        ? g.IsInner.Value
                        : (ConcentricRings && g.RMax.HasValue && g.RMax.Value <= TrackRadius + 5),
                });
            }

            Dividers = new List<DividerRuntime>();
            if (cfg.Dividers != null)
            {
                foreach (DividerConfig d in cfg.Dividers)
                {
                    Dividers.Add(new DividerRuntime
                    {
                        RMin = d.RMin.HasValue ? d.RMin.Value : double.NaN,
                        RMax = d.RMax.HasValue ? d.RMax.Value : double.NaN,
                        Angle = d.Angle,
                        IsInner = d.IsInner.HasValue
                            ? d.IsInner.Value
                            : (ConcentricRings && d.RMax.HasValue && d.RMax.Value <= TrackRadius + 5),
                    });
                }
            }

            TrackWalls = new List<TrackWallRuntime>();
            if (cfg.TrackWalls != null)
            {
                foreach (TrackWallConfig t in cfg.TrackWalls)
                {
                    TrackWalls.Add(new TrackWallRuntime { R = t.R, GapAngle = t.GapAngle, GapSize = t.GapSize });
                }
            }

            DeflectorPins = new List<PinRuntime>();
            if (cfg.DeflectorPins != null)
            {
                for (int idx = 0; idx < cfg.DeflectorPins.Count; idx++)
                {
                    DeflectorPinConfig p = cfg.DeflectorPins[idx];
                    DeflectorPins.Add(new PinRuntime
                    {
                        Id = idx,
                        R = p.R,
                        Angle = p.Angle,
                        Radius = OrDefault(p.Radius, 9),
                        Restitution = OrDefault(p.Restitution, 1.35),
                        IsInner = p.IsInner.HasValue ? p.IsInner.Value : (ConcentricRings && p.R <= TrackRadius),
                        HitTimer = 0,
                        Color = p.Color,
                        Accent = p.Accent,
                    });
                }
            }

            double count = OrDefault(cfg.InitialDebrisCount, 14);
            List<string> types = cfg.InitialDebrisTypes ?? new List<string> { "titanium" };

            for (int i = 0; i < count; i++)
            {
                string type = types.Count == 0 ? "titanium" : types[i % types.Count];
                double spawnAngle = -Math.PI * 0.5 + (Rand() - 0.5) * 0.7;
                double spawnRadius = InnerRadius + Rand() * (RingRadius - InnerRadius);
                double x = Center.X + JsMath.Cos(spawnAngle) * spawnRadius;
                double y = Center.Y + JsMath.Sin(spawnAngle) * spawnRadius;

                double vx = (Rand() - 0.5) * 20;
                double vy = (Rand() - 0.5) * 20;
                SpawnDebris(x, y, vx, vy, type, 1);
            }
        }

        public Debris SpawnDebris(double x, double y, double vx, double vy, string type, double weight)
        {
            if (DebrisList.Count >= MaxDebrisLimit)
            {
                int idx = (int)JsMath.Floor(Rand() * DebrisList.Count);
                if (idx >= 0 && idx < DebrisList.Count)
                {
                    DebrisList[idx].Weight += 1;
                }
                return null;
            }

            double radius = 6.5;
            string color = "#38bdf8";
            double mass = 1.0;

            if (type == "solar")
            {
                radius = 5.5;
                color = "#fbbf24";
                mass = 0.8;
            }
            else if (type == "volatile")
            {
                radius = 7.5;
                color = "#ef4444";
                mass = 1.2;
            }
            else if (type == "magnetic")
            {
                radius = 6.0;
                color = "#a855f7";
                mass = 1.1;
            }

            var debris = new Debris
            {
                Id = NextDebrisId++,
                X = x,
                Y = y,
                Vx = vx,
                Vy = vy,
                Radius = radius,
                Mass = mass,
                Type = type,
                Color = color,
                Weight = weight,
                State = DebrisState.InRing,
                IngestProgress = 0,
                GateCooldown = 0,
            };

            DebrisList.Add(debris);
            return debris;
        }

        public Debris SpawnDebris(double x, double y)
        {
            return SpawnDebris(x, y, 0, 0, "titanium", 1);
        }

        public void SpawnBackupScrap(int count = 8)
        {
            string[] types = { "titanium", "solar", "titanium" };
            for (int i = 0; i < count; i++)
            {
                string type = types[i % types.Length];
                double spawnAngle = -Math.PI * 0.5 + (Rand() - 0.5) * 0.8;
                double spawnRadius = InnerRadius + 15 + Rand() * (RingRadius - InnerRadius - 30);
                double x = Center.X + JsMath.Cos(spawnAngle) * spawnRadius;
                double y = Center.Y + JsMath.Sin(spawnAngle) * spawnRadius;

                double vx = (Rand() - 0.5) * 40;
                double vy = (Rand() - 0.5) * 40;
                SpawnDebris(x, y, vx, vy, type, 1);
            }
        }

        public void ApplyInputDrag(double deltaX)
        {
            AngularVelocity += deltaX * DragSensitivity;
            AngularVelocity = JsMath.Max(-12, JsMath.Min(12, AngularVelocity));
        }

        public void ApplyRcsBurst(double direction = 1)
        {
            AngularVelocity += direction * 3.8;
            RcsThrusting = true;
            RcsDirection = direction;
            RcsBoostTimer = 0.42;
            _audio.PlaySpinRatchet(1.6);
            if (OnBoostTriggered != null)
            {
                OnBoostTriggered(direction);
            }
        }

        public void Update(double dt)
        {
            if (RcsBoostTimer > 0)
            {
                RcsBoostTimer = JsMath.Max(0, RcsBoostTimer - dt);
                if (RcsBoostTimer == 0)
                {
                    RcsThrusting = false;
                    RcsDirection = 0;
                }
            }

            double prevAngle = RingAngle;
            RingAngle += AngularVelocity * dt;
            AngularVelocity *= JsMath.Pow(Damping, dt * 60);
            if (Math.Abs(AngularVelocity) < 0.001)
            {
                AngularVelocity = 0;
            }

            if (ConcentricRings)
            {
                InnerRingAngle += AngularVelocity * GearRatio * dt;
            }
            else
            {
                InnerRingAngle = RingAngle;
            }

            foreach (FlapRuntime f in OneWayFlaps)
            {
                f.SwingVel += (-f.Swing * 28 - f.SwingVel * 10) * dt;
                f.Swing += f.SwingVel * dt;
                if (f.Swing < 0)
                {
                    f.Swing = 0;
                    f.SwingVel = 0;
                }
                else if (f.Swing > f.MaxSwing)
                {
                    f.Swing = f.MaxSwing;
                    f.SwingVel = 0;
                }
            }

            double angleTraveled = Math.Abs(RingAngle - LastRatchetAngle);
            if (angleTraveled >= Math.PI / 16)
            {
                double spinSpeed = Math.Abs(AngularVelocity);
                _audio.PlaySpinRatchet(JsMath.Min(1.5, 0.8 + spinSpeed * 0.2));
                LastRatchetAngle = RingAngle;
            }

            double worldApertureStart = NormalizeAngle(RingAngle + Math.PI * 0.5 - ApertureAngle * 0.5);
            double worldApertureEnd = NormalizeAngle(RingAngle + Math.PI * 0.5 + ApertureAngle * 0.5);

            foreach (PinRuntime pin in DeflectorPins)
            {
                if (pin.HitTimer > 0)
                {
                    pin.HitTimer = JsMath.Max(0, pin.HitTimer - dt);
                }
            }

            for (int i = DebrisList.Count - 1; i >= 0; i--)
            {
                Debris d = DebrisList[i];

                if (d.State == DebrisState.Removed)
                {
                    DebrisList.RemoveAt(i);
                    continue;
                }

                if (d.State == DebrisState.Ingesting)
                {
                    UpdateIngestion(d, dt);
                    continue;
                }

                double dxToBh = BlackHole.X - d.X;
                double dyToBh = BlackHole.Y - d.Y;
                double distToBh = JsMath.Hypot(dxToBh, dyToBh);

                if (distToBh <= BlackHole.EventHorizonRadius)
                {
                    d.State = DebrisState.Ingesting;
                    _audio.PlayDebrisSwallowed();
                    if (OnDebrisSwallowed != null)
                    {
                        OnDebrisSwallowed(d);
                    }
                    continue;
                }

                if (distToBh < BlackHole.GravityRadius)
                {
                    double force = (BlackHole.GravityStrength / (distToBh * distToBh + 400)) * d.Mass;
                    double normX = dxToBh / (distToBh + 0.001);
                    double normY = dyToBh / (distToBh + 0.001);
                    d.Vx += normX * force * dt;
                    d.Vy += normY * force * dt;
                }

                d.Vx += Gravity.X * dt;
                d.Vy += Gravity.Y * dt;

                double dxToCenter = d.X - Center.X;
                double dyToCenter = d.Y - Center.Y;
                double distToCenter = JsMath.Hypot(dxToCenter, dyToCenter);

                if (d.State == DebrisState.InRing)
                {
                    double ringVel = (ConcentricRings && distToCenter < TrackRadius)
                        ? AngularVelocity * GearRatio
                        : AngularVelocity;

                    double centrifugal = distToCenter * (ringVel * ringVel) * 0.06;
                    if (distToCenter > 1)
                    {
                        d.Vx += (dxToCenter / distToCenter) * centrifugal * dt;
                        d.Vy += (dyToCenter / distToCenter) * centrifugal * dt;
                    }

                    double tangX = -dyToCenter / distToCenter;
                    double tangY = dxToCenter / distToCenter;
                    double ringSurfaceSpeed = ringVel * distToCenter;
                    double debrisTangSpeed = d.Vx * tangX + d.Vy * tangY;
                    double speedDiff = ringSurfaceSpeed - debrisTangSpeed;
                    d.Vx += tangX * speedDiff * dt * 1.4;
                    d.Vy += tangY * speedDiff * dt * 1.4;
                }

                if (d.GateCooldown > 0)
                {
                    d.GateCooldown = JsMath.Max(0, d.GateCooldown - dt);
                }

                double currentSpeed = JsMath.Hypot(d.Vx, d.Vy);
                if (currentSpeed > 460)
                {
                    d.Vx = (d.Vx / currentSpeed) * 460;
                    d.Vy = (d.Vy / currentSpeed) * 460;
                }

                d.X += d.Vx * dt;
                d.Y += d.Vy * dt;

                d.Vx *= 0.994;
                d.Vy *= 0.994;

                ResolveRingCollisions(d, worldApertureStart, worldApertureEnd);

                if (d.State == DebrisState.Falling)
                {
                    ResolveMagneticFunnel(d, dt);

                    if (d.Y > BlackHole.Y + 140 || d.X < -60 || d.X > 500)
                    {
                        d.State = DebrisState.Removed;
                        if (OnDebrisSpilled != null)
                        {
                            OnDebrisSpilled(d);
                        }
                        continue;
                    }
                }

                if (d.State == DebrisState.InRing)
                {
                    ResolveDividerCollisions(d);
                    ResolveFlapperCollisions(d);
                    ResolveTrackWallCollisions(d);
                    ResolveDeflectorCollisions(d);
                    CheckMultiplierGates(d);
                }
            }

            ResolveDebrisCollisions();
        }

        public void ResolveMagneticFunnel(Debris d, double dt)
        {
            double topY = Center.Y + RingRadius - 15;
            double bottomY = BlackHole.Y;

            if (d.Y >= topY && d.Y <= bottomY + 25)
            {
                double t = JsMath.Max(0, JsMath.Min(1, (d.Y - topY) / JsMath.Max(1, bottomY - topY)));
                double halfWidth = 95 * (1 - t) + 46 * t;
                double centerX = BlackHole.X;

                // Centering spring. k = 16 (omega 4 rad/s) with only the per-step 0.994
                // drag is underdamped: the half-period matches the fall, so debris that
                // leave the aperture off-center arrive on the opposite rail (~39 px),
                // outside the 28 px event horizon, and spill. c = 8 is critical damping
                // (2*sqrt(16)). Idle then spills nothing; a hard swipe or spin still can.
                double dx = centerX - d.X;
                d.Vx += (dx * 16 - d.Vx * 8) * dt;

                double leftRail = centerX - halfWidth + d.Radius;
                double rightRail = centerX + halfWidth - d.Radius;

                if (d.X < leftRail)
                {
                    d.X = leftRail;
                    if (d.Vx < 0)
                    {
                        d.Vx = -d.Vx * 0.45;
                    }
                }
                else if (d.X > rightRail)
                {
                    d.X = rightRail;
                    if (d.Vx > 0)
                    {
                        d.Vx = -d.Vx * 0.45;
                    }
                }
            }
        }

        public void UpdateIngestion(Debris d, double dt)
        {
            d.IngestProgress += dt * 3.8;
            double dx = BlackHole.X - d.X;
            double dy = BlackHole.Y - d.Y;
            d.X += dx * dt * 8.5 - dy * dt * 4.5;
            d.Y += dy * dt * 8.5 + dx * dt * 4.5;
            d.Radius = JsMath.Max(0.1, d.Radius * (1 - dt * 4.5));

            if (d.IngestProgress >= 1.0 || d.Radius <= 0.5)
            {
                d.State = DebrisState.Removed;
            }
        }

        public void ResolveRingCollisions(Debris d, double apStart, double apEnd)
        {
            if (d.State != DebrisState.InRing) return;

            double dx = d.X - Center.X;
            double dy = d.Y - Center.Y;
            double dist = JsMath.Hypot(dx, dy);
            if (dist < 1) return;

            double angle = NormalizeAngle(JsMath.Atan2(dy, dx));
            bool isOverAperture = IsAngleBetween(angle, apStart, apEnd);

            if (dist > RingRadius - d.Radius)
            {
                if (isOverAperture)
                {
                    if (d.State != DebrisState.Falling)
                    {
                        d.State = DebrisState.Falling;
                        d.Vx *= 0.35;
                        d.Vy = JsMath.Max(d.Vy, 95);
                    }
                }
                else
                {
                    double overlap = dist - (RingRadius - d.Radius);
                    double nx = dx / dist;
                    double ny = dy / dist;
                    d.X -= nx * overlap;
                    d.Y -= ny * overlap;

                    double dot = d.Vx * nx + d.Vy * ny;
                    if (dot > 0)
                    {
                        d.Vx = (d.Vx - 1.25 * dot * nx) * 0.8;
                        d.Vy = (d.Vy - 1.25 * dot * ny) * 0.8;
                        _audio.PlayDebrisCollision(JsMath.Min(1.0, dot / 70), d.Type);
                        CheckVolatileDetonation(d, dot);
                    }
                }
            }

            if (d.State == DebrisState.InRing && dist < InnerRadius + d.Radius)
            {
                double overlap = InnerRadius + d.Radius - dist;
                double nx = dx / dist;
                double ny = dy / dist;
                d.X += nx * overlap;
                d.Y += ny * overlap;

                double dot = d.Vx * nx + d.Vy * ny;
                if (dot < 0)
                {
                    d.Vx = (d.Vx - 1.25 * dot * nx) * 0.8;
                    d.Vy = (d.Vy - 1.25 * dot * ny) * 0.8;
                    _audio.PlayDebrisCollision(JsMath.Min(1.0, Math.Abs(dot) / 70), d.Type);
                }
            }
        }

        public void ResolveDividerCollisions(Debris d)
        {
            double dx = d.X - Center.X;
            double dy = d.Y - Center.Y;
            double dist = JsMath.Hypot(dx, dy);

            foreach (DividerRuntime div in Dividers)
            {
                if (dist < div.RMin - d.Radius || dist > div.RMax + d.Radius) continue;

                double ringRot = (div.IsInner && ConcentricRings) ? InnerRingAngle : RingAngle;

                double angleInRing = NormalizeAngle(JsMath.Atan2(dy, dx) - ringRot);

                double divAngle = NormalizeAngle(div.Angle);
                double angleDiff = angleInRing - divAngle;
                while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
                while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

                double arcDist = angleDiff * dist;
                double thickness = 6;

                if (Math.Abs(arcDist) < thickness * 0.5 + d.Radius)
                {
                    double normalSign = arcDist > 0 ? 1 : -1;
                    double worldDivAngle = divAngle + ringRot;
                    double normalX = -JsMath.Sin(worldDivAngle) * normalSign;
                    double normalY = JsMath.Cos(worldDivAngle) * normalSign;

                    double penetration = thickness * 0.5 + d.Radius - Math.Abs(arcDist);
                    d.X += normalX * penetration;
                    d.Y += normalY * penetration;

                    double dot = d.Vx * normalX + d.Vy * normalY;
                    if (dot < 0)
                    {
                        d.Vx = (d.Vx - 1.3 * dot * normalX) * 0.75;
                        d.Vy = (d.Vy - 1.3 * dot * normalY) * 0.75;
                        _audio.PlayDebrisCollision(JsMath.Min(1.0, Math.Abs(dot) / 65), d.Type);
                        CheckVolatileDetonation(d, Math.Abs(dot));
                    }
                }
            }
        }

        public void ResolveFlapperCollisions(Debris d)
        {
            if (OneWayFlaps == null || OneWayFlaps.Count == 0) return;
            double dx = d.X - Center.X;
            double dy = d.Y - Center.Y;
            double dist = JsMath.Hypot(dx, dy);

            foreach (FlapRuntime f in OneWayFlaps)
            {
                if (dist < f.RMin - d.Radius || dist > f.RMax + d.Radius) continue;

                double ringRot = (f.IsInner && ConcentricRings) ? InnerRingAngle : RingAngle;

                double angleInRing = NormalizeAngle(JsMath.Atan2(dy, dx) - ringRot);

                double currentFlapAngle = NormalizeAngle(f.Angle + f.Swing * OrDefault(f.NormalSign, 1));
                double angleDiff = angleInRing - currentFlapAngle;
                while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
                while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

                double arcDist = angleDiff * dist;
                double thickness = 7;

                if (Math.Abs(arcDist) < thickness * 0.5 + d.Radius)
                {
                    double forwardSign = OrDefault(f.NormalSign, 1);
                    double pushDirection = arcDist * forwardSign;

                    if (pushDirection < 0)
                    {
                        double pushForce = JsMath.Min(2.5, (thickness * 0.5 + d.Radius - Math.Abs(arcDist)) * 0.18);
                        f.Swing += pushForce * 0.3;
                        f.SwingVel += pushForce * 12;
                        if (f.Swing > f.MaxSwing) f.Swing = f.MaxSwing;
                    }
                    else
                    {
                        if (f.Swing <= 0.08)
                        {
                            double normalSign = arcDist > 0 ? 1 : -1;
                            double worldFlapAngle = currentFlapAngle + ringRot;
                            double normalX = -JsMath.Sin(worldFlapAngle) * normalSign;
                            double normalY = JsMath.Cos(worldFlapAngle) * normalSign;

                            double penetration = thickness * 0.5 + d.Radius - Math.Abs(arcDist);
                            d.X += normalX * penetration;
                            d.Y += normalY * penetration;

                            double dot = d.Vx * normalX + d.Vy * normalY;
                            if (dot < 0)
                            {
                                d.Vx = (d.Vx - 1.25 * dot * normalX) * 0.75;
                                d.Vy = (d.Vy - 1.25 * dot * normalY) * 0.75;
                                _audio.PlayDebrisCollision(JsMath.Min(1.0, Math.Abs(dot) / 60), d.Type);
                            }
                        }
                    }
                }
            }
        }

        public void ResolveTrackWallCollisions(Debris d)
        {
            if (TrackWalls.Count == 0) return;

            double dx = d.X - Center.X;
            double dy = d.Y - Center.Y;
            double dist = JsMath.Hypot(dx, dy);
            double angleInRing = NormalizeAngle(JsMath.Atan2(dy, dx) - RingAngle);

            foreach (TrackWallRuntime tw in TrackWalls)
            {
                double thickness = 6;
                if (Math.Abs(dist - tw.R) < thickness * 0.5 + d.Radius)
                {
                    double gapAng = NormalizeAngle(tw.GapAngle);
                    double diff = angleInRing - gapAng;
                    while (diff > Math.PI) diff -= Math.PI * 2;
                    while (diff < -Math.PI) diff += Math.PI * 2;

                    if (Math.Abs(diff) < OrDefault(tw.GapSize, 0.4) * 0.5)
                    {
                        continue;
                    }

                    bool isOutside = dist > tw.R;
                    double nx = dx / dist;
                    double ny = dy / dist;
                    double targetDist = isOutside
                        ? tw.R + thickness * 0.5 + d.Radius
                        : tw.R - thickness * 0.5 - d.Radius;
                    double shift = targetDist - dist;

                    d.X += nx * shift;
                    d.Y += ny * shift;

                    double dot = d.Vx * nx + d.Vy * ny;
                    if ((isOutside && dot < 0) || (!isOutside && dot > 0))
                    {
                        d.Vx = (d.Vx - 1.3 * dot * nx) * 0.75;
                        d.Vy = (d.Vy - 1.3 * dot * ny) * 0.75;
                        _audio.PlayDebrisCollision(JsMath.Min(1.0, Math.Abs(dot) / 65), d.Type);
                    }
                }
            }
        }

        public void ResolveDeflectorCollisions(Debris d)
        {
            if (DeflectorPins == null || DeflectorPins.Count == 0) return;

            foreach (PinRuntime pin in DeflectorPins)
            {
                double ringRot = (pin.IsInner && ConcentricRings) ? InnerRingAngle : RingAngle;
                double ringVel = (pin.IsInner && ConcentricRings) ? AngularVelocity * GearRatio : AngularVelocity;

                double worldAngle = ringRot + pin.Angle;
                double pinX = Center.X + JsMath.Cos(worldAngle) * pin.R;
                double pinY = Center.Y + JsMath.Sin(worldAngle) * pin.R;

                double dx = d.X - pinX;
                double dy = d.Y - pinY;
                double dist = JsMath.Hypot(dx, dy);
                double minDist = pin.Radius + d.Radius;

                if (dist < minDist && dist > 0.001)
                {
                    double nx = dx / dist;
                    double ny = dy / dist;
                    double overlap = minDist - dist;

                    d.X += nx * overlap;
                    d.Y += ny * overlap;

                    double pinVx = -JsMath.Sin(worldAngle) * pin.R * ringVel;
                    double pinVy = JsMath.Cos(worldAngle) * pin.R * ringVel;

                    double relVx = d.Vx - pinVx;
                    double relVy = d.Vy - pinVy;
                    double velAlongNorm = relVx * nx + relVy * ny;

                    if (velAlongNorm < 0)
                    {
                        double restitution = OrDefault(pin.Restitution, 1.35);
                        double impulse = -(1 + restitution) * velAlongNorm;

                        d.Vx += nx * impulse;
                        d.Vy += ny * impulse;

                        d.Vx += (Rand() - 0.5) * 20;
                        d.Vy += (Rand() - 0.5) * 20;

                        pin.HitTimer = 0.25;
                        _audio.PlayBumperBounce();
                        if (OnBumperHit != null)
                        {
                            OnBumperHit(new BumperHit { X = pinX, Y = pinY, Pin = pin, Debris = d });
                        }
                    }
                }
            }
        }

        private void SyncAdjacentGates(GateRuntime gate, int debrisId)
        {
            gate.ProcessedDebris.Add(debrisId);
            foreach (GateRuntime otherGate in Gates)
            {
                if (ReferenceEquals(otherGate, gate)) continue;
                if (ConcentricRings && otherGate.IsInner != gate.IsInner) continue;
                double diff = Math.Abs(NormalizeAngle(otherGate.Angle) - NormalizeAngle(gate.Angle));
                if (diff > Math.PI) diff = Math.PI * 2 - diff;
                if (diff < 0.16 * Math.PI)
                {
                    otherGate.ProcessedDebris.Add(debrisId);
                }
            }
        }

        public void CheckMultiplierGates(Debris d)
        {
            if (d.GateCooldown > 0) return;

            double dx = d.X - Center.X;
            double dy = d.Y - Center.Y;
            double dist = JsMath.Hypot(dx, dy);

            foreach (GateRuntime gate in Gates)
            {
                if (gate.ProcessedDebris.Contains(d.Id)) continue;

                double ringRot = (gate.IsInner && ConcentricRings) ? InnerRingAngle : RingAngle;

                double angleInRing = NormalizeAngle(JsMath.Atan2(dy, dx) - ringRot);

                double gateAngle = NormalizeAngle(gate.Angle);
                double angleDiff = angleInRing - gateAngle;
                while (angleDiff > Math.PI) angleDiff -= Math.PI * 2;
                while (angleDiff < -Math.PI) angleDiff += Math.PI * 2;

                double arcDist = Math.Abs(angleDiff * dist);
                if (arcDist < 16 && dist >= gate.RMin - d.Radius && dist <= gate.RMax + d.Radius)
                {
                    d.GateCooldown = 0.22;

                    SyncAdjacentGates(gate, d.Id);

                    _audio.PlayGatePass(gate.Type, gate.Value);

                    if (OnGatePassed != null)
                    {
                        OnGatePassed(gate, d);
                    }

                    if (gate.Type == "BOOST")
                    {
                        double boostMagnitude = OrDefault(gate.Value, 150);
                        double ringDir = OrDefault(JsMath.Sign(AngularVelocity), 1);
                        double boostAngle = ringRot + gateAngle + Math.PI * 0.5 * ringDir;
                        d.Vx += JsMath.Cos(boostAngle) * boostMagnitude;
                        d.Vy += JsMath.Sin(boostAngle) * boostMagnitude;
                        _audio.PlayRcsThrust();
                        continue;
                    }

                    double newCount = 0;
                    if (gate.Type == "MULTIPLY")
                    {
                        newCount = JsMath.Max(0, gate.Value - 1);
                    }
                    else if (gate.Type == "ADD")
                    {
                        newCount = gate.Value;
                    }

                    double span = JsMath.Min(24, (gate.RMax - gate.RMin) * 0.35);
                    double worldGateAngle = ringRot + gateAngle;
                    double nx = JsMath.Cos(worldGateAngle);
                    double ny = JsMath.Sin(worldGateAngle);
                    double tx = -ny;
                    double ty = nx;

                    for (int k = 0; k < newCount; k++)
                    {
                        double radialOffset = newCount > 1
                            ? ((k / (newCount - 1)) - 0.5) * span
                            : 0;
                        double jitterTang = (Rand() - 0.5) * d.Radius * 0.4;
                        double sx = d.X + nx * radialOffset + tx * jitterTang;
                        double sy = d.Y + ny * radialOffset + ty * jitterTang;
                        double svx = d.Vx * (0.85 + Rand() * 0.25) + (Rand() - 0.5) * 12;
                        double svy = d.Vy * (0.85 + Rand() * 0.25) + (Rand() - 0.5) * 12;
                        Debris spawned = SpawnDebris(sx, sy, svx, svy, d.Type, 1);
                        if (spawned != null)
                        {
                            spawned.GateCooldown = 0.25;
                            SyncAdjacentGates(gate, spawned.Id);
                        }
                    }
                }
            }
        }

        public void CheckVolatileDetonation(Debris d, double impactVelocity)
        {
            if (d.Type != "volatile" || impactVelocity < 170) return;

            d.State = DebrisState.Removed;
            _audio.PlayHazardExplosion();

            if (OnDebrisExploded != null)
            {
                OnDebrisExploded(d);
            }

            foreach (Debris other in DebrisList)
            {
                if (other.Id == d.Id) continue;
                double dx = other.X - d.X;
                double dy = other.Y - d.Y;
                double dist = JsMath.Hypot(dx, dy);
                if (dist < 55)
                {
                    double force = (55 - dist) * 8;
                    other.Vx += (dx / (dist + 0.1)) * force;
                    other.Vy += (dy / (dist + 0.1)) * force;
                }
            }
        }

        public void ResolveDebrisCollisions()
        {
            int len = DebrisList.Count;
            for (int i = 0; i < len; i++)
            {
                Debris a = DebrisList[i];
                if (a.State == DebrisState.Removed || a.State == DebrisState.Ingesting) continue;

                for (int j = i + 1; j < len; j++)
                {
                    Debris b = DebrisList[j];
                    if (b.State == DebrisState.Removed || b.State == DebrisState.Ingesting) continue;

                    double dx = b.X - a.X;
                    double dy = b.Y - a.Y;
                    double dist = JsMath.Hypot(dx, dy);
                    double minDist = a.Radius + b.Radius;

                    if (dist < minDist && dist > 0.001)
                    {
                        double overlap = JsMath.Min(minDist - dist, JsMath.Min(a.Radius, b.Radius) * 0.75);
                        double nx = dx / dist;
                        double ny = dy / dist;

                        a.X -= nx * overlap * 0.5;
                        a.Y -= ny * overlap * 0.5;
                        b.X += nx * overlap * 0.5;
                        b.Y += ny * overlap * 0.5;

                        double kx = a.Vx - b.Vx;
                        double ky = a.Vy - b.Vy;
                        double rawP = (2 * (nx * kx + ny * ky)) / (a.Mass + b.Mass);
                        double p = JsMath.Min(38, JsMath.Max(0, rawP));

                        if (p > 0)
                        {
                            a.Vx -= p * b.Mass * nx * 0.8;
                            a.Vy -= p * b.Mass * ny * 0.8;
                            b.Vx += p * a.Mass * nx * 0.8;
                            b.Vy += p * a.Mass * ny * 0.8;
                        }
                    }
                }
            }
        }

        public static double NormalizeAngle(double rad)
        {
            double a = rad % (Math.PI * 2);
            if (a < 0) a += Math.PI * 2;
            return a;
        }

        public static bool IsAngleBetween(double angle, double start, double end)
        {
            if (start <= end)
            {
                return angle >= start && angle <= end;
            }
            return angle >= start || angle <= end;
        }
    }
}
