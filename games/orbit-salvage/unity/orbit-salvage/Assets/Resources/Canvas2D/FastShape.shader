Shader "Hidden/Canvas2D/FastShape"
{
    // One quad per primitive. flags.x: 0 rect, 1 ellipse, 2 rounded rect, 3 capsule, 4 ellipse ring, 5 circular arc stroke.
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex C2D_RectVert
            #pragma fragment frag
            #pragma target 4.5
            #include "UnityCG.cginc"
            #include "../../Shaders/Canvas2D/Canvas2DCommon.cginc"

            Texture2D<float> _C2D_ClipTex;

            float SdRound(float2 p, float2 b, float rad)
            {
                float2 q = abs(p) - b + rad;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - rad;
            }

            float SdEllipse(float2 p, float2 r)
            {
                float2 rq = p / max(r, float2(0.5, 0.5));
                return (length(rq) - 1.0) * min(r.x, r.y);
            }

            // gb: start angle, signed sweep, half width, cap (0 butt, 1 round, 2 square). Canvas +y down.
            float SdArc(float2 p, float a0, float sweep, float radius, float hw, float cap)
            {
                float mid = a0 + sweep * 0.5;
                float co = cos(mid), si = sin(mid);
                float2 q = float2(co * p.x + si * p.y, -si * p.x + co * p.y);
                float halfAng = abs(sweep) * 0.5;
                float ap = atan2(q.y, q.x);
                float radial = abs(length(q) - radius);
                float limit = halfAng;
                if (cap > 1.5) limit += hw / max(radius, 0.5);
                if (abs(ap) <= limit) return radial - hw;
                if (cap < 0.5 || cap > 1.5) return 1.0;
                float endAng = ap > 0.0 ? halfAng : -halfAng;
                float2 capP = float2(cos(endAng), sin(endAng)) * radius;
                return length(q - capP) - hw;
            }

            float SdFast(float2 pc, float mode, float4 ga, float4 gb)
            {
                float sd = -1.0;
                if (mode < 1.5)
                {
                    float ang = gb.x;
                    float co = cos(ang), si = sin(ang);
                    float2 d = pc - ga.xy;
                    float2 q = float2(co * d.x + si * d.y, -si * d.x + co * d.y);
                    sd = SdEllipse(q, ga.zw);
                }
                else if (mode < 2.5)
                {
                    float2 half = ga.zw * 0.5;
                    float2 p = pc - (ga.xy + half);
                    float rad = p.x < 0.0 ? (p.y < 0.0 ? gb.x : gb.w) : (p.y < 0.0 ? gb.y : gb.z);
                    rad = min(max(rad, 0.0), min(half.x, half.y));
                    sd = SdRound(p, half, rad);
                }
                else if (mode < 3.5)
                {
                    float2 a = ga.xy, b = ga.zw;
                    float2 pa = pc - a, ba = b - a;
                    float denom = max(dot(ba, ba), 1e-4);
                    float h = saturate(dot(pa, ba) / denom);
                    sd = length(pa - ba * h) - gb.x;
                }
                else if (mode < 4.5)
                {
                    float ang = gb.x;
                    float co = cos(ang), si = sin(ang);
                    float2 d = pc - ga.xy;
                    float2 q = float2(co * d.x + si * d.y, -si * d.x + co * d.y);
                    sd = abs(SdEllipse(q, ga.zw)) - gb.y;
                }
                else
                {
                    sd = SdArc(pc - ga.xy, gb.x, gb.y, ga.z, gb.z, gb.w);
                }
                if (mode < 0.5) sd = -10.0;
                return sd;
            }

            float4 Shade(float2 pc, float mode, float4 ga, float4 gb, float4 fl, float4 sc, float alpha)
            {
                float cov = saturate(0.5 - SdFast(pc, mode, ga, gb));
                if (fl.z > 0.5)
                    cov *= _C2D_ClipTex.Load(int3(C2D_CanvasToTexel(int2(floor(pc))), 0));
                float a = cov * sc.a * alpha;
                return float4(sc.rgb * a, a);
            }

            float4 frag(float4 svpos : SV_POSITION) : SV_Target
            {
                float2 pc = C2D_FragToCanvas(svpos);
                float4 fl = C2D_P(IDX_FLAGS);
                return Shade(pc, fl.x, C2D_P(IDX_GA), C2D_P(IDX_GB), fl, C2D_P(IDX_COLOR), C2D_P(IDX_ALPHA).x);
            }
            ENDHLSL
        }

        // One draw for a run of analytic quads. Each instance is a param-block index.
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #include "UnityCG.cginc"
            #include "../../Shaders/Canvas2D/Canvas2DCommon.cginc"

            Texture2D<float> _C2D_ClipTex;
            StructuredBuffer<uint> _C2D_FastBase;
            float _C2D_FastOffset;

            float SdRound(float2 p, float2 b, float rad)
            {
                float2 q = abs(p) - b + rad;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - rad;
            }

            float SdEllipse(float2 p, float2 r)
            {
                float2 rq = p / max(r, float2(0.5, 0.5));
                return (length(rq) - 1.0) * min(r.x, r.y);
            }

            float SdArc(float2 p, float a0, float sweep, float radius, float hw, float cap)
            {
                float mid = a0 + sweep * 0.5;
                float co = cos(mid), si = sin(mid);
                float2 q = float2(co * p.x + si * p.y, -si * p.x + co * p.y);
                float halfAng = abs(sweep) * 0.5;
                float ap = atan2(q.y, q.x);
                float radial = abs(length(q) - radius);
                float limit = halfAng;
                if (cap > 1.5) limit += hw / max(radius, 0.5);
                if (abs(ap) <= limit) return radial - hw;
                if (cap < 0.5 || cap > 1.5) return 1.0;
                float endAng = ap > 0.0 ? halfAng : -halfAng;
                float2 capP = float2(cos(endAng), sin(endAng)) * radius;
                return length(q - capP) - hw;
            }

            float SdFast(float2 pc, float mode, float4 ga, float4 gb)
            {
                float sd = -1.0;
                if (mode < 1.5)
                {
                    float ang = gb.x;
                    float co = cos(ang), si = sin(ang);
                    float2 d = pc - ga.xy;
                    float2 q = float2(co * d.x + si * d.y, -si * d.x + co * d.y);
                    sd = SdEllipse(q, ga.zw);
                }
                else if (mode < 2.5)
                {
                    float2 half = ga.zw * 0.5;
                    float2 p = pc - (ga.xy + half);
                    float rad = p.x < 0.0 ? (p.y < 0.0 ? gb.x : gb.w) : (p.y < 0.0 ? gb.y : gb.z);
                    rad = min(max(rad, 0.0), min(half.x, half.y));
                    sd = SdRound(p, half, rad);
                }
                else if (mode < 3.5)
                {
                    float2 a = ga.xy, b = ga.zw;
                    float2 pa = pc - a, ba = b - a;
                    float denom = max(dot(ba, ba), 1e-4);
                    float h = saturate(dot(pa, ba) / denom);
                    sd = length(pa - ba * h) - gb.x;
                }
                else if (mode < 4.5)
                {
                    float ang = gb.x;
                    float co = cos(ang), si = sin(ang);
                    float2 d = pc - ga.xy;
                    float2 q = float2(co * d.x + si * d.y, -si * d.x + co * d.y);
                    sd = abs(SdEllipse(q, ga.zw)) - gb.y;
                }
                else
                {
                    sd = SdArc(pc - ga.xy, gb.x, gb.y, ga.z, gb.z, gb.w);
                }
                if (mode < 0.5) sd = -10.0;
                return sd;
            }

            struct V2F
            {
                float4 pos : SV_POSITION;
                // Two bytes as floats. A11 stores TEXCOORD as fp16, exact only through 2048.
                // A param index (stride 28) past that rounds, so the quad stays put and shades the wrong block.
                float slotHi : TEXCOORD0;
                float slotLo : TEXCOORD1;
            };

            // 6 vertices per quad, one instance. SV_InstanceID on A11 ignores a base instance.
            V2F vert(uint vid : SV_VertexID)
            {
                V2F o;
                uint local = vid / 6;
                uint corner = vid - local * 6;
                uint b = _C2D_FastBase[(uint)(_C2D_FastOffset + 0.5) + local];
                o.slotHi = (float)(b >> 8);
                o.slotLo = (float)(b & 255u);
                float4 rect = _C2D_Params[b + IDX_RECT];
                float2 p = C2D_QuadCorner(corner, rect);
                o.pos = float4(C2D_PixelToNdc(p), 0, 1);
                return o;
            }

            float4 frag(V2F i) : SV_Target
            {
                uint b = ((uint)(i.slotHi + 0.5) << 8) | (uint)(i.slotLo + 0.5);
                float2 pc = C2D_FragToCanvas(i.pos);
                float4 fl = _C2D_Params[b + IDX_FLAGS];
                float cov = saturate(0.5 - SdFast(pc, fl.x, _C2D_Params[b + IDX_GA], _C2D_Params[b + IDX_GB]));
                if (fl.z > 0.5)
                    cov *= _C2D_ClipTex.Load(int3(C2D_CanvasToTexel(int2(floor(pc))), 0));
                float4 sc = _C2D_Params[b + IDX_COLOR];
                float a = cov * sc.a * _C2D_Params[b + IDX_ALPHA].x;
                return float4(sc.rgb * a, a);
            }
            ENDHLSL
        }
    }
}
