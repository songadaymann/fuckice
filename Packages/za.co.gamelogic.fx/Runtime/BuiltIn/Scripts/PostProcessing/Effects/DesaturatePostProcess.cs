using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process to see the image's luminosity.
	/// See <see href="../common/docs/effects-reference-common.html#desaturate"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.Desaturate)]
	public sealed class DesaturatePostProcess : NamedShaderPostProcess
	{
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.Desaturate;
	}
}
