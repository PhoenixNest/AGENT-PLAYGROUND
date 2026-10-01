Shader "Hidden/Canvas2D/Fill"
{
    // Resolves an accumulated (or blurred) coverage mask against a paint (solid, linear or radial gradient)
    // and composites with premultiplied source-over, like a canvas 2D fill.
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        HLSLINCLUDE
        #pragma vertex C2D_RectVert
        #pragma fragment frag
        #pragma target 4.5
        #include "UnityCG.cginc"
        #include "Canvas2DCommon.cginc"
        #include "Canvas2DPaint.cginc"

        Texture2D<float> _C2D_Accum;
        Texture2D<float> _C2D_Mask;
        Texture2D<float> _C2D_ClipTex;

        float Coverage(float2 pc)
        {
            float4 fl = C2D_P(IDX_FLAGS);
            int2 tx = C2D_CanvasToTexel(int2(floor(pc)));
            float cov;
            if (fl.x < 0.5) cov = C2D_ApplyRule(_C2D_Accum.Load(int3(tx, 0)), fl.y);
            else cov = _C2D_Mask.Load(int3(tx, 0));
            if (fl.z > 0.5) cov *= _C2D_ClipTex.Load(int3(tx, 0));
            return cov;
        }
        ENDHLSL

        Pass // 0: source-over
        {
            Blend One OneMinusSrcAlpha
            HLSLPROGRAM
            float4 frag(float4 svpos : SV_POSITION) : SV_Target
            {
                float2 pc = C2D_FragToCanvas(svpos);
                float cov = Coverage(pc);
                float4 tg = C2D_P(IDX_TEXT);
                if (tg.x > 0.5)
                {
                    // Skia glyph-mask preblend (SkMaskGamma, sRGB luminance): contrast, then blend in linear light.
                    float4 tl = C2D_P(IDX_IMGC);
                    float srca = cov + (1.0 - cov) * tg.y * cov;
                    float linOut = tl.x * srca + (1.0 - srca) * tl.y;
                    float o = linOut <= 0.0031308 ? linOut * 12.92 : 1.055 * pow(max(linOut, 0.0), 1.0 / 2.4) - 0.055;
                    cov = saturate((o - tg.w) / (tg.z - tg.w));
                }
                float4 paint = C2D_EvalPaint(pc);
                float4 fl = C2D_P(IDX_FLAGS);
                if (fl.w > 0.5)
                {
                    float4 sc = C2D_P(IDX_SHADOW);
                    float a = sc.a * paint.a;
                    paint = float4(sc.rgb * a, a);
                }
                return paint * (cov * C2D_P(IDX_ALPHA).x);
            }
            ENDHLSL
        }

        Pass // 1: clearRect (destination-out with coverage)
        {
            Blend Zero OneMinusSrcAlpha
            HLSLPROGRAM
            float4 frag(float4 svpos : SV_POSITION) : SV_Target
            {
                float cov = Coverage(C2D_FragToCanvas(svpos));
                return float4(cov, cov, cov, cov);
            }
            ENDHLSL
        }
    }
}
