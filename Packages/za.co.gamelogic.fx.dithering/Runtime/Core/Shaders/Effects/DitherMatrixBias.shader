Shader "Gamelogic/Fx/Dithering/DitherMatrixBias"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}

		_PixelSize("Pixel Size", Vector) = (1,1,0,0)
		_LevelCount("Quantization Levels", Vector) = (1,1,1,0)
		_Smoothness("Smoothness", Float) = 0

		_DitherAmountMin("Dither Amount Min", Float) = 0
		_DitherAmountMax("Dither Amount Max", Float) = 1

		_MatrixRWidth("Matrix R Width", Int) = 2
		_MatrixRHeight("Matrix R Height", Int) = 2

		_MatrixGWidth("Matrix G Width", Int) = 2
		_MatrixGHeight("Matrix G Height", Int) = 2

		_MatrixBWidth("Matrix B Width", Int) = 2
		_MatrixBHeight("Matrix B Height", Int) = 2
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature GAMELOGIC_HAS_URP

			#ifdef GAMELOGIC_HAS_URP
			#define UNITY_PIPELINE_URP 1
			#endif

			#include "Packages/za.co.gamelogic.fx.dithering/Runtime/Core/Shaders/Dithering.hlsl"

			#define MAX_MATRIX_SIZE 256

			#define SAMPLE_MATRIX(NAME, ARRAY, WIDTH, HEIGHT)                     \
				float NAME(float2 uv)                                              \
				{                                                                  \
					int2 index = pixel_mod(                                         \
						uv ,                                                         \
						_ScreenParams.xy/ _PixelSize,                                \
						uint2(WIDTH, HEIGHT)                                         \
					);                                                              \
					int flatIndex = index.y * WIDTH + index.x;                      \
					return ARRAY[flatIndex];                                        \
				}

			float _DitherAmountMin;
			float _DitherAmountMax;
			int3 _LevelCount;

			float _MatrixR[MAX_MATRIX_SIZE];
			int _MatrixRWidth;
			int _MatrixRHeight;

			float _MatrixG[MAX_MATRIX_SIZE];
			int _MatrixGWidth;
			int _MatrixGHeight;

			float _MatrixB[MAX_MATRIX_SIZE];
			int _MatrixBWidth;
			int _MatrixBHeight;

			float2 _PixelSize;
			float _Smoothness;

			SAMPLE_MATRIX(sample_matrix_r, _MatrixR, _MatrixRWidth, _MatrixRHeight)
			SAMPLE_MATRIX(sample_matrix_g, _MatrixG, _MatrixGWidth, _MatrixGHeight)
			SAMPLE_MATRIX(sample_matrix_b, _MatrixB, _MatrixBWidth, _MatrixBHeight)

			float4 frag(INPUT i) : SV_Target
			{
				float4 color = SAMPLE(_MainTex, i.uv);
				float3 bias;

				bias.r = sample_matrix_r(i.uv);
				bias.g = sample_matrix_g(i.uv);
				bias.b = sample_matrix_b(i.uv);

				color.rgb += lerp(_DitherAmountMin, _DitherAmountMax, bias);
				color.rgb = quantize_smooth(color.rgb, _LevelCount, _Smoothness);
				color = saturate(color);

				return color;
			}
			ENDHLSL
		}
	}
}
