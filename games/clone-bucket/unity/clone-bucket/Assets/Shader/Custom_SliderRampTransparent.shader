Shader "Custom/SliderRampTransparent" {
	Properties {
		[HeaderHelp(BASE, Base Properties)] _Color ("Color", Vector) = (1,1,1,1)
		_HColor ("Highlight Color", Vector) = (0.785,0.785,0.785,1)
		_SColor ("Shadow Color", Vector) = (0.195,0.195,0.195,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		[Separator] [Header(RAMP SETTINGS)] _RampThreshold ("Ramp Threshold", Range(0, 1)) = 0.5
		_RampSmooth ("Ramp Smoothing", Range(0.001, 1)) = 0.1
		[Separator] [HeaderHelp(TRANSPARENCY)] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Blending Source", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Blending Dest", Float) = 10
		[Separator] [HideInInspector] __dummy__ ("unused", Float) = 0
	}
	SubShader{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
		LOD 200

		Pass
		{
			Tags { "LightMode"="ForwardBase" }
			Blend [_SrcBlend] [_DstBlend]
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_fwdbase
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "SliderRampCommon.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			float4 _HColor;
			float4 _SColor;
			float _RampThreshold;
			float _RampSmooth;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldNormal : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 n = normalize(i.worldNormal);
				float ramp = SR_Ramp(n, SR_LightDir(), _RampThreshold, _RampSmooth);
				float3 shade = SR_ShadeColor(ramp, _SColor, _HColor.rgb);

				float4 tex = tex2D(_MainTex, i.uv);
				float3 col = tex.rgb * _Color.rgb * _LightColor0.rgb;
				return float4(shade * col, tex.a * _Color.a);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
