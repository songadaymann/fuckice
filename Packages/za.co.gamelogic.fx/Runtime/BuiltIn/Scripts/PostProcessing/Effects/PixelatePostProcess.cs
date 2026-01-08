using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process that applies a pixelation effect to the image
	/// by sampling blocks of pixels instead of individual texels.
	/// See <see href="../common/docs/effects-reference-common.html#pixelate"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.Pixelate)]
	public sealed class PixelatePostProcess : NamedShaderPostProcess
	{
		private static readonly int PixelSizeID = Shader.PropertyToID("_PixelSize");
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.Pixelate;
	
		[SerializeField] private Vector2 pixelSize = new Vector2(2, 2);
		
		public Vector2 PixelSize
		{
			get => pixelSize;
			set => pixelSize = value;
		}

		protected override void SetMaterialProperties(Material effectMaterial)
		{
			effectMaterial.SetVector(PixelSizeID, pixelSize);
		}
	}
}
