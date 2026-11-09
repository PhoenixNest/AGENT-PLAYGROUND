Shader "UI/GrayButton" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		[Header(Gray Effect)] _GrayScale ("Gray Scale", Range(0, 1)) = 0
		_GrayColor ("Gray Color", Vector) = (0.5,0.5,0.5,1)
		[Header(Advanced Settings)] _Brightness ("Brightness", Range(0, 2)) = 1
		_Contrast ("Contrast", Range(0, 2)) = 1
		_Saturation ("Saturation", Range(0, 2)) = 1
		[Header(UI Settings)] _StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}
	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" "CanUseSpriteAtlas" = "True" }

		Stencil {
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			fixed4 _TextureSampleAdd;
			fixed4 _Color;
			float4 _MainTex_ST;
			float4 _ClipRect;
			fixed _GrayScale;
			fixed4 _GrayColor;
			float _Brightness;
			float _Contrast;
			float _Saturation;

			v2f vert(appdata_t v) {
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = mul(unity_ObjectToWorld, v.vertex);
				OUT.vertex = UnityObjectToClipPos(v.vertex);
				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				OUT.color = v.color * _Color;
				return OUT;
			}

			float3 SaturateHSV(float3 rgb, float satScale) {
				float maxc = max(rgb.r, max(rgb.g, rgb.b));
				float minc = min(rgb.r, min(rgb.g, rgb.b));
				float delta = maxc - minc;
				float s = delta / (maxc + 1e-10);
				float h;
				if (delta < 1e-6) {
					h = 0.0;
				} else if (maxc == rgb.r) {
					h = frac((rgb.g - rgb.b) / delta / 6.0);
				} else if (maxc == rgb.g) {
					h = (rgb.b - rgb.r) / delta / 6.0 + 1.0 / 3.0;
				} else {
					h = (rgb.r - rgb.g) / delta / 6.0 + 2.0 / 3.0;
				}
				float3 hueRGB = saturate(abs(frac(h + float3(1.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0);
				return maxc * lerp(float3(1.0, 1.0, 1.0), hueRGB, saturate(s * satScale));
			}

			fixed4 frag(v2f IN) : SV_Target {
				fixed4 c = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

				float2 p = IN.worldPosition.xy;
				float inX = step(_ClipRect.x, p.x) * step(p.x, _ClipRect.z);
				float inY = step(_ClipRect.y, p.y) * step(p.y, _ClipRect.w);
				float mask = inX * inY;
				c.a *= mask;
				#ifdef UNITY_UI_ALPHACLIP
				clip(c.a - 0.001);
				#endif

				float3 rgb = (c.rgb * _Brightness - 0.5) * _Contrast + 0.5;
				if (_Saturation != 1.0) {
					rgb = SaturateHSV(rgb, _Saturation);
				}

				float lum = dot(rgb, float3(0.2126, 0.7152, 0.0722));
				float3 grey = lerp(rgb, lum * _GrayColor.rgb, _GrayScale);
				rgb = _GrayScale > 0.0 ? grey : rgb;
				return fixed4(saturate(rgb), c.a);
			}
			ENDCG
		}
	}
}
