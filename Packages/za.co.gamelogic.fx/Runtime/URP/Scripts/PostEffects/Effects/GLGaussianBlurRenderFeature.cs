#if GAMELOGIC_HAS_URP
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that applies a smooth, natural-looking blur using a Gaussian weight curve.
	/// See <see href="../common/docs/effects-reference-common.html#gaussian-blur"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.GaussianBlur)]
	public class GLGaussianBlurRenderFeature : SeparableRenderFeature
	{
		private static readonly int SigmaID = Shader.PropertyToID("_Sigma");
		
		[SerializeField] private float sigma = 1f;
		
		public float Sigma
		{
			get => sigma;
			set => sigma = value;
		}

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.GaussianBlur;

		protected override void SetMaterialProperties(Material material)
		{
			material.SetFloat(SigmaID, sigma);
		}
	}
}
#endif
