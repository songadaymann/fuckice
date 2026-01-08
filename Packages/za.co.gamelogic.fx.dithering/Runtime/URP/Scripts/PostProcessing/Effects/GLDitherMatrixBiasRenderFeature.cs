#if GAMELOGIC_HAS_URP
using System.Linq;
using Gamelogic.Fx.Internal;
using Gamelogic.Fx.URP.PostProcessing;
using UnityEngine;
using Constants = Gamelogic.Fx.Dithering.PostProcessing.Effects.Internal.Constants;
using ShaderNames = Gamelogic.Fx.Dithering.Internal.ShaderNames;
using HelpURLs = Gamelogic.Fx.Dithering.PostProcessing.Effects.Internal.HelpURLs;

using Gamelogic.Extensions;

namespace Gamelogic.Fx.Dithering.URP
{

	/*
		This is the canonical URP RenderFeature wrapper
		for the DitherMatrixBias URP shader.

		Follows the exact same pattern as GLAdjustGammaRenderFeature.
	*/

	/// <summary>
	/// A post process that applies a data-driven dither matrix.
	/// See <see href="../dithering/docs/effects-reference-dithering.html#dither-matrix-bias"/>
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.DitherMatrixBias)]
	public sealed class GLDitherMatrixBiasRenderFeature : PostProcessRenderFeature
	{
		private static readonly int PixelSizeID = Shader.PropertyToID("_PixelSize");
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int SmoothnessID = Shader.PropertyToID("_Smoothness");

		private static readonly int DitherMinID = Shader.PropertyToID("_DitherAmountMin");
		private static readonly int DitherMaxID = Shader.PropertyToID("_DitherAmountMax");

		private static readonly int MatrixRWidthID = Shader.PropertyToID("_MatrixRWidth");
		private static readonly int MatrixRHeightID = Shader.PropertyToID("_MatrixRHeight");
		private static readonly int MatrixRValuesID = Shader.PropertyToID("_MatrixR");

		private static readonly int MatrixGWidthID = Shader.PropertyToID("_MatrixGWidth");
		private static readonly int MatrixGHeightID = Shader.PropertyToID("_MatrixGHeight");
		private static readonly int MatrixGValuesID = Shader.PropertyToID("_MatrixG");

		private static readonly int MatrixBWidthID = Shader.PropertyToID("_MatrixBWidth");
		private static readonly int MatrixBHeightID = Shader.PropertyToID("_MatrixBHeight");
		private static readonly int MatrixBValuesID = Shader.PropertyToID("_MatrixB");

		[SerializeField] private Vector2 pixelSize = new Vector2(2, 2);

		[SerializeField] private Vector3Int levelCount = new Vector3Int(2, 2, 2);

		[Range(0, 1)]
		[SerializeField]
		private float smoothness = 0f;

		[Range(-1f, 1f)]
		[SerializeField] private float ditherAmountMin = -0.1f;

		[Range(-1f, 1f)]
		[SerializeField] private float ditherAmountMax = .1f;

		[Presets(nameof(DitherMatrixPresets), nameof(DitherMatrixPresets))]
		[SerializeField] private FloatMatrix matrixR = DitherMatrixPresets.Checker.Clone();

		[Presets(nameof(DitherMatrixPresets), nameof(DitherMatrixPresets))]
		[SerializeField] private FloatMatrix matrixG = DitherMatrixPresets.Checker.Clone();

		[Presets(nameof(DitherMatrixPresets), nameof(DitherMatrixPresets))]
		[SerializeField] private FloatMatrix matrixB = DitherMatrixPresets.Checker.Clone();
		
		public Vector3Int LevelCount
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

		protected override string ShaderName =>
			Constants.ShaderNameRoot + ShaderNames.DitherMatrixBias;

		protected override PostEffectPass GetPass() =>
			CreateSimplePass(ShaderNames.DitherMatrixBias, SetMaterialProperties);

		private void SetMaterialProperties(Material material)
		{
			material.SetVector(PixelSizeID, pixelSize);
			material.SetVector(LevelCountID, (Vector3)levelCount);
			material.SetFloat(SmoothnessID, smoothness);

			material.SetFloat(DitherMinID, ditherAmountMin);
			material.SetFloat(DitherMaxID, ditherAmountMax);

			material.SetInt(MatrixRWidthID, matrixR.Width);
			material.SetInt(MatrixRHeightID, matrixR.Height);
			material.SetFloatArray(MatrixRValuesID, matrixR.Normalize().Values.ToArray());

			material.SetInt(MatrixGWidthID, matrixG.Width);
			material.SetInt(MatrixGHeightID, matrixG.Height);
			material.SetFloatArray(MatrixGValuesID, matrixG.Normalize().Values.ToArray());

			material.SetInt(MatrixBWidthID, matrixB.Width);
			material.SetInt(MatrixBHeightID, matrixB.Height);
			material.SetFloatArray(MatrixBValuesID, matrixB.Normalize().Values.ToArray());
		}
	}
}
#endif
