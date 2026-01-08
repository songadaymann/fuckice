using UnityEngine;
using UnityEngine.Rendering;

#if GAMELOGIC_HAS_URP
using UnityEngine.Rendering.Universal;
#endif

namespace Gamelogic.Fx.URP.Internal
{
	internal static class PipelineDefiner
	{
		private const string HasUrpKeyword = "GAMELOGIC_HAS_URP";
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		internal static void Init()
		{
#if GAMELOGIC_HAS_URP
			if (UsingURP())
			{
				EnableURPSupport();
			}
			else
			{
				DisableURPSupport();
			}
#else
				DisableURPSupport();
#endif
		}

		private static void EnableURPSupport()
		{
			Shader.EnableKeyword(HasUrpKeyword);
			Debug.Log("URP package found. Enabled global shader keyword: " + HasUrpKeyword);
		}

		private static void DisableURPSupport()
		{
			Shader.DisableKeyword(HasUrpKeyword);
			Debug.Log("URP package not found or pipeline asset is not assigned. Disabled global shader keyword: " + HasUrpKeyword);
		}

#if GAMELOGIC_HAS_URP
		private static bool UsingURP() => GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset;
#endif
	}
}
