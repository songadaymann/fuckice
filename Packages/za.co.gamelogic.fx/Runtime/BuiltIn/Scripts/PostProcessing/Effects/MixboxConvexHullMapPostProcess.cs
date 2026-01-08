using System.Collections.Generic;
using System.Linq;
using Gamelogic.Fx.Internal;
using Gamelogic.Extensions.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// A post process that pushes each pixel’s color toward chosen primaries using convex-hull projection,
	/// but blends them with Mixbox for more natural, smoother color transitions than normal interpolation.
	/// See <see href="../common/docs/effects-reference-common.html#mixbox-convex-hull-map"/>.
	/// </summary>
	[RequiresMixbox]
	[HelpURL(Constants.HelpURLRoot + HelpURLs.MixboxConvexHullMap)]
	[Version(1, 1, 0)]
	public sealed class MixboxConvexHullMapPostProcess : NamedShaderPostProcess
	{
		private static readonly int MixBoxLUTID = Shader.PropertyToID("_MixboxLUT");
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int BackgroundColorID = Shader.PropertyToID("_BackgroundColor");
		private const string PrimaryColorBaseName = "_PrimaryColor";


		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.MixboxConvexHullMap;

		[SerializeField] private Texture2D mixboxLUT = null;
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
			primaryColors.ValidateMaxCount(Constants.MaxPrimaryColors);
			UpdateBaseName();
		}

		private void UpdateBaseName() => primaryColors.SetBaseName(PrimaryColorBaseName);


		protected override void SetMaterialProperties(Material effectMaterial)
		{
#if UNITY_2021_2_OR_NEWER
			effectMaterial.SetInteger(LevelCountID, levelCount);
#else
			effectMaterial.SetInt(LevelCountID, levelCount);
#endif
			effectMaterial.SetTexture(MixBoxLUTID, mixboxLUT);
			effectMaterial.SetColor(BackgroundColorID, backgroundColor);
			effectMaterial.SetColors(primaryColors);
		}
	}
}
