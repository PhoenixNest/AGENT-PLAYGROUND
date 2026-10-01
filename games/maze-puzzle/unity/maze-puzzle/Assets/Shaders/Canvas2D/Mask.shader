Shader "Hidden/Canvas2D/Mask"
{
    // Mask utilities: accumulated area -> coverage, separable gaussian blur (shadows), clip intersection.
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend Off

        HLSLINCLUDE
        #pragma vertex C2D_RectVert
        #pragma fragment frag
        #pragma target 4.5
        #include "UnityCG.cginc"
        #include "Canvas2DCommon.cginc"

        Texture2D<float> _C2D_Accum;
        Texture2D<float> _C2D_Mask;
        Texture2D<float> _C2D_ClipTex;

        float MaskAt(int2 ip, float4 mr)
        {
            if (ip.x < mr.x || ip.y < mr.y || ip.x >= mr.z || ip.y >= mr.w) return 0;
            return _C2D_Mask.Load(int3(C2D_CanvasToTexel(ip), 0));
        }

        float Blur(float4 svpos, int2 axis)
        {
            float4 bp = C2D_P(IDX_BLUR);
            float4 mr = C2D_P(IDX_MASKRECT);
            int2 ip = int2(floor(C2D_FragToCanvas(svpos)));
            int r = (int)bp.y;
            float inv2s2 = 1.0 / (2.0 * bp.x * bp.x);
            float sum = 0.0, wsum = 0.0;
            [loop] for (int k = -r; k <= r; k++)
            {
                float w = exp(-(float)(k * k) * inv2s2);
                sum += w * MaskAt(ip + axis * k, mr);
                wsum += w;
            }
            return sum / wsum;
        }
        ENDHLSL

        Pass // 0: accumulated area -> coverage mask (fill rule applied)
        {
            HLSLPROGRAM
            float frag(float4 svpos : SV_POSITION) : SV_Target
            {
                int2 ip = int2(floor(C2D_FragToCanvas(svpos)));
                return C2D_ApplyRule(_C2D_Accum.Load(int3(C2D_CanvasToTexel(ip), 0)), C2D_P(IDX_FLAGS).y);
            }
            ENDHLSL
        }

        Pass // 1: blur horizontal
        {
            HLSLPROGRAM
            float frag(float4 svpos : SV_POSITION) : SV_Target { return Blur(svpos, int2(1, 0)); }
            ENDHLSL
        }

        Pass // 2: blur vertical
        {
            HLSLPROGRAM
            float frag(float4 svpos : SV_POSITION) : SV_Target { return Blur(svpos, int2(0, 1)); }
            ENDHLSL
        }

        Pass // 3: clip = previous clip * coverage(accum)
        {
            HLSLPROGRAM
            float frag(float4 svpos : SV_POSITION) : SV_Target
            {
                float4 fl = C2D_P(IDX_FLAGS);
                int2 ip = int2(floor(C2D_FragToCanvas(svpos)));
                int2 tx = C2D_CanvasToTexel(ip);
                float c = C2D_ApplyRule(_C2D_Accum.Load(int3(tx, 0)), fl.y);
                if (fl.z > 0.5) c *= _C2D_ClipTex.Load(int3(tx, 0));
                return c;
            }
            ENDHLSL
        }

        Pass // 4: fill with constant (clip init = 1)
        {
            HLSLPROGRAM
            float frag(float4 svpos : SV_POSITION) : SV_Target { return C2D_P(IDX_BLUR).z; }
            ENDHLSL
        }
    }
}
