Shader "CrazyLabs/MazeShader" {
	Properties {
		_ColorFront ("Front Maze Color", Vector) = (1,0,0.6,1)
		_ColorBack ("Back Maze Color", Vector) = (0.5,0,0.3,1)
		_FrontDirection ("Front Direction", Vector) = (0,0,-1,1)
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			float4 _ColorFront;
			float4 _ColorBack;
			float4 _FrontDirection;

			struct appdata {
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR0;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				return i.color;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
