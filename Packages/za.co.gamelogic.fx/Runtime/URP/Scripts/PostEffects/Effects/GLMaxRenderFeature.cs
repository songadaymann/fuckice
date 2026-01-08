#if GAMELOGIC_HAS_URP
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that replaces each pixel with the maximum color value found in its neighborhood.
	/// See <see href="../common/docs/effects-reference-common.html#max-filter"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.MaxFilter)]
	public class GLMaxRenderFeature : SeparableRenderFeature
	{
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.Max;
	}
}
#endif
