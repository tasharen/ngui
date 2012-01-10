Shader "Unlit/Transparent Colored (Clipped)"
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
			"Queue" = "Transparent+1"
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
			#pragma multi_compile CLIP_METHOD_ALPHA CLIP_METHOD_HARD CLIP_METHOD_SOFT

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float4 _ClipRange = float4(0.0, 0.0, 1000.0, 1000.0);

			#if defined (CLIP_METHOD_SOFT)
			float2 _ClipSharpness = float2(20.0, 20.0);
			#endif
			
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
				o.worldPos = v.vertex;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			fixed4 frag (v2f IN) : COLOR
			{
				float2 factor = abs(IN.worldPos.xy - _ClipRange.xy) / _ClipRange.zw;

			#if defined (CLIP_METHOD_HARD)
				// Method 1: clip() function
				clip(1.0 - max(factor.x, factor.y));
			#endif

				// Sample the texture
				fixed4 col = tex2D(_MainTex, IN.texcoord) * IN.color;

			#if defined (CLIP_METHOD_ALPHA)
				float val = 1.0 - max(factor.x, factor.y);

				// Method 2: 'if' statement
				if (val < 0.0) col.a = 0.0;
				
				// Method 3: no 'if' statement -- may be faster on some devices
				//col.a *= ceil(clamp(val, 0.0, 1.0));
			#endif

			#if defined (CLIP_METHOD_SOFT)
				// Method 4: smooth edges
				factor = float2(1.0) - factor;
				factor *= _ClipSharpness;
				col.a *= clamp( min(factor.x, factor.y), 0.0, 1.0);
			#endif
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