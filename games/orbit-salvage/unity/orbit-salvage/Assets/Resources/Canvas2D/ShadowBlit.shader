Shader "Hidden/Canvas2D/ShadowBlit"
{
    // Premultiplied source-over of a shadow bitmap into IDX_RECT. Used only when shadowlow is on.
    // The bitmap is a blur, so the sampler follows the texture filter (bilinear when upsampling).
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

            Texture2D _C2D_Layer;
            SamplerState sampler_C2D_Layer;

            float4 frag(float4 svpos : SV_POSITION) : SV_Target
            {
                float2 pc = C2D_FragToCanvas(svpos);
                float4 r = C2D_P(IDX_RECT);
                float2 size = max(r.zw - r.xy, float2(1.0, 1.0));
                float2 uv;
                uv.x = (pc.x - r.x) / size.x;
                uv.y = 1.0 - (pc.y - r.y) / size.y;
                return _C2D_Layer.SampleLevel(sampler_C2D_Layer, saturate(uv), 0);
            }
            ENDHLSL
        }
    }
}
