#if GAMELOGIC_HAS_URP
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions.Internal;
using Gamelogic.Fx.Internal;
using Gamelogic.Fx.URP.PostProcessing;
using UnityEngine;

namespace Gamelogic.Fx.CrossHatching.URP.PostProcessing.Effects
{
	/// <summary>
	/// A post process that pushes each pixel’s color toward chosen primaries using convex-hull projection,
	/// but blends them with Mixbox for more natural, smoother color transitions than normal interpolation.
	/// See <see href="../common/docs/effects-reference-common.html#mixbox-convex-hull-map"/>.
	/// </summary>
	[RequiresMixbox]
	[HelpURL(Constants.HelpURLRoot + HelpURLs.MixboxConvexHullMap)]
	[Version(1, 0, 0)]
	public sealed class GLMixboxConvexHullMapRenderFeature : PostProcessRenderFeature
	{
		private const string PrimaryColorBaseName = "_PrimaryColor";
		private const int MaxPrimaryColorCount = 10;
		
		private static readonly int MixboxLUTID = Shader.PropertyToID("_MixboxLUT");
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int BackgroundColorID = Shader.PropertyToID("_BackgroundColor");

		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.MixboxConvexHullMap;

		[SerializeField] private Texture2D mixboxLUT;
		[Min(0)]
		[SerializeField] private int levelCount = 3;
		[SerializeField] private Color backgroundColor = Constants.DefaultBackgroundColor;
		[SerializeField] private ColorShaderPropertyList primaryColors = Constants.DefaultPrimaryColorsCopy;
		
		/// <summary>
		/// Gets or sets the number of refinement iterations used for Mixbox convex-hull projection.
		/// Higher values progressively move colors closer to the convex hull defined by the primary colors.
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
		/// for Mixbox convex-hull projection.
		/// </summary>
		public Color BackgroundColor
		{
			get => backgroundColor;
			set => backgroundColor = value;
		}

		/// <summary>
		/// Gets or sets the collection of primary colors that define the convex hull.
		/// Pixel colors are iteratively blended toward these palette anchors using Mixbox.
		/// </summary>
		public IEnumerable<Color> PrimaryColors
		{
			get => primaryColors;
			set => primaryColors.Colors = value?.Take(Constants.MaxPrimaryColors);
		}


		public void Awake() => UpdateBaseName();
		
		public void OnValidate()
		{
			primaryColors.ValidateMaxCount(MaxPrimaryColorCount);
			UpdateBaseName();
		}
		
		private void UpdateBaseName() => primaryColors.SetBaseName(PrimaryColorBaseName);
		
		protected override PostEffectPass GetPass() =>
			CreateSimplePass(ShaderNames.MixboxConvexHullMap, SetMaterialProperties);

		private void SetMaterialProperties(Material material)
		{
			material.SetTexture(MixboxLUTID, mixboxLUT);
			material.SetInteger(LevelCountID, levelCount);
			material.SetColor(BackgroundColorID, backgroundColor);
			material.SetColors(primaryColors);
		}
	}
}
#endif
