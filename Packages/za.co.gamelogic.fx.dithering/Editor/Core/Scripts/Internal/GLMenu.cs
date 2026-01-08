using UnityEditor;
using UnityEngine;

namespace Gamelogic.Fx.Dithering.Editor.Internal
{
	internal static class GLMenu
	{
		[MenuItem("Help/Gamelogic/Fx.Dithering/Documentation")]
		public static void OpenFxDitheringAPI() => Application.OpenURL("https://www.gamelogic.co.za/documentation/fx/dithering/");
	}
}
