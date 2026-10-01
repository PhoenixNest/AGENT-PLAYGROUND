using System;
using System.Collections.Generic;
using OrbitalSalvage.JsCompat;

namespace OrbitalSalvage.Logic
{
    public static class MazeLevelBuilder
    {
        private static double Or(double? v, double fallback)
        {
            return v.HasValue ? v.Value : fallback;
        }

        private static double Norm(double a)
        {
            double v = a % (Math.PI * 2);
            if (v < 0) v += Math.PI * 2;
            return v;
        }

        private static double AngDiff(double a, double b)
        {
            double d = Math.Abs(Norm(a) - Norm(b));
            if (d > Math.PI) d = Math.PI * 2 - d;
            return d;
        }

        private static bool RadOverlap(double min1, double max1, double min2, double max2)
        {
            return !(max1 <= min2 || min1 >= max2);
        }

        public static SectorConfig SanitizeSector(SectorConfig config)
        {
            double MIN_CLEARANCE = 0.12 * Math.PI;
            double MIN_GATE_CLEARANCE = 0.18 * Math.PI;
            List<GateConfig> gates = config.Gates ?? new List<GateConfig>();
            List<DividerConfig> dividers = config.Dividers ?? new List<DividerConfig>();
            List<DeflectorPinConfig> pins = config.DeflectorPins ?? new List<DeflectorPinConfig>();
            List<OneWayFlapConfig> flaps = config.OneWayFlaps ?? new List<OneWayFlapConfig>();
            bool concentric = config.ConcentricRings == true;

            for (int i = 0; i < gates.Count; i++)
            {
                GateConfig gate = gates[i];
                int gi = i;
                double gRMin = Or(gate.RMin, 85);
                double gRMax = Or(gate.RMax, 160);

                double CalcClearance(double testAng)
                {
                    double minD = 999;
                    foreach (DividerConfig div in dividers)
                    {
                        if (concentric && (gate.IsInner != div.IsInner)) continue;
                        double dRMin = Or(div.RMin, 85);
                        double dRMax = Or(div.RMax, 160);
                        if (!RadOverlap(gRMin, gRMax, dRMin, dRMax)) continue;
                        double diff = AngDiff(testAng, div.Angle);
                        if (diff < minD) minD = diff;
                    }
                    for (int j = 0; j < gates.Count; j++)
                    {
                        if (gi == j) continue;
                        GateConfig og = gates[j];
                        if (concentric && (gate.IsInner != og.IsInner)) continue;
                        double oRMin = Or(og.RMin, 85);
                        double oRMax = Or(og.RMax, 160);
                        if (!RadOverlap(gRMin, gRMax, oRMin, oRMax)) continue;
                        double diff = AngDiff(testAng, og.Angle);
                        if (diff < MIN_GATE_CLEARANCE && diff * 0.7 < minD)
                        {
                            minD = diff * 0.7;
                        }
                    }
                    foreach (OneWayFlapConfig flap in flaps)
                    {
                        if (concentric && (gate.IsInner != flap.IsInner)) continue;
                        double fRMin = Or(flap.RMin, 85);
                        double fRMax = Or(flap.RMax, 160);
                        if (!RadOverlap(gRMin, gRMax, fRMin, fRMax)) continue;
                        double diff = AngDiff(testAng, flap.Angle);
                        if (diff < minD) minD = diff;
                    }
                    return minD;
                }

                double curClearance = CalcClearance(gate.Angle);
                if (curClearance < MIN_CLEARANCE)
                {
                    double bestAng = gate.Angle;
                    double bestDist = curClearance;
                    for (double delta = -0.5 * Math.PI; delta <= 0.5 * Math.PI; delta += 0.01 * Math.PI)
                    {
                        double testAng = gate.Angle + delta;
                        double testMin = CalcClearance(testAng);
                        if (testMin > bestDist)
                        {
                            bestDist = testMin;
                            bestAng = testAng;
                        }
                    }
                    gate.Angle = bestAng;
                }
            }

            for (int pass = 0; pass < 3; pass++)
            {
                for (int i = 0; i < gates.Count; i++)
                {
                    for (int j = i + 1; j < gates.Count; j++)
                    {
                        GateConfig g1 = gates[i];
                        GateConfig g2 = gates[j];
                        if (concentric && g1.IsInner != g2.IsInner) continue;
                        double r1Min = Or(g1.RMin, 85);
                        double r1Max = Or(g1.RMax, 160);
                        double r2Min = Or(g2.RMin, 85);
                        double r2Max = Or(g2.RMax, 160);
                        if (!RadOverlap(r1Min, r1Max, r2Min, r2Max)) continue;

                        double diff = AngDiff(g1.Angle, g2.Angle);
                        if (diff < MIN_GATE_CLEARANCE)
                        {
                            double bestAng = g2.Angle;
                            double bestDist = -1;
                            double[] dirs = { 1, -1 };
                            foreach (double dir in dirs)
                            {
                                for (double step = 0.04 * Math.PI; step <= 0.5 * Math.PI; step += 0.02 * Math.PI)
                                {
                                    double testAng = g1.Angle + dir * (MIN_GATE_CLEARANCE + step);
                                    double minDivD = 999;
                                    foreach (DividerConfig div in dividers)
                                    {
                                        if (concentric && (g2.IsInner != div.IsInner)) continue;
                                        double dRMin = Or(div.RMin, 85);
                                        double dRMax = Or(div.RMax, 160);
                                        if (!RadOverlap(r2Min, r2Max, dRMin, dRMax)) continue;
                                        double dDiff = AngDiff(testAng, div.Angle);
                                        if (dDiff < minDivD) minDivD = dDiff;
                                    }
                                    if (minDivD >= MIN_CLEARANCE)
                                    {
                                        double sep1 = AngDiff(testAng, g1.Angle);
                                        if (sep1 > bestDist)
                                        {
                                            bestDist = sep1;
                                            bestAng = testAng;
                                        }
                                    }
                                }
                            }
                            g2.Angle = bestAng;
                        }
                    }
                }
            }

            foreach (DeflectorPinConfig pin in pins)
            {
                foreach (GateConfig gate in gates)
                {
                    if (concentric && (pin.IsInner != gate.IsInner)) continue;
                    double gRMin = Or(gate.RMin, 85);
                    double gRMax = Or(gate.RMax, 160);
                    if (pin.R < gRMin - 12 || pin.R > gRMax + 12) continue;

                    double diff = AngDiff(pin.Angle, gate.Angle);
                    if (diff < MIN_CLEARANCE)
                    {
                        double bestAng = pin.Angle;
                        double bestDist = diff;
                        for (double delta = -0.35 * Math.PI; delta <= 0.35 * Math.PI; delta += 0.01 * Math.PI)
                        {
                            double testAng = pin.Angle + delta;
                            double testDiff = AngDiff(testAng, gate.Angle);
                            if (testDiff > bestDist)
                            {
                                bestDist = testDiff;
                                bestAng = testAng;
                            }
                        }
                        pin.Angle = bestAng;
                    }
                }
            }

            return config;
        }

        public static List<SectorConfig> GetSectors()
        {
            var levels = new List<SectorConfig>();
            for (int i = 1; i <= 50; i++)
            {
                levels.Add(BuildLevel(i));
            }
            return levels;
        }

        public static List<WorldInfo> GetSectorWorlds()
        {
            return new List<WorldInfo>
            {
                new WorldInfo { Id = "world_1", WorldIndex = 1, Title = "Bubblegum Belt", Tagline = "Orbital Gravity Basics & Clean Flow", Icon = "\U0001F36C", Color = "#f472b6", LevelRangeStart = 1, LevelRangeEnd = 10 },
                new WorldInfo { Id = "world_2", WorldIndex = 2, Title = "Cheese Moon Chutes", Tagline = "Switchbacks & One-Way Flappers", Icon = "\U0001F9C0", Color = "#facc15", LevelRangeStart = 11, LevelRangeEnd = 20 },
                new WorldInfo { Id = "world_3", WorldIndex = 3, Title = "Cyber Donut Rings", Tagline = "Concentric Planetary Dual-Track Gearing", Icon = "\U0001F369", Color = "#38bdf8", LevelRangeStart = 21, LevelRangeEnd = 30 },
                new WorldInfo { Id = "world_4", WorldIndex = 4, Title = "Nebula Pinball", Tagline = "Pinball Bumpers, Forking Gauntlets & Boost", Icon = "\U0001F30C", Color = "#c084fc", LevelRangeStart = 31, LevelRangeEnd = 40 },
                new WorldInfo { Id = "world_5", WorldIndex = 5, Title = "Singularity Horizon", Tagline = "Master Dual-Ring Orbital Gauntlets", Icon = "\U0001F573\uFE0F", Color = "#4ade80", LevelRangeStart = 41, LevelRangeEnd = 50 },
            };
        }

        public static SectorConfig BuildLevel(int levelNumber)
        {
            double worldIndex = Math.Ceiling(levelNumber / 10.0);
            int subLevel = ((levelNumber - 1) % 10) + 1;
            SectorConfig config;

            if (worldIndex == 1)
            {
                config = BuildWorld1Level(levelNumber, subLevel);
            }
            else if (worldIndex == 2)
            {
                config = BuildWorld2Level(levelNumber, subLevel);
            }
            else if (worldIndex == 3)
            {
                config = BuildWorld3Level(levelNumber, subLevel);
            }
            else if (worldIndex == 4)
            {
                config = BuildWorld4Level(levelNumber, subLevel);
            }
            else
            {
                config = BuildWorld5Level(levelNumber, subLevel);
            }

            config.RewardCredits = 250 + (levelNumber - 1) * 5;

            return SanitizeSector(config);
        }

        private static DividerConfig Div(double rMin, double rMax, double angle, bool? isInner = null)
        {
            return new DividerConfig { RMin = rMin, RMax = rMax, Angle = angle, IsInner = isInner };
        }

        private static GateConfig Gate(string type, double value, double angle, double rMin, double rMax, string color, bool? isInner = null)
        {
            return new GateConfig { Type = type, Value = value, Angle = angle, RMin = rMin, RMax = rMax, IsInner = isInner, Color = color };
        }

        private static OneWayFlapConfig Flap(double rMin, double rMax, double angle, double maxSwing, double normalSign, bool? isInner = null)
        {
            return new OneWayFlapConfig { RMin = rMin, RMax = rMax, Angle = angle, MaxSwing = maxSwing, NormalSign = normalSign, IsInner = isInner };
        }

        private static DeflectorPinConfig Pin(double r, double angle, double radius, double restitution, string color, string accent, bool? isInner = null)
        {
            return new DeflectorPinConfig { R = r, Angle = angle, Radius = radius, Restitution = restitution, IsInner = isInner, Color = color, Accent = accent };
        }

        private static List<string> Types(params string[] t)
        {
            return new List<string>(t);
        }

        public static SectorConfig BuildWorld1Level(int levelNum, int sub)
        {
            double ringRadius = 155;
            double innerRadius = 82;
            double quota = 20 + (sub - 1) * 6;
            double initialDebrisCount = 10 + sub;
            double apertureAngle = Math.PI * (0.32 - sub * 0.005);

            var dividers = new List<DividerConfig>
            {
                Div(85, 155, -Math.PI * 0.5),
                Div(85, 125, 0),
                Div(115, 155, Math.PI * (0.32 + sub * 0.005)),
                Div(85, 125, Math.PI),
                Div(115, 155, Math.PI * (0.68 - sub * 0.005)),
            };

            if (sub >= 4)
            {
                dividers.Add(Div(115, 155, -Math.PI * 0.28));
            }
            if (sub >= 7)
            {
                dividers.Add(Div(115, 155, Math.PI * 1.28));
            }

            var gates = new List<GateConfig>
            {
                Gate("MULTIPLY", 2, -Math.PI * 0.12, 85, 155, "#facc15"),
                Gate("ADD", 3 + Math.Floor(sub * 0.5), Math.PI * 0.20, 85, 155, "#38bdf8"),
                Gate("MULTIPLY", sub >= 5 ? 3 : 2, Math.PI * 0.85, 85, 155, "#f472b6"),
            };

            return new SectorConfig
            {
                Id = "level_" + levelNum,
                LevelNumber = levelNum,
                WorldIndex = 1,
                Name = "Bubblegum Chute " + sub,
                Code = "LVL " + levelNum,
                Description = "Tilt the centrifuge gently to funnel candy scrap into Nibbles!",
                Quota = quota,
                InitialDebrisCount = initialDebrisCount,
                InitialDebrisTypes = sub >= 5 ? Types("titanium", "solar") : Types("titanium"),
                RingRadius = ringRadius,
                CorridorWidth = 36,
                InnerRadius = innerRadius,
                ApertureAngle = apertureAngle,
                Dividers = dividers,
                Gates = gates,
            };
        }

        public static SectorConfig BuildWorld2Level(int levelNum, int sub)
        {
            double ringRadius = 162;
            double innerRadius = 86;
            double quota = 65 + (sub - 1) * 7;
            double initialDebrisCount = 14 + sub;
            double apertureAngle = Math.PI * 0.28;

            var dividers = new List<DividerConfig>
            {
                Div(86, 162, -Math.PI * 0.5),
                Div(86, 132, -Math.PI * 0.20),
                Div(120, 162, Math.PI * 0.12),
                Div(86, 132, Math.PI * 0.42),
                Div(86, 132, Math.PI * 1.20),
                Div(120, 162, Math.PI * 0.88),
                Div(86, 132, Math.PI * 0.60),
            };

            if (sub >= 5)
            {
                dividers.Add(Div(120, 162, -Math.PI * 0.48));
                dividers.Add(Div(120, 162, Math.PI * 1.48));
            }

            var gates = new List<GateConfig>
            {
                Gate("MULTIPLY", 3, -Math.PI * 0.35, 120, 162, "#f472b6"),
                Gate("ADD", 5 + Math.Floor(sub * 0.6), -Math.PI * 0.04, 86, 125, "#38bdf8"),
                Gate("MULTIPLY", 2, Math.PI * 0.27, 120, 162, "#4ade80"),
                Gate("MULTIPLY", sub >= 6 ? 3 : 2, Math.PI * 1.04, 86, 162, "#facc15"),
            };

            var oneWayFlaps = new List<OneWayFlapConfig>();
            if (sub >= 4)
            {
                oneWayFlaps.Add(Flap(86, 132, Math.PI * 0.25, Math.PI * 0.35, 1));
            }
            if (sub >= 7)
            {
                oneWayFlaps.Add(Flap(86, 132, Math.PI * 0.75, Math.PI * 0.35, -1));
            }

            return new SectorConfig
            {
                Id = "level_" + levelNum,
                LevelNumber = levelNum,
                WorldIndex = 2,
                Name = "Moon Chute " + sub,
                Code = "LVL " + levelNum,
                Description = "Watch the spicy batteries! Rock back and forth through switchbacks and one-way valves!",
                Quota = quota,
                InitialDebrisCount = initialDebrisCount,
                InitialDebrisTypes = Types("titanium", "solar", "volatile"),
                RingRadius = ringRadius,
                CorridorWidth = 38,
                InnerRadius = innerRadius,
                ApertureAngle = apertureAngle,
                Dividers = dividers,
                Gates = gates,
                OneWayFlaps = oneWayFlaps,
            };
        }

        public static SectorConfig BuildWorld3Level(int levelNum, int sub)
        {
            double ringRadius = 172;
            double innerRadius = 90;
            double trackRadius = 131;
            double quota = 105 + (sub - 1) * 9;
            double initialDebrisCount = 16 + sub;
            double apertureAngle = Math.PI * 0.26;

            double gapAngle = sub % 2 == 1 ? -Math.PI * 0.5 : 0;
            var trackWalls = new List<TrackWallConfig>
            {
                new TrackWallConfig { R = trackRadius, GapAngle = gapAngle, GapSize = Math.PI * (0.44 - sub * 0.01) },
            };

            var dividers = new List<DividerConfig>
            {
                Div(90, 131, -Math.PI * 0.15, true),
                Div(90, 131, Math.PI * 0.45, true),
                Div(90, 131, Math.PI * 1.15, true),
                Div(131, 172, -Math.PI * 0.15, false),
                Div(131, 172, Math.PI * 0.25, false),
                Div(131, 172, Math.PI * 0.75, false),
                Div(131, 172, Math.PI * 1.15, false),
            };

            if (sub >= 6)
            {
                dividers.Add(Div(131, 172, -Math.PI * 0.4, false));
            }

            var gates = new List<GateConfig>
            {
                Gate("MULTIPLY", 2, Math.PI * 0.15, 90, 131, "#38bdf8", true),
                Gate("ADD", 8 + sub, 0.04, 131, 172, "#facc15", false),
                Gate("MULTIPLY", 3, Math.PI * 0.95, 131, 172, "#c084fc", false),
                Gate("MULTIPLY", 2, -Math.PI * 0.28, 131, 172, "#4ade80", false),
            };

            var deflectorPins = new List<DeflectorPinConfig>
            {
                Pin(152, Math.PI * 0.50, 9.5, 1.35, "#38bdf8", "#0284c7", false),
                Pin(110, -Math.PI * 0.45, 9, 1.3, "#facc15", "#eab308", true),
            };

            return new SectorConfig
            {
                Id = "level_" + levelNum,
                LevelNumber = levelNum,
                WorldIndex = 3,
                Name = "Cyber Orbit " + sub,
                Code = "LVL " + levelNum,
                Description = "Planetary Gears! Inner and outer tracks counter-rotate. Align airlocks to launch scrap!",
                Quota = quota,
                InitialDebrisCount = initialDebrisCount,
                InitialDebrisTypes = Types("titanium", "solar", "magnetic"),
                RingRadius = ringRadius,
                CorridorWidth = 41,
                InnerRadius = innerRadius,
                ApertureAngle = apertureAngle,
                ConcentricRings = true,
                GearRatio = -1.0,
                TrackWalls = trackWalls,
                Dividers = dividers,
                Gates = gates,
                DeflectorPins = deflectorPins,
            };
        }

        public static SectorConfig BuildWorld4Level(int levelNum, int sub)
        {
            double ringRadius = 176;
            double innerRadius = 92;
            double quota = 155 + (sub - 1) * 11;
            double initialDebrisCount = 18 + sub;
            double apertureAngle = Math.PI * 0.25;

            var dividers = new List<DividerConfig>
            {
                Div(92, 176, -Math.PI * 0.5),
                Div(92, 145, Math.PI * 1.25),
                Div(125, 176, Math.PI * 0.92),
                Div(92, 145, Math.PI * 0.60),
                Div(120, 176, -Math.PI * 0.26),
                Div(92, 148, Math.PI * 0.06),
                Div(125, 176, Math.PI * 0.38),
            };

            if (sub >= 4)
            {
                dividers.Add(Div(92, 140, -Math.PI * 0.40));
            }
            if (sub >= 7)
            {
                dividers.Add(Div(92, 140, Math.PI * 1.42));
            }

            var gates = new List<GateConfig>
            {
                Gate("ADD", 10 + sub, Math.PI * 1.08, 92, 176, "#38bdf8"),
                Gate("MULTIPLY", sub >= 5 ? 4 : 3, -Math.PI * 0.10, 92, 176, "#f472b6"),
                Gate("MULTIPLY", 2, Math.PI * 0.22, 92, 176, "#facc15"),
                Gate("MULTIPLY", 2, Math.PI * 0.76, 92, 176, "#4ade80"),
            };

            var deflectorPins = new List<DeflectorPinConfig>
            {
                Pin(135, -Math.PI * 0.38, 10.5, 1.4, "#ec4899", "#f43f5e"),
                Pin(135, Math.PI * 0.54, 10.5, 1.4, "#facc15", "#eab308"),
                Pin(110, Math.PI * 1.42, 9.5, 1.35, "#06b6d4", "#0891b2"),
            };

            if (sub >= 4)
            {
                deflectorPins.Add(Pin(155, -Math.PI * 0.48, 11, 1.45, "#a855f7", "#9333ea"));
            }

            if (sub >= 3)
            {
                gates.Add(Gate("BOOST", 160, Math.PI * 1.40, 92, 130, "#84cc16"));
            }

            var oneWayFlaps = new List<OneWayFlapConfig>
            {
                Flap(92, 145, Math.PI * 0.76, Math.PI * 0.35, 1),
            };

            return new SectorConfig
            {
                Id = "level_" + levelNum,
                LevelNumber = levelNum,
                WorldIndex = 4,
                Name = "Nebula Pinball " + sub,
                Code = "LVL " + levelNum,
                Description = "Pinball bumpers & forking routes! Ricochet scrap through multipliers and slingshot gates!",
                Quota = quota,
                InitialDebrisCount = initialDebrisCount,
                InitialDebrisTypes = Types("titanium", "solar", "volatile", "magnetic"),
                RingRadius = ringRadius,
                CorridorWidth = 42,
                InnerRadius = innerRadius,
                ApertureAngle = apertureAngle,
                Dividers = dividers,
                Gates = gates,
                DeflectorPins = deflectorPins,
                OneWayFlaps = oneWayFlaps,
            };
        }

        public static SectorConfig BuildWorld5Level(int levelNum, int sub)
        {
            double ringRadius = 180;
            double innerRadius = 94;
            double trackRadius = 137;
            double quota = 210 + (sub - 1) * 16;
            double initialDebrisCount = 20 + sub;
            double apertureAngle = Math.PI * 0.24;

            var trackWalls = new List<TrackWallConfig>
            {
                new TrackWallConfig { R = trackRadius, GapAngle = -Math.PI * 0.5 + (sub % 2 == 0 ? 0.3 : -0.3), GapSize = Math.PI * 0.4 },
            };

            var dividers = new List<DividerConfig>
            {
                Div(94, 137, -Math.PI * 0.12, true),
                Div(94, 137, Math.PI * 0.42, true),
                Div(94, 137, Math.PI * 0.82, true),
                Div(94, 137, Math.PI * 1.22, true),
                Div(137, 180, -Math.PI * 0.32, false),
                Div(137, 180, 0, false),
                Div(137, 180, Math.PI * 0.24, false),
                Div(137, 180, Math.PI * 0.76, false),
                Div(137, 180, Math.PI * 1.06, false),
                Div(137, 180, Math.PI * 1.36, false),
            };

            var gates = new List<GateConfig>
            {
                Gate("MULTIPLY", 3, Math.PI * 0.15, 94, 137, "#c084fc", true),
                Gate("ADD", 12 + sub, -Math.PI * 0.16, 137, 180, "#facc15", false),
                Gate("MULTIPLY", 3, Math.PI * 0.90, 137, 180, "#f472b6", false),
                Gate("MULTIPLY", 4, -Math.PI * 0.48, 137, 180, "#4ade80", false),
            };

            bool isFinale = levelNum == 50;

            var deflectorPins = new List<DeflectorPinConfig>
            {
                Pin(115, Math.PI * 0.35, 10, 1.4, "#ec4899", "#f43f5e", true),
                Pin(160, 0.12, 11, 1.45, "#facc15", "#eab308", false),
                Pin(160, Math.PI * 1.22, 10, 1.4, "#06b6d4", "#0891b2", false),
                Pin(115, Math.PI * 1.02, 10, 1.35, "#a855f7", "#9333ea", true),
            };

            if (sub >= 5)
            {
                deflectorPins.Add(Pin(158, Math.PI * 0.46, 12, 1.5, "#f43f5e", "#e11d48", false));
            }

            if (sub >= 2)
            {
                gates.Add(Gate("BOOST", 180, -Math.PI * 0.48, 94, 137, "#84cc16", true));
            }

            var oneWayFlaps = new List<OneWayFlapConfig>
            {
                Flap(137, 180, Math.PI * 0.24, Math.PI * 0.35, 1, false),
                Flap(94, 137, Math.PI * 0.82, Math.PI * 0.35, 1, true),
            };

            return new SectorConfig
            {
                Id = "level_" + levelNum,
                LevelNumber = levelNum,
                WorldIndex = 5,
                Name = isFinale ? "The Kessler Singularity" : "Horizon Sector " + sub,
                Code = "LVL " + levelNum,
                Description = isFinale
                    ? "\u2B50 GRAND FINALE! Clear humanity's greatest orbital debris cascade to save Earth!"
                    : "Master planetary gear gauntlet! Route high-density debris through multi-stage shredders!",
                Quota = quota,
                InitialDebrisCount = initialDebrisCount,
                InitialDebrisTypes = Types("titanium", "solar", "volatile", "magnetic"),
                RingRadius = ringRadius,
                CorridorWidth = 43,
                InnerRadius = innerRadius,
                ApertureAngle = apertureAngle,
                ConcentricRings = true,
                GearRatio = -1.25,
                TrackWalls = trackWalls,
                Dividers = dividers,
                Gates = gates,
                DeflectorPins = deflectorPins,
                OneWayFlaps = oneWayFlaps,
                IsFinale = isFinale,
            };
        }

        public static SectorConfig BuildDailyShift(DailyShiftInfo dailyInfo)
        {
            double seed = (dailyInfo.Seed.HasValue && dailyInfo.Seed.Value != 0 && !double.IsNaN(dailyInfo.Seed.Value)) ? dailyInfo.Seed.Value : 12345;
            double s = seed;
            Func<double> rnd = () =>
            {
                s = (s * 1664525 + 1013904223) % 4294967296;
                return s / 4294967296;
            };

            string tier = string.IsNullOrEmpty(dailyInfo.Tier) ? "silver" : dailyInfo.Tier;
            int numDividers;
            double apertureAngle;
            double initialDebrisCount;
            List<string> initialDebrisTypes;
            int numGates;

            if (tier == "bronze")
            {
                numDividers = 4;
                apertureAngle = Math.PI * 0.35;
                initialDebrisCount = 12;
                initialDebrisTypes = Types("solar", "titanium");
                numGates = 2;
            }
            else if (tier == "gold")
            {
                numDividers = 8;
                apertureAngle = Math.PI * 0.22;
                initialDebrisCount = 22;
                initialDebrisTypes = Types("titanium", "solar", "volatile");
                numGates = 4;
            }
            else
            {
                numDividers = 6;
                apertureAngle = Math.PI * 0.28;
                initialDebrisCount = 16;
                initialDebrisTypes = Types("titanium", "solar", "volatile");
                numGates = 3;
            }

            var dividers = new List<DividerConfig>();
            double step = (Math.PI * 2) / numDividers;

            double apCenter = Math.PI * 0.5;
            double apHalfWidth = apertureAngle * 0.5 + 0.08;

            double[] gateAngles = { -Math.PI * 0.5, step * 0.5, step * 2.5, step * 1.5 };

            for (int i = 0; i < numDividers; i++)
            {
                double ang = i * step + (rnd() - 0.5) * 0.08;
                double diff = Math.Abs(ang - apCenter);
                while (diff > Math.PI) diff = Math.PI * 2 - diff;
                if (diff < apHalfWidth) continue;

                bool tooCloseToGate = false;
                for (int g = 0; g < numGates; g++)
                {
                    double ga = gateAngles[g];
                    double gDiff = Math.Abs(ang - ga);
                    while (gDiff > Math.PI) gDiff = Math.PI * 2 - gDiff;
                    if (gDiff < 0.16 * Math.PI)
                    {
                        tooCloseToGate = true;
                        break;
                    }
                }
                if (tooCloseToGate) continue;

                double mid = 125;
                if (rnd() > 0.5)
                {
                    dividers.Add(Div(88, mid, ang));
                }
                else
                {
                    dividers.Add(Div(mid, 165, ang));
                }
            }

            List<GateConfig> gates;
            List<OneWayFlapConfig> oneWayFlaps;

            if (tier == "bronze")
            {
                gates = new List<GateConfig>
                {
                    Gate("MULTIPLY", 2, gateAngles[0], 88, 165, "#facc15"),
                    Gate("ADD", 5, gateAngles[1], 88, 165, "#38bdf8"),
                };
                oneWayFlaps = new List<OneWayFlapConfig>();
            }
            else if (tier == "gold")
            {
                gates = new List<GateConfig>();
                gates.Add(Gate("MULTIPLY", 3, gateAngles[0], 88, 165, "#f472b6"));
                gates.Add(Gate("ADD", 10 + Math.Floor(rnd() * 6), gateAngles[1], 88, 165, "#38bdf8"));
                gates.Add(Gate("MULTIPLY", 2, gateAngles[2], 88, 165, "#facc15"));
                gates.Add(Gate("MULTIPLY", 2, gateAngles[3], 88, 165, "#4ade80"));
                oneWayFlaps = new List<OneWayFlapConfig>
                {
                    Flap(88, 135, -0.15 * Math.PI, Math.PI * 0.35, 1),
                    Flap(120, 165, 0.85 * Math.PI, Math.PI * 0.35, -1),
                };
            }
            else
            {
                gates = new List<GateConfig>();
                gates.Add(Gate("MULTIPLY", 2 + Math.Floor(rnd() * 2), gateAngles[1], 88, 165, "#facc15"));
                gates.Add(Gate("ADD", 5 + Math.Floor(rnd() * 5), gateAngles[2], 88, 165, "#38bdf8"));
                gates.Add(Gate("MULTIPLY", 3, gateAngles[0], 88, 165, "#f472b6"));
                oneWayFlaps = new List<OneWayFlapConfig>
                {
                    Flap(88, 135, -0.15 * Math.PI, Math.PI * 0.35, 1),
                };
            }

            var config = new SectorConfig
            {
                Id = string.IsNullOrEmpty(dailyInfo.Id) ? "daily_shift" : dailyInfo.Id,
                Name = dailyInfo.Title,
                Code = tier.ToUpperInvariant(),
                Description = "Daily Contract: " + (dailyInfo.HazardModifier ?? "undefined"),
                Quota = dailyInfo.Quota.GetValueOrDefault(),
                InitialDebrisCount = initialDebrisCount,
                InitialDebrisTypes = initialDebrisTypes,
                RingRadius = 165,
                CorridorWidth = 38,
                InnerRadius = 89,
                ApertureAngle = apertureAngle,
                Dividers = dividers,
                Gates = gates,
                OneWayFlaps = oneWayFlaps,
                IsDaily = true,
                DailyTier = tier,
                RewardCredits = dailyInfo.RewardCredits,
            };

            return SanitizeSector(config);
        }
    }
}
