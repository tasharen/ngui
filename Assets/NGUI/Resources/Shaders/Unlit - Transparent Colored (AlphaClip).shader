Shader "Unlit/Transparent Colored (AlphaClip)"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_Color ("Tint Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		LOD 200
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Color (0,0,0,0) }
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float4 _ClipRange = float4(0.0, 0.0, 1000.0, 1000.0);
			float4x4 _ClipMatrix;

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
				float4 worldPos : TEXCOORD1;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.worldPos = mul(_ClipMatrix, v.vertex);
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag (v2f IN) : COLOR
			{
				float2 factor = abs(IN.worldPos.xy - _ClipRange.xy) / _ClipRange.zw;

				// Sample the texture
				fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;
				float val = 1.0 - max(factor.x, factor.y);

				// Option 1: 'if' statement
				if (val < 0.0) col.a = 0.0;

				// Option 2: no 'if' statement -- may be faster on some devices
				//col.a *= ceil(clamp(val, 0.0, 1.0));

				return col * _Color;
			}
			ENDCG
		}
	}
	
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		LOD 100
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Color (0,0,0,0) }
		ColorMask RGB
		AlphaTest Greater .01
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			ColorMaterial AmbientAndDiffuse
			
			SetTexture [_MainTex]
			{
				Combine Texture * Primary
			}
			
			SetTexture [_MainTex]
			{
				ConstantColor [_Color]
				Combine Previous * Constant
			}
		}
	}
}