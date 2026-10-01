Shader "Hidden/Canvas2D/Text"
{
    // Glyph quads sampled from a TextMeshPro SDF atlas. The signed distance is converted to pixel coverage
    // (fill) or to a band around the outline (stroke) and composited with source-over.
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #include "UnityCG.cginc"
            #include "Canvas2DCommon.cginc"
            #include "Canvas2DPaint.cginc"

            StructuredBuffer<float4> _C2D_Glyphs;
            sampler2D _C2D_Atlas;
            Texture2D<float> _C2D_ClipTex;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(uint vid : SV_VertexID)
            {
                uint g = vid / 6;
                uint c = vid % 6;
                uint base = ((uint)(C2D_P(IDX_EDGE).x + 0.5) + g) * 3;
                float4 g0 = _C2D_Glyphs[base];
                float4 g1 = _C2D_Glyphs[base + 1];
                float4 g2 = _C2D_Glyphs[base + 2];
                float4 ma = C2D_P(IDX_IMGA);
                float4 mb = C2D_P(IDX_IMGB);
                uint i = c == 0 ? 0 : c == 1 ? 1 : c == 2 ? 2 : c == 3 ? 2 : c == 4 ? 1 : 3;
                float fx = (i & 1) ? 1.0 : 0.0;
                float fy = (i & 2) ? 1.0 : 0.0;
                float lx = lerp(g0.z, g1.x, fx);
                float ly = lerp(g0.w, g1.y, fy);
                float2 p = g0.xy + float2(ma.x * lx + ma.z * ly, ma.y * lx + ma.w * ly);
                v2f o;
                o.pos = float4(C2D_PixelToNdc(p), 0, 1);
                o.uv = float2(lerp(g1.z, g2.x, fx), lerp(g1.w, g2.y, 1.0 - fy));
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 tp = C2D_P(IDX_TEXT);
                float d = tex2D(_C2D_Atlas, i.uv).a;
                float dist = (d - 0.5) * tp.w + C2D_P(IDX_IMGC).x;
                float cov;
                if (tp.x < 0.5) cov = saturate(dist + 0.5);
                else cov = saturate(tp.y + 0.5 - abs(dist));
                cov = pow(cov, tp.z);

                float2 pc = C2D_FragToCanvas(i.pos);
                if (C2D_P(IDX_FLAGS).z > 0.5)
                    cov *= _C2D_ClipTex.Load(int3(C2D_CanvasToTexel(int2(floor(pc))), 0));
                float4 paint = C2D_EvalPaint(pc);
                return paint * (cov * C2D_P(IDX_ALPHA).x);
            }
            ENDHLSL
        }
    }
}
