Shader "WalkingFat/LiquidBottle/SparkleLiquid02" {
	Properties {
		[Space(30)] _BottleThickness ("Bottle Thickness", Range(0, 0.1)) = 0.1
		_BottleThickness1off ("Bottle Thickness1off", Range(0, 1)) = 0
		_BottleThickness1 ("Bottle Thickness1", Range(0, 0.1)) = 0.1
		[Space(30)] _MainTexture ("_MainTexture", 2D) = "black" {}
		_Color ("Color", Vector) = (1,1,1,1)
		_AlphaScale ("Alpha Scale", Range(0, 1)) = 1
		_AlphaScale01 ("Alpha Scale1", Range(0, 5)) = 1
		_AlphaScale03 ("Alpha Scale3", Range(0, 5)) = 1
		_RampTex ("Ramp Tex", 2D) = "white" {}
		_CubeReflect ("Reflect (CUBE)", Cube) = "black" {}
		_ReflectColor ("ReflectColor", Vector) = (1,1,1,1)
		_ReflectAmount ("Reflect Amount", Range(0, 5)) = 1
		_FresnelWeightAlphaC ("Fresnel Weight Alpha Bright", Vector) = (0.4,0.1,1,0)
		_SpecColorCloth ("Specular Color Cloth", Vector) = (0.2,0.2,0.2,1)
		_ShininessCloth ("Shininess Cloth", Float) = 8
		_DarknessLightness ("Cloth Dark X Light Y", Vector) = (0,0,0,0)
		_LightIntensityAmbient ("Light Intensity Ambient", Vector) = (1,1.25,0,0)
		_LightDirectionCloth ("Light Direction Cloth", Vector) = (-0.35,-0.75,0,0)
	}
	SubShader {
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Back

		Pass {
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTexture;
			float4 _MainTexture_ST;
			float4 _Color;
			float _BottleThickness;
			float _AlphaScale;
			float _AlphaScale01;
			float _AlphaScale03;
			float4 _FresnelWeightAlphaC;
			float4 _SpecColorCloth;
			float _ShininessCloth;
			float4 _DarknessLightness;
			float4 _LightIntensityAmbient;
			float4 _LightDirectionCloth;

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float shade : TEXCOORD3;
				float2 uv : TEXCOORD4;
			};

			v2f vert(appdata v) {
				v2f o;
				float4 inflated = float4(v.vertex.xyz + _BottleThickness * v.normal, v.vertex.w);
				o.pos = UnityObjectToClipPos(inflated);
				o.worldPos = mul(unity_ObjectToWorld, inflated).xyz;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);

				float3 vd = normalize(_WorldSpaceCameraPos - o.worldPos);
				float3 h = normalize(vd - _LightDirectionCloth.xyz);
				float k = saturate(dot(h, o.worldNormal));
				float lightTerm = _DarknessLightness.y * k + 1.0;
				float darkTerm = (1.0 - k) * (-_DarknessLightness.x) + 1.0;
				o.shade = lightTerm * darkTerm;

				o.uv = v.texcoord * _MainTexture_ST.xy + _MainTexture_ST.zw;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				float3 n = normalize(i.worldNormal);
				float3 vd = normalize(_WorldSpaceCameraPos - i.worldPos);
				float ndv = dot(vd, n);

				float fres = (1.0 - saturate(abs(ndv) / max(_FresnelWeightAlphaC.x, 1e-4))) * _FresnelWeightAlphaC.y;

				float3 r2 = vd - 2.0 * ndv * n;
				float3 h = normalize(vd - _LightDirectionCloth.xyz);
				float specBase = saturate(dot(-h, r2));
				float a = saturate(_LightIntensityAmbient.x * abs(dot(h, n)) + _LightIntensityAmbient.y) * _Color.x;
				float sp = pow(max(specBase, 1e-5), _ShininessCloth) * (_SpecColorCloth.w * _SpecColorCloth.x);
				float s = a * i.shade + saturate(sp * 2.5);

				float t = saturate(fres * (_FresnelWeightAlphaC.z - s) + s);
				float alphaTerm = pow(max(t, 1e-5), _AlphaScale01) * _AlphaScale + t * _AlphaScale03;
				float texAlpha = tex2D(_MainTexture, i.uv).a;

				return fixed4(_Color.rgb, saturate(texAlpha + alphaTerm));
			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}
