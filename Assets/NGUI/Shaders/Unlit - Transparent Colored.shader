Shader "Unlit/Transparent Colored"
{
	Properties
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
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
		
		LOD 100
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			ColorMaterial AmbientAndDiffuse
			Lighting Off
			
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