Shader "Custom/SliderRamp_maze" {
	Properties {
		[HeaderHelp(BASE, Base Properties)] _Color ("Color", Vector) = (1,1,1,1)
		_HColor ("Highlight Color", Vector) = (0.785,0.785,0.785,1)
		_SColor ("Shadow Color", Vector) = (0.195,0.195,0.195,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		_MatcapTex ("Matcap Texture", 2D) = "black" {}
		_MatcapIntensity ("Matcap Intensity", Range(0, 1)) = 1
		_FresnelColor ("Fresnel Color", Vector) = (1,1,1,1)
		_FresnelPower ("Fresnel Power", Range(0.1, 10)) = 2
		_FresnelIntensity ("Fresnel Intensity", Range(0, 2)) = 1
		_EnvCubemap ("Environment Cubemap", Cube) = "" {}
		_EnvIntensity ("Environment Intensity", Range(0, 2)) = 1
		_EnvRotation ("Environment Rotation", Range(0, 360)) = 0
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
			float4 _FresnelColor;
			float _FresnelPower;
			float _FresnelIntensity;
			samplerCUBE _EnvCubemap;
			float _EnvIntensity;
			float _EnvRotation;
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

			float3 SRM_RotateY(float3 d, float degrees)
			{
				float a = radians(degrees);
				float s = sin(a);
				float c = cos(a);
				return float3(c * d.x + s * d.z, d.y, -s * d.x + c * d.z);
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
				lit *= _LightColor0.rgb;

				float fresnel = pow(max(1.0 - saturate(dot(n, v)), 0.0), _FresnelPower) * _FresnelIntensity;
				float3 r = SRM_RotateY(reflect(-v, n), _EnvRotation);
				float3 env = texCUBE(_EnvCubemap, r).rgb * _EnvIntensity + _FresnelColor.rgb * fresnel;

				return float4(lit * shade + env, 1.0);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
