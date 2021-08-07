Shader "Unlit/GridShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Line Color", Color) = (1,1,1,1)
		_TileX ("Tile X", Int) = 8
		_TileY ("Tile Y", Int) = 8
		_LineWeight ("Line Weight", Float) = 0.1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			int _TileX;
			int _TileY;
			float _LineWeight;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 transparent = fixed4(0,0,0,0);
				if (frac(i.uv.x * (_TileX + _LineWeight)) < _LineWeight) {
					return _Color;
				}
				if (frac(i.uv.y * (_TileY + _LineWeight)) < _LineWeight) {
					return _Color;
				}
				return transparent;
			}
			ENDCG
		}
	}
}
