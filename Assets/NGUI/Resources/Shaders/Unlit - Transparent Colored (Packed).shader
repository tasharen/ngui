Shader "Unlit/Transparent Colored (Packed)"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Offset -1, -1
			Fog { Mode Off }
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct appdata_t
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 frag (v2f IN) : COLOR
			{
				half4 mask = tex2D(_MainTex, IN.texcoord);

				half luminance = mask.r;
				luminance = lerp(luminance, mask.g, saturate(IN.color.a * 4.0 - 1.0));
				luminance = lerp(luminance, mask.b, saturate(IN.color.a * 4.0 - 2.0));
				luminance = lerp(luminance, mask.a, saturate(IN.color.a * 4.0 - 3.0));

				return half4(IN.color.rgb, luminance);
			}
			ENDCG
		}
	}
	Fallback Off
}