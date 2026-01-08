using System.Linq;
using Gamelogic.Extensions;
using Gamelogic.Fx.BuiltIn.PostProcessing;
using Gamelogic.Fx.Internal;
using Unity.Mathematics;
using UnityEngine;
using ShaderNames = Gamelogic.Fx.Dithering.Internal.ShaderNames;
using Constants = Gamelogic.Fx.Dithering.PostProcessing.Effects.Internal.Constants;
using HelpURLs = Gamelogic.Fx.Dithering.PostProcessing.Effects.Internal.HelpURLs;

namespace Gamelogic.Fx.Dithering.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process that applies a data-driven dither matrix.
	/// See <see href="../dithering/docs/effects-reference-dithering.html#dither-matrix-bias"/>
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.DitherMatrixBias)]
	public sealed class DitherMatrixBiasPostProcess : NamedShaderPostProcess
	{
		private static readonly int MatrixRID = Shader.PropertyToID("_MatrixR");
		private static readonly int MatrixRWidthID = Shader.PropertyToID("_MatrixRWidth");
		private static readonly int MatrixRHeightID = Shader.PropertyToID("_MatrixRHeight");
		
		private static readonly int MatrixGID = Shader.PropertyToID("_MatrixG");
		private static readonly int MatrixGWidthID = Shader.PropertyToID("_MatrixGWidth");
		private static readonly int MatrixGHeightID = Shader.PropertyToID("_MatrixGHeight");
		
		private static readonly int MatrixBID = Shader.PropertyToID("_MatrixB");
		private static readonly int MatrixBWidthID = Shader.PropertyToID("_MatrixBWidth");
		private static readonly int MatrixBHeightID = Shader.PropertyToID("_MatrixBHeight");
		
		private static readonly int PixelSizeID = Shader.PropertyToID("_PixelSize");
		private static readonly int DitherAmountMinID = Shader.PropertyToID("_DitherAmountMin");
		private static readonly int DitherAmountMaxID = Shader.PropertyToID("_DitherAmountMax");
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");

		[SerializeField] private int3 levelCount = 4 * math.int3(2, 2, 2);
		
		[Presets(nameof(DitherMatrixPresets), nameof(DitherMatrixPresets))]
		[SerializeField] private FloatMatrix matrixR = DitherMatrixPresets.Checker.Clone();
		
		[Presets(nameof(DitherMatrixPresets), nameof(DitherMatrixPresets))]
		[SerializeField] private FloatMatrix matrixG = DitherMatrixPresets.Checker.Clone();
		
		[Presets(nameof(DitherMatrixPresets), nameof(DitherMatrixPresets))]
		[SerializeField] private FloatMatrix matrixB = DitherMatrixPresets.Checker.Clone();
		
		[SerializeField] private Vector2 pixelSize = Vector2.one;
		
		[Range(-1f, 1f)]
		[SerializeField] private float ditherAmountMin = -0.1f;
		
		[Range(-1f, 1f)]
		[SerializeField] private float ditherAmountMax = 0.1f;
		
		[Range(0f, 1f)]	
		[SerializeField] private float smoothness = 0.1f;

		public int3 LevelCount
		{
			get => levelCount;
			set => levelCount = value;
		}
		
		public FloatMatrix MatrixR
		{
			get => matrixR;
			set => matrixR = value;
		}
		
		public FloatMatrix MatrixG
		{
			get => matrixG;
			set => matrixG = value;
		}
		
		public FloatMatrix MatrixB
		{
			get => matrixB;
			set => matrixB = value;
		}
		
		public Vector2 PixelSize
		{
			get => pixelSize;
			set => pixelSize = value;
		}

		public float DitherAmountMin
		{
			get => ditherAmountMin;
			set
			{
				value.ThrowIfOutOfRange(-1f, 1f, nameof(value));
				ditherAmountMin = value;
			}
		}

		public float DitherAmountMax
		{
			get => ditherAmountMax;
			set
			{
				value.ThrowIfOutOfRange(-1f, 1f, nameof(value));
				ditherAmountMax = value;
			}
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

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.DitherMatrixBias;
		
		protected override void SetMaterialProperties(Material effectMaterial)
		{
			effectMaterial.SetInt(MatrixRWidthID, matrixR.Width);
			effectMaterial.SetInt(MatrixRHeightID, matrixR.Height);
			effectMaterial.SetFloatArray(MatrixRID, matrixR.Normalize().Values.ToArray());
			
			effectMaterial.SetInt(MatrixGWidthID, matrixG.Width);
			effectMaterial.SetInt(MatrixGHeightID, matrixG.Height);
			effectMaterial.SetFloatArray(MatrixGID, matrixG.Normalize().Values.ToArray());
			
			effectMaterial.SetInt(MatrixBWidthID, matrixB.Width);
			effectMaterial.SetInt(MatrixBHeightID, matrixB.Height);
			effectMaterial.SetFloatArray(MatrixBID, matrixB.Normalize().Values.ToArray());
			
			effectMaterial.SetVector(PixelSizeID, pixelSize);
			effectMaterial.SetFloat(DitherAmountMinID, ditherAmountMin);
			effectMaterial.SetFloat(DitherAmountMaxID, ditherAmountMax);
			effectMaterial.SetFloat(SmoothnessID, smoothness);
			effectMaterial.SetVector(LevelCountID, levelCount.ToVector4XYZ());
		}
	}
}
