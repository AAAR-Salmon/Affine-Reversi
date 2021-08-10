Shader "Unlit/GridShader" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Line Color", Color) = (1,1,1,1)
		_FracX ("Frac X", Int) = 8
		_FracY ("Frac Y", Int) = 8
		_LineWeight ("Line Weight", Float) = 0.1
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			int _FracX;
			int _FracY;
			float _LineWeight;

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target {
				if (frac(i.uv.x * (_FracX + _LineWeight)) > _LineWeight && frac(i.uv.y * (_FracY + _LineWeight)) > _LineWeight) {
					clip(-1);
				}
				return _Color;
			}
			ENDCG
		}
	}
}
