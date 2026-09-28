using System;
using UnityEngine;

namespace OrbitalSalvage.Data
{
    /// <summary>
    /// Port of MazeLevelBuilder.sanitizeSector() from web prototype.
    /// Inspects and resolves angular overlaps between gates, dividers, and pins.
    /// </summary>
    public static class MazeSanitizer
    {
        private static float NormalizeAngle(float a)
        {
            float v = a % (Mathf.PI * 2f);
            if (v < 0) v += Mathf.PI * 2f;
            return v;
        }

        private static float AngleDifference(float a, float b)
        {
            float d = Mathf.Abs(NormalizeAngle(a) - NormalizeAngle(b));
            if (d > Mathf.PI) d = Mathf.PI * 2f - d;
            return d;
        }

        private static bool RadialOverlap(float min1, float max1, float min2, float max2)
        {
            return !(max1 <= min2 || min1 >= max2);
        }

        public static SectorDefinition Sanitize(SectorDefinition sector)
        {
            if (sector == null) return null;

            const float MIN_CLEARANCE = 0.12f * Mathf.PI;      // ~21.6 deg from dividers
            const float MIN_GATE_CLEARANCE = 0.18f * Mathf.PI; // ~32.4 deg between gates

            var gates = sector.gates;
            var dividers = sector.dividers;
            var pins = sector.deflectorPins;
            var flaps = sector.oneWayFlaps;

            // 1. Joint Radial Slot Search for each gate
            for (int i = 0; i < gates.Count; i++)
            {
                var gate = gates[i];
                float gRMin = gate.rMin;
                float gRMax = gate.rMax;

                float CalcClearance(float testAng)
                {
                    float minD = 999f;
                    // Dividers
                    foreach (var div in dividers)
                    {
                        if (sector.concentricRings && (gate.isInner != div.isInner)) continue;
                        if (!RadialOverlap(gRMin, gRMax, div.rMin, div.rMax)) continue;
                        float diff = AngleDifference(testAng, div.angle);
                        if (diff < minD) minD = diff;
                    }
                    // Other gates
                    for (int j = 0; j < gates.Count; j++)
                    {
                        if (i == j) continue;
                        var og = gates[j];
                        if (sector.concentricRings && (gate.isInner != og.isInner)) continue;
                        if (!RadialOverlap(gRMin, gRMax, og.rMin, og.rMax)) continue;
                        float diff = AngleDifference(testAng, og.angle);
                        if (diff < MIN_GATE_CLEARANCE && diff * 0.7f < minD)
                        {
                            minD = diff * 0.7f;
                        }
                    }
                    // Flaps
                    foreach (var flap in flaps)
                    {
                        if (sector.concentricRings && (gate.isInner != flap.isInner)) continue;
                        if (!RadialOverlap(gRMin, gRMax, flap.rMin, flap.rMax)) continue;
                        float diff = AngleDifference(testAng, flap.angle);
                        if (diff < minD) minD = diff;
                    }
                    return minD;
                }

                float curClearance = CalcClearance(gate.angle);
                if (curClearance < MIN_CLEARANCE)
                {
                    float bestAng = gate.angle;
                    float bestDist = curClearance;
                    for (float delta = -0.5f * Mathf.PI; delta <= 0.5f * Mathf.PI; delta += 0.01f * Mathf.PI)
                    {
                        float testAng = gate.angle + delta;
                        float testMin = CalcClearance(testAng);
                        if (testMin > bestDist)
                        {
                            bestDist = testMin;
                            bestAng = testAng;
                        }
                    }
                    gate.angle = bestAng;
                }
            }

            // 2. Strict Gate-to-Gate Separation Pass
            for (int pass = 0; pass < 3; pass++)
            {
                for (int i = 0; i < gates.Count; i++)
                {
                    for (int j = i + 1; j < gates.Count; j++)
                    {
                        var g1 = gates[i];
                        var g2 = gates[j];
                        if (sector.concentricRings && g1.isInner != g2.isInner) continue;
                        if (!RadialOverlap(g1.rMin, g1.rMax, g2.rMin, g2.rMax)) continue;

                        float diff = AngleDifference(g1.angle, g2.angle);
                        if (diff < MIN_GATE_CLEARANCE)
                        {
                            float bestAng = g2.angle;
                            float bestDist = -1f;
                            int[] dirs = new int[] { 1, -1 };
                            foreach (int dir in dirs)
                            {
                                for (float step = 0.04f * Mathf.PI; step <= 0.5f * Mathf.PI; step += 0.02f * Mathf.PI)
                                {
                                    float testAng = g1.angle + dir * (MIN_GATE_CLEARANCE + step);
                                    float minDivD = 999f;
                                    foreach (var div in dividers)
                                    {
                                        if (sector.concentricRings && (g2.isInner != div.isInner)) continue;
                                        if (!RadialOverlap(g2.rMin, g2.rMax, div.rMin, div.rMax)) continue;
                                        float dDiff = AngleDifference(testAng, div.angle);
                                        if (dDiff < minDivD) minDivD = dDiff;
                                    }
                                    if (minDivD >= MIN_CLEARANCE)
                                    {
                                        float sep1 = AngleDifference(testAng, g1.angle);
                                        if (sep1 > bestDist)
                                        {
                                            bestDist = sep1;
                                            bestAng = testAng;
                                        }
                                    }
                                }
                            }
                            g2.angle = bestAng;
                        }
                    }
                }
            }

            // 3. Resolve Gate-Pin overlaps
            foreach (var pin in pins)
            {
                foreach (var gate in gates)
                {
                    if (sector.concentricRings && (pin.isInner != gate.isInner)) continue;
                    if (pin.r < gate.rMin - 12f || pin.r > gate.rMax + 12f) continue;

                    float diff = AngleDifference(pin.angle, gate.angle);
                    if (diff < MIN_CLEARANCE)
                    {
                        float bestAng = pin.angle;
                        float bestDist = diff;
                        for (float delta = -0.35f * Mathf.PI; delta <= 0.35f * Mathf.PI; delta += 0.01f * Mathf.PI)
                        {
                            float testAng = pin.angle + delta;
                            float testDiff = AngleDifference(testAng, gate.angle);
                            if (testDiff > bestDist)
                            {
                                bestDist = testDiff;
                                bestAng = testAng;
                            }
                        }
                        pin.angle = bestAng;
                    }
                }
            }

            return sector;
        }
    }
}
