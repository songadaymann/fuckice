using System.Collections.Generic;
using System.Linq;
using Gamelogic.Fx.Internal;
using Gamelogic.Extensions.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/* This class is the model for how to implement a sequence of properties (in this case colors).
	*/
	
	/// <summary>
	/// A post process that gradually shifts colors toward the provided primaries,
	/// producing posterized, clustered, or palette–constrained looks.
	/// See <see href="../common/docs/effects-reference-common.html#convex-hull-map"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.ConvexHullMap)]
	[Version(1, 1, 0)]
	public sealed class ConvexHullMapPostProcess : NamedShaderPostProcess
	{
		private static readonly int LevelCountID = Shader.PropertyToID("_LevelCount");
		private static readonly int BackgroundColorID = Shader.PropertyToID("_BackgroundColor");
		private const string PrimaryColorBaseName = "_PrimaryColor";
		private const int MaxPrimaryColorCount = 10;
		
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.ConvexHullMap;

		[Min(1)]
		[SerializeField] private int levelCount = 3;
		[SerializeField] private Color backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
		[SerializeField] private ColorShaderPropertyList primaryColors = Constants.DefaultPrimaryColorsCopy;

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
		
		private void UpdateBaseName() => primaryColors.SetBaseName(PrimaryColorBaseName);

		protected override void SetMaterialProperties(Material effectMaterial)
		{
#if UNITY_2021_2_OR_NEWER
			effectMaterial.SetInteger(LevelCountID, levelCount);
#else
			effectMaterial.SetInt(LevelCountID, levelCount);
#endif
			effectMaterial.SetColor(BackgroundColorID, backgroundColor);
			effectMaterial.SetColors(primaryColors);
		}

		public void OnValidate()
		{
			primaryColors.ValidateMaxCount(MaxPrimaryColorCount);
			UpdateBaseName();
		}
	}
}
