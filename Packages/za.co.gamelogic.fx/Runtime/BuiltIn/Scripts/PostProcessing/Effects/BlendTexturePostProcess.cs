using Gamelogic.Extensions;
using Gamelogic.Fx.Internal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process that outputs a fullscreen constant textureTiling.
	/// See <see href="../common/docs/effects-reference-common.html#blend-texture"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.BlendTexture)]
	public sealed class BlendTexturePostProcess : NamedShaderPostProcess
	{
		private static readonly int OpacityID = Shader.PropertyToID("_Opacity");
		
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.BlendTexture;

		[FormerlySerializedAs("texture")] 
		[FormerlySerializedAs("textureTiling")]
		[SerializeField] private Texture2DTiling overlayTexture = null;
		
		[SerializeField, Range(0f, 1f)] private float opacity = 1.0f;
		
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
		
		protected override void SetMaterialProperties(Material effectMaterial)
		{
			effectMaterial.SetTextureTiling("_OverlayTex", overlayTexture);
			effectMaterial.SetFloat(OpacityID, opacity);
		}
	}
}
