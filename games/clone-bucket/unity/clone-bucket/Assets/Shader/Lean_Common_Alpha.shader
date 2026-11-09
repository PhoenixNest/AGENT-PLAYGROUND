Shader "Lean/Common/Alpha" {
	Properties {
		_MainTex ("Main Tex", 2D) = "white" {}
		_Color ("Color", Vector) = (1,1,1,1)
	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
		LOD 100

		Cull Off
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;

			struct appdata {
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target {
				fixed4 tex = tex2D(_MainTex, i.uv);
				return fixed4(tex.rgb * _Color.rgb, tex.a * _Color.a);
			}
			ENDCG
		}
	}
	Fallback "Unlit/Transparent"
}
