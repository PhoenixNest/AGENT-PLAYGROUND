Shader "Custom/SliderRamp" {
	Properties {
		[HeaderHelp(BASE, Base Properties)] _Color ("Color", Vector) = (1,1,1,1)
		_HColor ("Highlight Color", Vector) = (0.785,0.785,0.785,1)
		_SColor ("Shadow Color", Vector) = (0.195,0.195,0.195,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		_MatcapTex ("Matcap Texture", 2D) = "black" {}
		_MatcapIntensity ("Matcap Intensity", Range(0, 1)) = 1
		[Separator] [Header(RAMP SETTINGS)] _RampThreshold ("Ramp Threshold", Range(0, 1)) = 0.5
		_RampSmooth ("Ramp Smoothing", Range(0.001, 1)) = 0.1
		[Separator] [HideInInspector] __dummy__ ("unused", Float) = 0
	}
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			Tags { "LightMode"="ForwardBase" }

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
			sampler2D _MatcapTex;
			float4 _Color;
			float4 _HColor;
			float4 _SColor;
			float _MatcapIntensity;
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
				float3 worldPos : TEXCOORD2;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 n = normalize(i.worldNormal);
				float3 v = normalize(_WorldSpaceCameraPos - i.worldPos);
				float ramp = SR_Ramp(n, SR_LightDir(), _RampThreshold, _RampSmooth);
				float3 shade = SR_ShadeColor(ramp, _SColor, _HColor.rgb);

				float3 tc = tex2D(_MainTex, i.uv).rgb * _Color.rgb;
				float3 matcap = tex2D(_MatcapTex, SR_MatcapUV(n, v)).rgb;
				float3 lit = (saturate(tc + matcap) - tc) * _MatcapIntensity + tc;

				float3 ambient = 0;
				#ifdef LIGHTPROBE_SH
				ambient = max(ShadeSH9(float4(n, 1.0)), 0.0);
				#endif
				return float4(lit * _LightColor0.rgb * shade + lit * ambient, 1.0);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
