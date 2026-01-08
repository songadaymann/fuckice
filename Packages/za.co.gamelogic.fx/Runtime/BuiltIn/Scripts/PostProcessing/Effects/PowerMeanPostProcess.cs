using Gamelogic.Fx.Internal;
using Gamelogic.Extensions.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process that blurs the image by computing a power mean (p-norm mean) of neighboring pixels.
	/// See <see href="../common/docs/effects-reference-common.html#power-mean"/>.
	/// </summary>
	/* This is the canonical implementation of a separable filter.
	*/
	[HelpURL(Constants.HelpURLRoot + HelpURLs.PowerMean)]

	[Version(1, 1, 0)]
	public class PowerMeanPostProcess : SeparableNamedShaderPostProcess
	{
		private static readonly int PowerID = Shader.PropertyToID("_Power");
		
		[SerializeField] private float power = 2.0f;
		
		/// <summary>
		/// The power to use in the power mean calculation.
		/// </summary>
		public float Power
		{
			get => power;
			set => power = value;
		}

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.PowerMean;

		protected override void SetMaterialProperties(Direction _, Material effectMaterial)
		{
			effectMaterial.SetFloat(PowerID, power);
		}
	}
}
