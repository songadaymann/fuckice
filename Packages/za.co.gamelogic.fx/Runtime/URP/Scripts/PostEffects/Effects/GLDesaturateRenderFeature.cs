#if GAMELOGIC_HAS_URP 
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process to see the image's luminosity.
	/// See <see href="../common/docs/effects-reference-common.html#desaturate"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.Desaturate)]
	public sealed class GLDesaturateRenderFeature : PostProcessRenderFeature
	{
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.Desaturate;
	
		protected override PostEffectPass GetPass() => CreateSimplePass(ShaderNames.Desaturate, SetMaterialProperties);

		private static void SetMaterialProperties(Material material)
		{
		}
	}
}
#endif
