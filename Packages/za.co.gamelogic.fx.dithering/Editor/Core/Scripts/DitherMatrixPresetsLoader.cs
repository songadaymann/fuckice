using Gamelogic.Extensions;
using UnityEditor;

namespace Gamelogic.Fx.Dithering.Editor
{
	[InitializeOnLoad]
	public sealed class DitherMatrixPresetsLoader
	{
		static DitherMatrixPresetsLoader()
		{
			PropertyDrawerData.RegisterValuesRetriever(
				nameof(DitherMatrixPresets),
				() => DitherMatrixPresets.All
			);
		}
	}
}
