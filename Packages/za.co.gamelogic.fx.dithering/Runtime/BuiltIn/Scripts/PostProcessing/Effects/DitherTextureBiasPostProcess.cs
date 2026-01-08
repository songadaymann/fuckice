using Gamelogic.Fx.Internal;
using Gamelogic.Extensions;
using UnityEngine;
using Gamelogic.Fx.BuiltIn.PostProcessing;
using Gamelogic.Fx.Dithering.PostProcessing.Effects.Internal;
using Constants = Gamelogic.Fx.Dithering.PostProcessing.Effects.Internal.Constants;
using ShaderNames = Gamelogic.Fx.Dithering.Internal.ShaderNames;
using HelpURLs = Gamelogic.Fx.Dithering.PostProcessing.Effects.Internal.HelpURLs;

namespace Gamelogic.Fx.Dithering.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// Post process effect that applies quantization with a dither texture bias pattern.
	/// See <see href="../dithering/docs/effects-reference-dithering.html#dither-texture-bias"/>
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.DitherTextureBias)]
	public sealed class DitherTextureBiasPostProcess : NamedShaderPostProcess
	{
		private static readonly int DitherAmountMinID = Shader.PropertyToID("_DitherAmountMin");
		private static readonly int DitherAmountMaxID = Shader.PropertyToID("_DitherAmountMax");
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");
		
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.DitherTextureBias;

		[SerializeField] private Texture2DTiling ditherPattern = new Texture2DTiling();

		[Range(-1f, 1f)] 
		[SerializeField] private float ditherAmountMin = -0.5f;

		[Range(-1f, 1f)] 
		[SerializeField,] private float ditherAmountMax = 0.5f;
		
		[SerializeField] private Vector3 levelCount = new Vector3(1f, 1f, 1f);

		[Range(0f, 1f)]
		[SerializeField] private float smoothness = 0f;

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

		public float DitherAmountMin
		{
			get => ditherAmountMin;
			set
			{
				value.ThrowIfOutOfRange(-1f, 1f, nameof(value));
				ditherAmountMin = value;
			}
		}

		public float DitherAmountMax
		{
			get => ditherAmountMax;
			set
			{
				value.ThrowIfOutOfRange(-1f, 1f, nameof(value));
				ditherAmountMax = value;
			}
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

		protected override void SetMaterialProperties(Material effectMaterial)
		{
			effectMaterial.SetTextureTiling("_DitherPatternTex", ditherPattern);
			effectMaterial.SetFloat(DitherAmountMinID, ditherAmountMin);
			effectMaterial.SetFloat(DitherAmountMaxID, ditherAmountMax);
			effectMaterial.SetVector(LevelCountID, new Vector4(levelCount.x, levelCount.y, levelCount.z, 1f));
			effectMaterial.SetFloat(SmoothnessID, smoothness);
		}
	}
}
