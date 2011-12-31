Shader "Refractive"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_NormalMap ("Normal Map (RGB), Translucency (A)", 2D) = "" {}
		_Color ("Color Tint", Color) = (1,1,1,1)
		_Focus ("Focus", Range(-100.0, 100.0)) = -100.0
	}

	Category
	{
		Tags
		{
			"Queue" = "Transparent+1"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		SubShader
		{
			LOD 300

			GrabPass
			{
				Name "BASE"
				Tags { "LightMode" = "Always" }
			}
		   
			Cull Off
			ZWrite Off
			ZTest LEqual
			Blend SrcAlpha OneMinusSrcAlpha
			AlphaTest Greater 0

CGPROGRAM
#pragma exclude_renderers gles
#pragma vertex vert
#pragma surface surf BlinnPhong alpha
#include "UnityCG.cginc"

sampler2D _GrabTexture;
sampler2D _MainTex;
sampler2D _NormalMap;

float4 _Color;
float4 _GrabTexture_TexelSize;
float _Focus;

struct Input
{
	float4 position : POSITION;
	float2 uv_MainTex : TEXCOORD0;
	float4 color : COLOR;
	float4 proj : TEXCOORD1;
};

void vert (inout appdata_full v, out Input o)
{
	o.position = mul(UNITY_MATRIX_MVP, v.vertex);
	
	#if UNITY_UV_STARTS_AT_TOP
		float scale = -1.0;
	#else
		float scale = 1.0;
	#endif
	o.proj.xy = (float2(o.position.x, o.position.y * scale) + o.position.w) * 0.5;
	o.proj.zw = o.position.zw;
}

void surf (Input IN, inout SurfaceOutput o)
{
	half4 tex = tex2D(_MainTex, IN.uv_MainTex);
	half4 nor = tex2D(_NormalMap, IN.uv_MainTex);
	
	half3 test = nor.rgb;
	nor.xyz = normalize(nor.xyz * 2.0 - 1.0);

	float2 offset = nor.xy * _GrabTexture_TexelSize.xy * _Focus;
	IN.proj.xy = offset * IN.proj.z + IN.proj.xy;
	half4 ref = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(IN.proj));
	
	half4 col;
	col.rgb = lerp(_Color.rgb * ref.rgb, IN.color.rgb * tex.rgb, nor.w);
	col.a = IN.color.a * _Color.a * tex.a;

	o.Albedo = col.rgb;
	o.Normal = nor.xyz;
	o.Specular = 1.0;
	o.Gloss = 0.02;
	o.Alpha = col.a;
}

ENDCG
		}
		
		SubShader
		{
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
	Fallback Off
}
