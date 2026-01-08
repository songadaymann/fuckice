using Gamelogic.Fx.Internal;
using UnityEngine;
using Unity.Mathematics;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process that reduces the number of distinct color values in the image,
	/// creating a posterized or stylized effect.
	/// See <see href="../common/docs/effects-reference-common.html#quantize"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.Quantize)]
	public sealed class QuantizePostProcess : NamedShaderPostProcess
	{
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.Quantize;

		// TODO Implement UI for locking and limiting to correct range. 
		[Tooltip("Number of quantization levels (per channel)")]
		[SerializeField] private int3 levelCount = new int3(2, 2, 2);
		
		[Tooltip("Smoothing (0 = sharp, 1 = smooth)")]
		[Range(0.0f, 1.0f)]
		[SerializeField] private float smoothness = 0.0f;
		
		public int3 Levels
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
	
		protected override void SetMaterialProperties(Material effectMaterial)
		{
			effectMaterial.SetVector(LevelCountID, levelCount.ToVector());
			effectMaterial.SetFloat(SmoothnessID, smoothness);
		}
	}
}
