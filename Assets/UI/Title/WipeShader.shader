Shader "UI/WipeShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_WipeTex ("Wipe Texture", 2D) = "defaulttexture" {}
		_AnimTime ("Time", Range (0, 1)) = 1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _WipeTex;
			float _AnimTime;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 mask = tex2D(_WipeTex, i.uv);

				float threshold = (1 - _AnimTime) * 2;
				float clampedDistance = clamp(threshold - mask.r, 0, 1);
				float x = 3 * (clampedDistance - 1);
				col.a = min(col.a, x * x);
				return col;
			}
			ENDCG
		}
	}
}
