using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process that smooths the image using an edge-preserving, noise-reducing filter.
	/// See <see href="../common/docs/effects-reference-common.html#bilateral-filter"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.BilateralFilter)]
	public class BilateralFilterPostProcess : SeparableNamedShaderPostProcess
	{
		private static readonly int SpatialSigmaID = Shader.PropertyToID("_SpatialSigma");
		private static readonly int RangeSigmaID = Shader.PropertyToID("_RangeSigma");

		[SerializeField] private float spatialSigma = 2.0f;
		[SerializeField] private float rangeSigma = 0.1f;
		
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.BilateralFilter;

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

		protected override void SetMaterialProperties(Direction _, Material effectMaterial)
		{
			effectMaterial.SetFloat(SpatialSigmaID, spatialSigma);
			effectMaterial.SetFloat(RangeSigmaID, rangeSigma);
		}
	}
}
