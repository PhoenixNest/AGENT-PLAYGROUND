#ifndef CANVAS2D_COMMON_INCLUDED
#define CANVAS2D_COMMON_INCLUDED

// Canvas pixel space: origin at the top-left canvas pixel, +y down, one unit = one device pixel.
float4 _C2D_Target;   // width, height, 1/width, 1/height
float  _C2D_FlipY;    // 1 when canvas row 0 is stored in the last memory row

// Per-draw parameters live in one structured buffer (layout must match Canvas2D.Gpu.cs ParamLayout).
StructuredBuffer<float4> _C2D_Params;
float _C2D_ParamBase;

#define C2D_STRIDE      28
#define IDX_RECT        0   // x0 y0 x1 y1 of the quad, canvas px
#define IDX_FLAGS       1   // covMode, fillRule, useClip, shadowTint
#define IDX_ALPHA       2   // globalAlpha, paintKind, gradPremul, gradN
#define IDX_COLOR       3   // straight rgba
#define IDX_SHADOW      4   // shadow colour (straight rgba)
#define IDX_GX0         5
#define IDX_GX1         6
#define IDX_GA          7
#define IDX_GB          8
#define IDX_GCOL        9   // 8 entries
#define IDX_GOFF        17  // 2 entries (8 floats)
#define IDX_EDGE        19  // edgeBase, bboxRight
#define IDX_MASKRECT    20
#define IDX_BLUR        21  // sigma, radius, constValue
#define IDX_IMGA        22  // inverse row0 (a b tx), smoothing
#define IDX_IMGB        23  // inverse row1 (a b ty)
#define IDX_IMGC        24  // src x y w h (texels)
#define IDX_IMGD        25  // dst w h, tex w h
#define IDX_TEXT        26  // mode (0 fill, 1 stroke), stroke half width (device px), contrast

float4 C2D_P(int i) { return _C2D_Params[(uint)(_C2D_ParamBase + 0.5) + (uint)i]; }

float2 C2D_PixelToNdc(float2 p)
{
    float2 uv = p * _C2D_Target.zw;
    float ry = _C2D_FlipY > 0.5 ? 1.0 - uv.y : uv.y;
    float2 ndc;
    ndc.x = uv.x * 2.0 - 1.0;
#if UNITY_UV_STARTS_AT_TOP
    ndc.y = 1.0 - 2.0 * ry;
#else
    ndc.y = 2.0 * ry - 1.0;
#endif
    return ndc;
}

float2 C2D_FragToCanvas(float4 svpos)
{
    float y = _C2D_FlipY > 0.5 ? _C2D_Target.y - svpos.y : svpos.y;
    return float2(svpos.x, y);
}

int2 C2D_CanvasToTexel(int2 pc)
{
    return int2(pc.x, _C2D_FlipY > 0.5 ? (int)_C2D_Target.y - 1 - pc.y : pc.y);
}

float2 C2D_QuadCorner(uint corner, float4 rect)
{
    // two triangles: 0 1 2 / 2 1 3
    uint i = corner == 0 ? 0 : corner == 1 ? 1 : corner == 2 ? 2 : corner == 3 ? 2 : corner == 4 ? 1 : 3;
    float x = (i & 1) ? rect.z : rect.x;
    float y = (i & 2) ? rect.w : rect.y;
    return float2(x, y);
}

float C2D_ApplyRule(float acc, float rule)
{
    float a = abs(acc);
    if (rule > 0.5)
    {
        float t = frac(a * 0.5) * 2.0;
        return t > 1.0 ? 2.0 - t : t;
    }
    return saturate(a);
}

// Standard vertex shader: one quad covering IDX_RECT.
float4 C2D_RectVert(uint vid : SV_VertexID) : SV_POSITION
{
    float2 p = C2D_QuadCorner(vid, C2D_P(IDX_RECT));
    return float4(C2D_PixelToNdc(p), 0, 1);
}

#endif
