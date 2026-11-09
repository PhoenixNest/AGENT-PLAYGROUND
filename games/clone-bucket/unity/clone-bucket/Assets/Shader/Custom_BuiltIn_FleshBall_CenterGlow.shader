Shader "Custom/BuiltIn/FleshBall_CenterGlow" {
	Properties {
		[Header(Base Colors)] _CenterColor ("Center Color", Vector) = (1,0.63,0.82,1)
		_EdgeColor ("Edge Color", Vector) = (0.4,0.2,0.75,1)
		[Header(Center)] _CenterPower ("Center Power", Range(0.5, 8)) = 2.2
		_EdgeDarkness ("Edge Darkness", Range(0, 2)) = 1
		[Header(Top Tint)] _TopTintColor ("Top Tint Color", Vector) = (0.7,0.8,1,1)
		_TopTintStrength ("Top Tint Strength", Range(0, 1)) = 0.22
		_TopTintSharpness ("Top Tint Sharpness", Range(0.5, 6)) = 1.6
		[Header(Specular)] _SpecColor ("Specular Color", Vector) = (1,1,1,1)
		_SpecPower ("Specular Power", Range(8, 128)) = 48
		_SpecStrength ("Specular Strength", Range(0, 2)) = 0.18
		[Header(Lighting)] _DiffuseStrength ("Diffuse Strength", Range(0, 2)) = 0.35
		_AmbientStrength ("Ambient Strength", Range(0, 2)) = 0.95
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass {
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			float4 _CenterColor;
			float4 _EdgeColor;
			float _CenterPower;
			float _EdgeDarkness;
			float4 _TopTintColor;
			float _TopTintStrength;
			float _TopTintSharpness;
			float _SpecPower;
			float _SpecStrength;
			float _DiffuseStrength;
			float _AmbientStrength;

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				float3 n = normalize(i.worldNormal);
				float3 v = normalize(_WorldSpaceCameraPos - i.worldPos);
				float3 l = normalize(_WorldSpaceLightPos0.xyz);

				float k = 1.0 - _EdgeDarkness * 0.35;
				float3 edge = k * _EdgeColor.rgb;
				float centre = pow(saturate(dot(n, v)), _CenterPower);
				float3 col = centre * (_CenterColor.rgb - k * _EdgeColor.rgb) + edge;

				float top = pow(saturate(n.y * 0.5 + 0.5), _TopTintSharpness);
				col += top * _TopTintColor.rgb * _TopTintStrength;

				float diffuse = saturate(dot(n, l)) * _DiffuseStrength + _AmbientStrength;

				float3 h = normalize(l + v);
				float spec = pow(saturate(dot(n, h)), _SpecPower) * _SpecStrength;
				return fixed4(col * diffuse + _SpecColor.rgb * spec, 1.0);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
