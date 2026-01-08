using Gamelogic.Fx.URP.Internal;
using UnityEditor;

namespace Gamelogic.Fx.Editor.URP.Internal
{
	[InitializeOnLoad]
	internal static class PipelineDefinerEditor
	{
		static PipelineDefinerEditor() => PipelineDefiner.Init();
	}
}
