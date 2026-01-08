Shader "Gamelogic/Fx/BlendTexture"
{
	Properties
	{
		_MainTex ("Render Texture", 2D) = "white" {}
		_OverlayTex ("Overlay Texture", 2D) = "white" {}
		_Opacity ("Overlay Opacity", Float) = 1.0
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
			Name "BlendTexture"
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

			DECLARE_TEX(_OverlayTex)
			
			float4 _OverlayTex_ST;
			float _Opacity;

			float4 frag(INPUT i) : SV_Target
			{
				float4 color = SAMPLE(_MainTex, i.uv);
				float2 overlay_uv = apply_tiling(i.uv, _OverlayTex_ST);
				float4 overlay = SAMPLE(_OverlayTex, overlay_uv);
				color.rgb = lerp(color.rgb, overlay.rgb, overlay.a * _Opacity);

				return color;
			}
			ENDHLSL
		}
	}

	Fallback Off
}
