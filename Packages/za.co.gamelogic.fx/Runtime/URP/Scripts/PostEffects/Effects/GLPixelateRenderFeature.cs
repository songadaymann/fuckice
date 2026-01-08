#if GAMELOGIC_HAS_URP 
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that applies a pixelation effect to the image
	/// by sampling blocks of pixels instead of individual texels.
	/// See <see href="../common/docs/effects-reference-common.html#pixelate"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.Pixelate)]
	public sealed class GLPixelateRenderFeature : PostProcessRenderFeature
	{
		private static readonly int PixelSizeID = Shader.PropertyToID("_PixelSize");

		[SerializeField, Min(0f)]
		private Vector2 pixelSize = new Vector2(2, 2);
		
		public Vector2 PixelSize
		{
			get => pixelSize;
			set => pixelSize = value;
		}

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.Pixelate;

		protected override PostEffectPass GetPass() => CreateSimplePass(ShaderNames.Pixelate, SetMaterialProperties);

		private void SetMaterialProperties(Material material)
		{
			material.SetVector(PixelSizeID, pixelSize);
		}
	}
}
#endif
