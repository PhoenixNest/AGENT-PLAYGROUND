Shader "Hidden/Canvas2D/PremulOver"
{
    Properties
    {
        _MainTex ("Under", 2D) = "white" {}
        _Over ("Over", 2D) = "white" {}
    }
    // Opaque composite: premultiplied _Over over _MainTex. One sample of each, no blend state.
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Blend Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Over;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 under = tex2D(_MainTex, i.uv);
                float4 over = tex2D(_Over, i.uv);
                float ia = 1.0 - over.a;
                return float4(over.rgb + under.rgb * ia, 1.0);
            }
            ENDHLSL
        }
    }
}
