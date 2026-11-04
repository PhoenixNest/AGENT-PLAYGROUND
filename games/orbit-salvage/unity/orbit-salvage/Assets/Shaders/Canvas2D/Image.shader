Shader "Hidden/Canvas2D/Image"
{
    // drawImage: coverage of the transformed destination rectangle times a premultiplied bilinear sample
    // of the source rectangle (no mip-maps), composited with source-over.
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
            #include "Canvas2DCommon.cginc"

            Texture2D<float> _C2D_Accum;
            Texture2D<float> _C2D_ClipTex;
            Texture2D<float4> _C2D_Image;

            float4 Texel(int2 t, float4 srcRect)
            {
                uint tw, th;
                _C2D_Image.GetDimensions(tw, th);
                int2 lo = max(int2(floor(srcRect.xy)), 0);
                int2 hi = min(int2(ceil(srcRect.xy + srcRect.zw)) - 1, int2(tw, th) - 1);
                t = clamp(t, lo, max(hi, lo));
                float4 c = _C2D_Image.Load(int3(t.x, (int)th - 1 - t.y, 0));
                return float4(c.rgb * c.a, c.a);
            }

            float4 frag(float4 svpos : SV_POSITION) : SV_Target
            {
                float2 pc = C2D_FragToCanvas(svpos);
                float4 fl = C2D_P(IDX_FLAGS);
                int2 tx = C2D_CanvasToTexel(int2(floor(pc)));
                float cov = C2D_ApplyRule(_C2D_Accum.Load(int3(tx, 0)), 0);
                if (fl.z > 0.5) cov *= _C2D_ClipTex.Load(int3(tx, 0));

                float4 ia = C2D_P(IDX_IMGA);
                float4 ib = C2D_P(IDX_IMGB);
                float4 src = C2D_P(IDX_IMGC);
                float4 dst = C2D_P(IDX_IMGD);
                float2 local = float2(dot(ia.xyz, float3(pc, 1)), dot(ib.xyz, float3(pc, 1)));
                float2 st = src.xy + (local - dst.xy) / dst.zw * src.zw;
                float4 sr = src;
                float4 c;
                if (ia.w < 0.5)
                {
                    c = Texel(int2(floor(st)), sr);
                }
                else
                {
                    float2 p = st - 0.5;
                    int2 p0 = int2(floor(p));
                    float2 f = p - p0;
                    float4 c00 = Texel(p0, sr);
                    float4 c10 = Texel(p0 + int2(1, 0), sr);
                    float4 c01 = Texel(p0 + int2(0, 1), sr);
                    float4 c11 = Texel(p0 + int2(1, 1), sr);
                    c = lerp(lerp(c00, c10, f.x), lerp(c01, c11, f.x), f.y);
                }
                return c * (cov * C2D_P(IDX_ALPHA).x);
            }
            ENDHLSL
        }
    }
}
