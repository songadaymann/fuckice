Shader "Gamelogic/Fx/AdjustGamma"
{
	Properties
	{
		_MainTex ("Render Texture", 2D) = "white" {}
		_Gamma ("Gamma Value", Float) = 1.0
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
			Name "AdjustGamma"
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

			float _Gamma;

			float4 frag(INPUT i) : SV_Target
			{
				float4 color = SAMPLE(_MainTex, i.uv);
				color = adjust_gamma(color, _Gamma);

				return color;
			}
			ENDHLSL
		}
	}

	Fallback Off
}
