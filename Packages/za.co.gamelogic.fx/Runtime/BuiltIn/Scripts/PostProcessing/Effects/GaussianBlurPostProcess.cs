using Gamelogic.Fx.Internal;
using Gamelogic.Extensions.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process that applies a smooth, natural-looking blur using a Gaussian weight curve.
	/// See <see href="../common/docs/effects-reference-common.html#gaussian-blur"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.GaussianBlur)]
	[Version(1, 1, 0)]
	public class GaussianBlurPostProcess : SeparableNamedShaderPostProcess
	{
		private static readonly int SigmaID = Shader.PropertyToID("_Sigma");
		
		[SerializeField] private float sigma = 1f;

		public float Sigma
		{
			get => sigma;
			set => sigma = value;
		}

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.GaussianBlur;

		protected override void SetMaterialProperties(Direction _, Material effectMaterial)
		{
			effectMaterial.SetFloat(SigmaID, sigma);
		}
	}
}
