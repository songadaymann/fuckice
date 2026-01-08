#if GAMELOGIC_HAS_URP
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that smooths the image using an edge-preserving, noise-reducing filter.
	/// See <see href="../common/docs/effects-reference-common.html#bilateral-filter"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.BilateralFilter)]
	public class GLBilateralFilterRenderFeature : SeparableRenderFeature
	{
		private static readonly int SpatialSigmaID = Shader.PropertyToID("_SpatialSigma");
		private static readonly int RangeSigmaID = Shader.PropertyToID("_RangeSigma");
		
		[SerializeField] private float spatialSigma = 1f;
		[SerializeField] private float rangeSigma = 0.1f;
		
		public float SpatialSigma
		{
			get => spatialSigma;
			set
			{
				value.ThrowIfNotPositive(nameof(value));
				spatialSigma = value;
			}
		}

		public float RangeSigma
		{
			get => rangeSigma;
			set
			{
				value.ThrowIfNotPositive(nameof(value));
				rangeSigma = value;
			}
		}

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.BilateralFilter;

		protected override void SetMaterialProperties(Material material)
		{
			material.SetFloat(SpatialSigmaID, spatialSigma);
			material.SetFloat(RangeSigmaID, rangeSigma);
		}
	}
}
#endif
