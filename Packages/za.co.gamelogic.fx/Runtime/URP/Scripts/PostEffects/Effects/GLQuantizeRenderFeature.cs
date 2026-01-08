#if GAMELOGIC_HAS_URP 
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that reduces the number of distinct color values in the image,
	/// creating a posterized or stylized effect.
	/// See <see href="../common/docs/effects-reference-common.html#quantize"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.Quantize)]
	public sealed class GLQuantizeRenderFeature : PostProcessRenderFeature
	{
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int SmoothingID = Shader.PropertyToID("_Smoothness");

		[Header("Settings")]
		[Tooltip("Number of quantization levels (per channel)")]
		[SerializeField] private Vector3 levelCount = new Vector3(2f, 2f, 2f);
		
		[Tooltip("SmoothingID (0 = sharp, 1 = smooth)")]
		[SerializeField, Range(0f, 1f)] private float smoothness = 0.5f;
		
		public Vector3 Levels
		{
			get => levelCount;
			set => levelCount = value;
		}
		
		public float Smoothness
		{
			get => smoothness;
			set
			{
				value.ThrowIfOutOfRange(0f, 1f, nameof(value));
				smoothness = value;
			}
		}

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.Quantize;

		protected override PostEffectPass GetPass() => CreateSimplePass(ShaderNames.Quantize, SetMaterialProperties);

		private void SetMaterialProperties(Material material)
		{
			material.SetVector(LevelCountID, new Vector4(levelCount.x, levelCount.y, levelCount.z, 1f));
			material.SetFloat(SmoothingID, smoothness);
		}
	}
}
#endif
