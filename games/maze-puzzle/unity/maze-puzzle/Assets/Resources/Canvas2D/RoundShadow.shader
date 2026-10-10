Shader "Hidden/Canvas2D/RoundShadow"
{
    // Analytic gaussian of an axis-aligned rounded rect. One quad, no mask targets.
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

            float ErfcApprox(float x)
            {
                float ax = abs(x);
                float t = 1.0 / (1.0 + 0.3275911 * ax);
                float p = (((((1.061405429 * t - 1.453152027) * t) + 1.421413741) * t - 0.284496736) * t + 0.254829592) * t;
                float e = 1.0 - p * exp(-ax * ax);
                float erf = x < 0.0 ? -e : e;
                return 1.0 - erf;
            }

            float SdRound(float2 p, float2 b, float rad)
            {
                float2 q = abs(p) - b + rad;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - rad;
            }

            float4 frag(float4 svpos : SV_POSITION) : SV_Target
            {
                float2 pc = C2D_FragToCanvas(svpos);
                float4 box = C2D_P(IDX_GA);
                float4 rad4 = C2D_P(IDX_GB);
                float2 half = box.zw * 0.5;
                float2 c = box.xy + half;
                float2 p = pc - c;
                float rad = p.x < 0.0 ? (p.y < 0.0 ? rad4.x : rad4.w) : (p.y < 0.0 ? rad4.y : rad4.z);
                rad = min(rad, min(half.x, half.y));
                float sd = SdRound(p, half, max(rad, 0.0));
                float sigma = max(C2D_P(IDX_BLUR).x, 0.35);
                float cov = saturate(0.5 * ErfcApprox(sd / (sigma * 1.41421356)));
                float4 fl = C2D_P(IDX_FLAGS);
                if (fl.z > 0.5)
                    cov *= _C2D_ClipTex.Load(int3(C2D_CanvasToTexel(int2(floor(pc))), 0));
                float4 sc = C2D_P(IDX_SHADOW);
                float a = cov * sc.a * C2D_P(IDX_ALPHA).x;
                return float4(sc.rgb * a, a);
            }
            ENDHLSL
        }
    }
}
