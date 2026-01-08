using Gamelogic.Extensions;
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process that overlays a texture on top of the scene, respecting transparency.
	/// See <see href="../common/docs/effects-reference-common.html#add-texture"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.AddTexture)]
	public sealed class AddTexturePostProcess : NamedShaderPostProcess
	{
		private static readonly int MinID = Shader.PropertyToID("_Min");
		private static readonly int MaxID = Shader.PropertyToID("_Max");
		
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.AddTexture;
	
		[SerializeField] private Texture2DTiling overlayTexture = null;

		[LockableVectorRange(-2, 2)] 
		[SerializeField] private LockableVector3 minRGB = new LockableVector3() { vector = -0.1f * Vector3.one, locked = true };
		
		[LockableVectorRange(-2, 2)]
		[SerializeField] private LockableVector3 maxRGB = new LockableVector3() { vector = 0.1f * Vector3.one, locked = true };
		
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
		
		public Vector3 MinRGB
		{
			get => minRGB.vector;
			set => minRGB.vector = value;
		}
		
		public Vector3 MaxRGB
		{
			get => maxRGB.vector;
			set => maxRGB.vector = value;
		}
		
		/// <inheritdoc/>
		protected override void SetMaterialProperties(Material effectMaterial)
		{
			effectMaterial.SetTextureTiling("_OverlayTex", overlayTexture);
			effectMaterial.SetVector(MinID, minRGB.vector);
			effectMaterial.SetVector(MaxID, maxRGB.vector);
		}
	}
}
