Shader "Gamelogic/Fx/Pixelate"
{
	Properties
	{
		_MainTex ("Render Texture", 2D) = "white" {}
		_PixelSize ("Pixel Size", Vector) = (2, 2, 0, 0)
	}

	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
			"Queue"="Overlay"
		}
		Pass
		{
			Name "Pixelate"
			Cull Off
			ZClip Off
			ZTest Always
			ZWrite Off

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature GAMELOGIC_HAS_URP

			#ifdef GAMELOGIC_HAS_URP
			#define UNITY_PIPELINE_URP 1
			#endif

			#include "Packages/za.co.gamelogic.fx/Runtime/Core/Shaders/Gamelogic.hlsl"

			float4 _MainTex_TexelSize;
			float2 _PixelSize;

			float4 frag(INPUT i) : SV_Target
			{
				float2 uv = pixelate_uv(i.uv, _PixelSize, _MainTex_TexelSize);
				float4 color = SAMPLE(_MainTex, uv);

				return color;
			}
			ENDHLSL
		}
	}

	Fallback Off
}
