Shader "Hidden/Canvas2D/LayerBlit"
{
    // 1:1 source-over of a premultiplied layer texture. The quad is in canvas pixels
    // (top-left origin, +y down) so it matches every other Canvas2D draw. ReadPixels
    // stores that top row at the last texture row, same as Image.shader, but the texels
    // are already premultiplied and must not be multiplied by alpha again.
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

            Texture2D<float4> _C2D_Layer;

            float4 frag(float4 svpos : SV_POSITION) : SV_Target
            {
                float2 pc = C2D_FragToCanvas(svpos);
                uint w, h;
                _C2D_Layer.GetDimensions(w, h);
                int2 t = int2(floor(pc));
                t.y = (int)h - 1 - t.y;
                if ((uint)t.x >= w || (uint)t.y >= h) return 0;
                return _C2D_Layer.Load(int3(t, 0)) * C2D_P(IDX_ALPHA).x;
            }
            ENDHLSL
        }
    }
}
