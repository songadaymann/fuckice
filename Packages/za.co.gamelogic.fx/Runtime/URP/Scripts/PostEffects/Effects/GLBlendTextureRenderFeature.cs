#if GAMELOGIC_HAS_URP
using Gamelogic.Extensions;
using Gamelogic.Fx.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that outputs a fullscreen constant textureTiling.
	/// See <see href="../common/docs/effects-reference-common.html#blend-texture"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.BlendTexture)]
	public sealed class GLBlendTextureRenderFeature : PostProcessRenderFeature
	{
		private static readonly int OverlayTexID = Shader.PropertyToID("_OverlayTex");
		private static readonly int OpacityID = Shader.PropertyToID("_Opacity");
		
		[SerializeField] private Texture2DTiling overlayTexture = new Texture2DTiling();
		
		[Range(0f, 1f)]
		[FormerlySerializedAs("alpha")]
		[SerializeField] private float opacity = 0.5f;
		
		public Texture2D OverlayTexture
		{
			get => overlayTexture.Texture;
			set => overlayTexture.Texture = value;
		}

		public Vector2 OverlayTextureTilingScale
		{
			get => overlayTexture.TilingScale;
			set => overlayTexture.TilingScale = value;
		}

		public Vector2 OverlayTextureTilingOffset
		{
			get => overlayTexture.TilingOffset;
			set => overlayTexture.TilingOffset = value;
		}

		public float Opacity
		{
			get => opacity;
			set
			{
				value.ThrowIfOutOfRange(0f, 1f, nameof(value));
				opacity = value;
			}
		}

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.BlendTexture;

		protected override PostEffectPass GetPass() => CreateSimplePass(ShaderNames.BlendTexture, SetMaterialProperties);

		private void SetMaterialProperties(Material material)
		{
			material.SetTextureTiling("_OverlayTex", overlayTexture);
			material.SetFloat(OpacityID, opacity);
		}
	}
}
#endif
