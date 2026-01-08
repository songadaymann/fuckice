#if GAMELOGIC_HAS_URP
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that replaces each pixel with the minimum color value found in its neighborhood.
	/// See <see href="../common/docs/effects-reference-common.html#min-filter"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.MinFilter)]
	public class GLMinRenderFeature : SeparableRenderFeature
	{
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.Min;
	}
}
#endif
