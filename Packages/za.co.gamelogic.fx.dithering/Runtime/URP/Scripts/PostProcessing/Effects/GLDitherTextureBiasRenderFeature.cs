#if GAMELOGIC_HAS_URP
using Gamelogic.Extensions;
using Gamelogic.Fx.Internal;
using UnityEngine;
using Gamelogic.Fx.URP.PostProcessing;

using Constants = Gamelogic.Fx.Dithering.PostProcessing.Effects.Internal.Constants;
using ShaderNames = Gamelogic.Fx.Dithering.Internal.ShaderNames;
using HelpURLs = Gamelogic.Fx.Dithering.PostProcessing.Effects.Internal.HelpURLs;

namespace Gamelogic.Fx.Dithering.URP
{
	/// <summary>
	/// Post process effect that applies quantization with a dither texture bias pattern.
	/// See <see href="../dithering/docs/effects-reference-dithering.html#dither-texture-bias"/>
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.DitherMatrixBias)]
	public sealed class GLDitherTextureBiasRenderFeature : PostProcessRenderFeature
	{
		private static readonly int DitherAmountMinID = Shader.PropertyToID("_DitherAmountMin");
		private static readonly int DitherAmountMaxID = Shader.PropertyToID("_DitherAmountMax");
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");

		[Header("Dithering")]
		[SerializeField] private Texture2DTiling ditherPattern = new Texture2DTiling();
		
		[Range(-1f, 1f)]
		[SerializeField] private float minDither = -0.5f;
		
		[Range(-1f, 1f)]
		[SerializeField] private float maxDither = 0.5f;

		[Header("Quantization")]
		[SerializeField] private Vector3 levelCount = new Vector3(2f, 2f, 2f);
		[SerializeField] [Range(0f, 1f)] private float smoothness = 0.5f;
		
		public Texture2D DitherPatternTexture
		{
			get => ditherPattern.Texture;
			set => ditherPattern.Texture = value;
		}

		public Vector2 DitherPatternTilingScale
		{
			get => ditherPattern.TilingScale;
			set => ditherPattern.TilingScale = value;
		}

		public Vector2 DitherPatternTilingOffset
		{
			get => ditherPattern.TilingOffset;
			set => ditherPattern.TilingOffset = value;
		}
		
		public Vector3 LevelCount
		{
			get => levelCount;
			set => levelCount = value;
		}

		public float Smoothness
		{
			get => smoothness;
			set
			{
				value.ThrowIfOutOfRange(0f, 1f, nameof(value));
				smoothness = value;
			}
		}

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.DitherTextureBias;

		protected override PostEffectPass GetPass() => CreateSimplePass(ShaderNames.DitherTextureBias, SetMaterialProperties);

		private void SetMaterialProperties(Material material)
		{
			material.SetTextureTiling("_DitherPatternTex", ditherPattern);
			
			material.SetFloat(DitherAmountMinID, minDither);
			material.SetFloat(DitherAmountMaxID, maxDither);

			material.SetVector(LevelCountID, new Vector4(levelCount.x, levelCount.y, levelCount.z, 1f));
			material.SetFloat(SmoothnessID, smoothness);
		}
	}
}
#endif
