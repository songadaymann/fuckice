using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing.Effects
{
	/// <summary>
	/// Maps the image’s tones to three colors (low, mid, and high) based on lightness,
	/// smoothly blending between them using inverse linear interpolation.
	/// See <see href="../common/docs/effects-reference-common.html#tri-tone-map"/>.
	/// </summary>
	[HelpURL(Constants.HelpURLRoot + HelpURLs.TriToneMap)]
	public sealed class TriToneMapPostProcess : NamedShaderPostProcess
	{
		private static readonly int LowColorID = Shader.PropertyToID("_LowColor");
		private static readonly int MidColorID = Shader.PropertyToID("_MidColor");
		private static readonly int HighColorID = Shader.PropertyToID("_HighColor");
		private static readonly int LowValueID = Shader.PropertyToID("_LowValue");
		private static readonly int MidValueID = Shader.PropertyToID("_MidValue");
		private static readonly int HighValueID = Shader.PropertyToID("_HighValue");
		protected override string ShaderName => Constants.ShaderNameRoot + ShaderNames.TriToneMap;
	
		[Header("Colors")]
		[SerializeField] private Color lowColor = new Color(0, 0, 0.3f);
		[SerializeField] private Color midColor = new Color(0.7f, 0.3f, 0.5f);
		[SerializeField] private Color highColor = new Color(1f, 1f, 0.7f);
		
		[Header("Thresholds")]
		[Range(0f, 1f)]
		[SerializeField] private float lowValue = 0.2f;
	
		[Range(0f, 1f)]
		[SerializeField] private float midValue = 0.5f;
	
		[Range(0f, 1f)]
		[SerializeField] private float highValue = 0.8f;

		public Color LowColor
		{
			get => lowColor;
			set => lowColor = value;
		}

		public Color MidColor
		{
			get => midColor;
			set => midColor = value;
		}

		public Color HighColor
		{
			get => highColor;
			set => highColor = value;
		}

		public float LowValue
		{
			get => lowValue;
			set
			{
				value.ThrowIfOutOfRange(0f, 1f, nameof(value));
				lowValue = value;
			}
		}

		public float MidValue
		{
			get => midValue;
			set
			{
				value.ThrowIfOutOfRange(0f, 1f, nameof(value));
				midValue = value;
			}
		}

		public float HighValue
		{
			get => highValue;
			set
			{
				value.ThrowIfOutOfRange(0f, 1f, nameof(value));
				highValue = value;
			}
		}

		protected override void SetMaterialProperties(Material effectMaterial)
		{
			effectMaterial.SetColor(LowColorID, lowColor);
			effectMaterial.SetColor(MidColorID, midColor);
			effectMaterial.SetColor(HighColorID, highColor);
		
			effectMaterial.SetFloat(LowValueID, lowValue);
			effectMaterial.SetFloat(MidValueID, midValue);
			effectMaterial.SetFloat(HighValueID, highValue);
		}
	}
}
