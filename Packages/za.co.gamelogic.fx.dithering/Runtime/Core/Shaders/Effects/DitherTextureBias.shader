Shader "Gamelogic/Fx/Dithering/DitherTextureBias"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

		_DitherPatternTex ("Dither Pattern", 2D) = "white" {}

		_DitherAmountMin ("Dither Amount Min", Float) = -0.5
		_DitherAmountMax ("Dither Amount Max", Float) = 0.5

		_LevelCount ("Quantization Levels", Vector) = (1, 1, 1, 0)
		_Smoothness ("Quantization Smoothness", Range(0,1)) = 0.0
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
			Name "DitherTextureBias"
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

			#include "Packages/za.co.gamelogic.fx.dithering/Runtime/Core/Shaders/Dithering.hlsl"
			
			DECLARE_TEX(_DitherPatternTex)
			float4 _DitherPatternTex_ST;

			float _DitherAmountMin;
			float _DitherAmountMax;
			float _ColorScale;

			float _Smoothness;
			float _Quantization;
			int4 _LevelCount;

			float4 frag(INPUT i) : SV_Target
			{
				float4 color = SAMPLE(_MainTex, i.uv);
				float2 tiled_uv = apply_tiling(i.uv, _DitherPatternTex_ST);
				float3 bias = SAMPLE(_DitherPatternTex, tiled_uv).rgb;

				color.rgb += lerp(_DitherAmountMin, _DitherAmountMax, bias);
				color = saturate(color);
				color = quantize_smooth(color, _LevelCount.xyz, _Smoothness);

				return color;
			}
			ENDHLSL
		}
	}
}
