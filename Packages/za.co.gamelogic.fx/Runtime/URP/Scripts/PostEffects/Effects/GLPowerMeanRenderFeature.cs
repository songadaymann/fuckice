#if GAMELOGIC_HAS_URP
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that blurs the image by computing a power mean (p-norm mean) of neighboring pixels.
	/// See <see href="../common/docs/effects-reference-common.html#power-mean"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.PowerMean)]
	public class GLPowerMeanRenderFeature : SeparableRenderFeature
	{
		private static readonly int PowerID = Shader.PropertyToID("_Power");
		
		[SerializeField] private float power = 2f;
		
		public float Power
		{
			get => power;
			set => power = value;
		}

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.PowerMean;

		protected override void SetMaterialProperties(Material material)
		{
			material.SetFloat(PowerID, power);
		}
	}
}
#endif
