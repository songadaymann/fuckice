#if GAMELOGIC_HAS_URP
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions.Internal;
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that gradually shifts colors toward the provided primaries,
	/// producing posterized, clustered, or palette–constrained looks.
	/// See <see href="../common/docs/effects-reference-common.html#convex-hull-map"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.ConvexHullMap)]
	[Version(1, 0, 0)]
	public sealed class GLConvexHullHatchRenderFeature : PostProcessRenderFeature
	{
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int BackgroundColorID = Shader.PropertyToID("_BackgroundColor");
		private const string PrimaryColorBaseName = "_PrimaryColor";
		private const int MaxPrimaryColorCount = 10;

		private static readonly int MaxOffsetID = Shader.PropertyToID("_MaxOffset");
		private static readonly int DebugMaxLayerCountID = Shader.PropertyToID("_DebugMaxLayerCount");

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.ConvexHullMap;

		[SerializeField] private int levelCount = 3;
		[SerializeField] private Color backgroundColor = Constants.DefaultBackgroundColor;

		[SerializeField] private ColorShaderPropertyList primaryColors = Constants.DefaultPrimaryColorsCopy;
		
		[SerializeField] private float maxOffset = 0f;
		[SerializeField] private int debugMaxLayerCount = 0;
		
		/// <summary>
		/// Gets or sets the number of refinement iterations used for convex-hull projection.
		/// Higher values move colors progressively closer to the convex hull defined by the primary colors.
		/// </summary>
		public int LevelCount
		{
			get => levelCount;
			set
			{
				value.ThrowIfNotPositive(nameof(value));
				levelCount = value;
			}
		}

		/// <summary>
		/// Gets or sets the background reference color used as the starting point
		/// for the convex-hull projection process.
		/// </summary>
		public Color BackgroundColor
		{
			get => backgroundColor;
			set => backgroundColor = value;
		}

		/// <summary>
		/// Gets or sets the collection of primary colors that define the convex hull.
		/// Pixel colors are iteratively projected toward these palette anchors.
		/// </summary>
		public IEnumerable<Color> PrimaryColors
		{
			get => primaryColors;
			set => primaryColors.Colors = value?.Take(MaxPrimaryColorCount);
		}

		
		public void Awake() => UpdateBaseName();
		
		public void OnValidate()
		{
			primaryColors.ValidateMaxCount(MaxPrimaryColorCount);
			UpdateBaseName();
		}
		
		private void UpdateBaseName() => primaryColors.SetBaseName(PrimaryColorBaseName);

		protected override PostEffectPass GetPass() =>
			CreateSimplePass(ShaderNames.ConvexHullMap, SetMaterialProperties);

		private void SetMaterialProperties(Material material)
		{
			material.SetInteger(LevelCountID, levelCount);
			material.SetColor(BackgroundColorID, backgroundColor);

			material.SetColors(primaryColors);
			
			material.SetFloat(MaxOffsetID, maxOffset);
			material.SetInteger(DebugMaxLayerCountID, debugMaxLayerCount);
		}
	}
}
#endif
