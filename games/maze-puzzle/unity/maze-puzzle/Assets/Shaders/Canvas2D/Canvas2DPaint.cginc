#ifndef CANVAS2D_PAINT_INCLUDED
#define CANVAS2D_PAINT_INCLUDED

float4 C2D_StopPremul(float4 c) { return float4(c.rgb * c.a, c.a); }

float4 C2D_EvalStops(float t, int n, float premulMode)
{
    float4 c0 = C2D_P(IDX_GCOL);
    if (t <= C2D_P(IDX_GOFF).x) return C2D_StopPremul(c0);
    float prevOff = C2D_P(IDX_GOFF).x;
    float4 prevCol = c0;
    [loop] for (int i = 1; i < n; i++)
    {
        float4 offv = C2D_P(IDX_GOFF + (i >> 2));
        float o1 = offv[i & 3];
        float4 col = C2D_P(IDX_GCOL + i);
        if (t <= o1)
        {
            float f = o1 > prevOff ? (t - prevOff) / (o1 - prevOff) : 1.0;
            if (premulMode > 0.5)
                return lerp(C2D_StopPremul(prevCol), C2D_StopPremul(col), f);
            float4 s = lerp(prevCol, col, f);
            return C2D_StopPremul(s);
        }
        prevOff = o1;
        prevCol = col;
    }
    return C2D_StopPremul(prevCol);
}

// Premultiplied paint colour at canvas pixel centre pc.
float4 C2D_EvalPaint(float2 pc)
{
    float4 pa = C2D_P(IDX_ALPHA);
    float kind = pa.y;
    if (kind < 0.5)
    {
        float4 c = C2D_P(IDX_COLOR);
        return float4(c.rgb * c.a, c.a);
    }
    if (kind > 2.5) return 0;
    float3 q = float3(pc, 1.0);
    float2 gp = float2(dot(C2D_P(IDX_GX0).xyz, q), dot(C2D_P(IDX_GX1).xyz, q));
    float4 ga = C2D_P(IDX_GA);
    int n = (int)pa.w;
    if (kind < 1.5)
    {
        float2 d = ga.zw;
        float t = dot(gp - ga.xy, d) / dot(d, d);
        return C2D_EvalStops(t, n, pa.z);
    }
    float4 gb = C2D_P(IDX_GB);
    float2 c0 = ga.xy;
    float r0 = ga.z;
    float2 cd = gb.xy;
    float dr = gb.z;
    float2 pd = gp - c0;
    float a = dot(cd, cd) - dr * dr;
    float b = dot(pd, cd) + r0 * dr;
    float c = dot(pd, pd) - r0 * r0;
    float t;
    if (abs(a) < 1e-6)
    {
        if (abs(b) < 1e-9) return 0;
        t = c / (2.0 * b);
        if (r0 + t * dr < 0.0) return 0;
    }
    else
    {
        float disc = b * b - a * c;
        if (disc < 0.0) return 0;
        float sq = sqrt(disc);
        float t1 = (b + sq) / a;
        float t2 = (b - sq) / a;
        float hi = max(t1, t2);
        float lo = min(t1, t2);
        if (r0 + hi * dr >= 0.0) t = hi;
        else if (r0 + lo * dr >= 0.0) t = lo;
        else return 0;
    }
    return C2D_EvalStops(t, n, pa.z);
}

#endif
