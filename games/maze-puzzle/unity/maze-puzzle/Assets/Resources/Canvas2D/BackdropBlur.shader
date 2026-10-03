Shader "Hidden/Canvas2D/BackdropBlur"
{
    // One separable box pass. Point samples, equal weights, edge clamp. Radius is in texels.
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
            float4 _MainTex_TexelSize;
            float _Radius;
            float4 _Axis;

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
                int r = (int)_Radius;
                if (r <= 0) return tex2Dlod(_MainTex, float4(i.uv, 0, 0));
                float2 step = _Axis.xy * _MainTex_TexelSize.xy;
                float4 acc = 0;
                for (int k = -r; k <= r; k++)
                    acc += tex2Dlod(_MainTex, float4(i.uv + step * k, 0, 0));
                return acc / (float)(r * 2 + 1);
            }
            ENDHLSL
        }
    }
}
