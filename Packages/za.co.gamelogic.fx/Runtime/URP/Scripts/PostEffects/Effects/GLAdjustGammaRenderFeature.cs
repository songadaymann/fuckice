#if GAMELOGIC_HAS_URP 
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/*
		This is the canonical implementation of the PostProcessRenderFeature.
		
		Why the GL prefix?  
			To prevent collisions with other code that may have implemented similar render features. 
		Why the RenderFeature suffix?
			To prevent collisions with our BuiltIn classes (otherwise the AddRenderFeature may try to add the other class). 
	*/

	/// <summary>
	/// A post process that adjust the gamma of the image.
	/// See <see href="../common/docs/effects-reference-common.html#adjust-gamma"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.AdjustGamma)]
	public sealed class GLAdjustGammaRenderFeature : PostProcessRenderFeature
	{
		private static readonly int GammaID = Shader.PropertyToID("_Gamma");

		[Min(0.1f)]
		[SerializeField] private float gamma = 1f;
		
		public float Gamma
		{
			get => gamma;
			set
			{
				value.ThrowIfNegative(nameof(value));
				gamma = value;
			}
		}
	
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.AdjustGamma;
	
		protected override PostEffectPass GetPass() => CreateSimplePass(ShaderNames.AdjustGamma, SetMaterialProperties);

		private void SetMaterialProperties(Material material)
		{
			material.SetFloat(GammaID, gamma);
		}
	}
}
#endif
