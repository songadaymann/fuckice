Shader "Gamelogic/Fx/BoxBlur"
{
	Properties
	{
		_MainTex ("Render Texture", 2D) = "white" {}

		_KernelOffset( "Kernel Offset", float ) = 0.0
		_KernelSize ("Kernel Jump Size", Integer) = 1.0
		_KernelJumpSize ("Kernel Jump Size", float) = 1.0
		_Direction ("Kernel Direction", Vector) = (1, 0, 0, 0)
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
			Name "BoxBlur"
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
			float _KernelOffset;
			int _KernelSize;
			float _KernelJumpSize;
			float2 _Direction;

			float4 frag(INPUT i) : SV_Target
			{
				const float2 unit_offset = _Direction * _MainTex_TexelSize.xy * _KernelJumpSize;
				float4 sum = 0;

				for (int x = 0; x < _KernelSize; x++)
				{
					float2 offset = (x + _KernelOffset) * unit_offset;
					sum += SAMPLE(_MainTex, i.uv + offset);
				}

				return sum / _KernelSize;
			}

			ENDHLSL
		}
	}

	Fallback Off
}
