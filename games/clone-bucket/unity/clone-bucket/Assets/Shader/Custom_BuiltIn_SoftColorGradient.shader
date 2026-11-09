Shader "Custom/BuiltIn/SoftColorGradient" {
	Properties {
		_TopColor ("Top Color", Vector) = (0.25,1,1,1)
		_BottomColor ("Bottom Color", Vector) = (0,0.28,1,1)
		_MidColor ("Mid Color", Vector) = (0.15,0.75,1,1)
		_GradientPower ("Gradient Power", Range(0.25, 4)) = 1.15
		_Brightness ("Brightness", Range(0, 3)) = 1.25
		_RimColor ("Rim Color", Vector) = (0.45,1,1,1)
		_RimPower ("Rim Power", Range(0.5, 8)) = 3
		_RimStrength ("Rim Strength", Range(0, 2)) = 0.45
		_SpecColorCustom ("Specular Color", Vector) = (0.85,1,1,1)
		_SpecPower ("Specular Power", Range(8, 128)) = 48
		_SpecStrength ("Specular Strength", Range(0, 2)) = 0.7
		_AmbientStrength ("Ambient Strength", Range(0, 1)) = 0.35
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

			float4 _TopColor;
			float4 _BottomColor;
			float4 _MidColor;
			float _GradientPower;
			float _Brightness;
			float4 _RimColor;
			float _RimPower;
			float _RimStrength;
			float4 _SpecColorCustom;
			float _SpecPower;
			float _SpecStrength;
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

				float g = pow(saturate(n.y * 0.5 + 0.5), _GradientPower);
				float w = saturate((g - 0.45) * 1.81818175);
				w = w * w * (3.0 - 2.0 * w);
				float3 mid = lerp(_BottomColor.rgb, _MidColor.rgb, g);
				float3 base = lerp(mid, _TopColor.rgb, w);

				float ndl = saturate(dot(n, l));
				float3 col = base * (ndl * (1.0 - _AmbientStrength) + _AmbientStrength) * _LightColor0.rgb
					+ base * _AmbientStrength;

				float3 h = normalize(l + v);
				float spec = pow(saturate(dot(n, h)), _SpecPower);
				float rim = pow(1.0 - saturate(dot(n, v)), _RimPower);
				col = _SpecColorCustom.rgb * spec * _SpecStrength + col;
				col = _RimColor.rgb * rim * _RimStrength + col;
				return fixed4(col * _Brightness, 1.0);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
