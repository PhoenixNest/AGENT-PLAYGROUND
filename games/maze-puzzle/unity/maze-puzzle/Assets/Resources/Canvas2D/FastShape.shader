Shader "Hidden/Canvas2D/FastShape"
{
    // One quad per primitive. flags.x: 1 ellipse, 2 rounded rect, 3 capsule, 4 ellipse ring.
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

            float4 frag(float4 svpos : SV_POSITION) : SV_Target
            {
                float2 pc = C2D_FragToCanvas(svpos);
                float4 fl = C2D_P(IDX_FLAGS);
                float mode = fl.x;
                float4 ga = C2D_P(IDX_GA);
                float4 gb = C2D_P(IDX_GB);
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
                else
                {
                    float ang = gb.x;
                    float co = cos(ang), si = sin(ang);
                    float2 d = pc - ga.xy;
                    float2 q = float2(co * d.x + si * d.y, -si * d.x + co * d.y);
                    sd = abs(SdEllipse(q, ga.zw)) - gb.y;
                }
                if (mode < 0.5) sd = -10.0;
                float cov = saturate(0.5 - sd);
                if (fl.z > 0.5)
                    cov *= _C2D_ClipTex.Load(int3(C2D_CanvasToTexel(int2(floor(pc))), 0));
                float4 sc = C2D_P(IDX_COLOR);
                float a = cov * sc.a * C2D_P(IDX_ALPHA).x;
                return float4(sc.rgb * a, a);
            }
            ENDHLSL
        }
    }
}
