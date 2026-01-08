using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/*
		This is the canonical implementation of the GenericPostProcess.
		
		The common suffix is to distinguish from RenderFeatures. 
	*/
	
	/// <summary>
	/// A post process that adjust the gamma of the image.
	/// See <see href="../common/docs/effects-reference-common.html#adjust-gamma"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.AdjustGamma)]
	public sealed class AdjustGammaPostProcess : NamedShaderPostProcess
	{
		private static readonly int GammaID = Shader.PropertyToID("_Gamma");
		
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.AdjustGamma;

		[Min(0.01f)]
		[SerializeField] private float gamma = 1.0f;

		public float Gamma
		{
			get => gamma;
			set
			{
				value.ThrowIfNegative(nameof(value));
				gamma = value;
			}
		}

		protected override void SetMaterialProperties(Material effectMaterial)
		{
			effectMaterial.SetFloat(GammaID, gamma);
		}
	}
}
