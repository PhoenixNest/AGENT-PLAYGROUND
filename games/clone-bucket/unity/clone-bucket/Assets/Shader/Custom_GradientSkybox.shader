Shader "Custom/GradientSkybox" {
	Properties {
		_Color1 ("Color 1", Vector) = (1,0.5,0.5,1)
		_Color2 ("Color 2", Vector) = (0.5,0.5,1,1)
		_Scale ("Scale", Float) = 1
		_OffsetY ("OffsetY", Float) = 0
	}
	SubShader {
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		LOD 200
		Cull Off
		ZWrite Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			float4 _Color1;
			float4 _Color2;
			float _Scale;
			float _OffsetY;

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float4 clip : TEXCOORD0;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.clip = o.pos;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				float t = saturate(i.clip.y / i.clip.w + _OffsetY) * _Scale;
				return lerp(_Color1, _Color2, t);
			}
			ENDCG
		}
	}
	Fallback "Unlit/Color"
}
