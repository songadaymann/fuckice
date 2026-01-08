using UnityEngine;

namespace Gamelogic.Fx
{
	internal static class MixboxFeatures
	{
		private const string HasMixboxKeyword = "GAMELOGIC_HAS_MIXBOX";

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
		internal static void Init()
		{
			#if GAMELOGIC_HAS_MIXBOX
				Shader.EnableKeyword(HasMixboxKeyword);
				Debug.Log("Mixbox package found. Enabled global shader keyword: " + HasMixboxKeyword);
			#else
				Shader.DisableKeyword(HasMixboxKeyword);
				Debug.Log("Mixbox package not found. Disabled global shader keyword: " + HasMixboxKeyword);
			#endif
		}
		
		internal static bool IsMixboxPresent()
		{
#if GAMELOGIC_HAS_MIXBOX
			return true;
#else
			return false;
#endif
		}
	}
}
