#if GAMELOGIC_HAS_URP
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that applies a uniform blur by averaging neighbour pixels colors.
	/// See <see href="../common/docs/effects-reference-common.html#box-blur"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.BoxBlur)]
	public class GLBoxBlurRenderFeature : SeparableRenderFeature
	{
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.BoxBlur;
	}
}
#endif
