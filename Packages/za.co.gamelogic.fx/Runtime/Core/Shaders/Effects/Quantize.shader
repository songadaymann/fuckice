Shader "Gamelogic/Fx/Quantize"
{
	Properties
	{
		_MainTex ("Render Texture", 2D) = "white" {}
		_LevelCount ("Levels", Vector) = (2, 2, 2, 1)
		_Smoothness ("Smoothness", Float) = 0
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
			Name "Quantize"
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

			int3 _LevelCount;
			float _Smoothness;

			float4 frag(INPUT i) : SV_Target
			{
				float4 color = SAMPLE(_MainTex, i.uv);
				color = quantize_smooth(color, _LevelCount, _Smoothness);

				return color;
			}
			ENDHLSL
		}
	}

	Fallback Off
}
