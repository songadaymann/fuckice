Shader "Gamelogic/Fx/Min"
{
	Properties
	{
		_MainTex ("Render Texture", 2D) = "white" {}

		_KernelOffset( "Kernel Offset", float ) = 0.0
		_KernelSize ("Kernel Size", Integer) = 1.0
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
			Name "Min"
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

			float3 min_filter_1D(float2 uv)
			{
				const float large_number = 1e30;
				float3 min_color = float3(large_number, large_number, large_number);

				float2 unit_offset = _Direction * _MainTex_TexelSize.xy * _KernelJumpSize;

				for (int x = 0; x < _KernelSize; x++)
				{
					float2 offset = (x + _KernelOffset) * unit_offset;
					float3 color = SAMPLE(_MainTex, uv + offset).rgb;

					min_color = min(min_color, color);
				}

				return min_color;
			}

			float4 frag(INPUT i) : SV_Target
			{
				float3 blurred_color = min_filter_1D(i.uv);
				return RGB1(blurred_color);
			}
			ENDHLSL
		}
	}

	Fallback Off
}
