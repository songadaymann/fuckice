using System.Linq;
using Gamelogic.Extensions;
using Gamelogic.Fx.BuiltIn.PostProcessing.Effects;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing
{
	/// <summary>
	/// Runs all post-processes found in children of a given root on the <see cref="Camera"/> this script is
	/// attached to.
	/// </summary>
	/// <remarks>
	/// Intended for the built-in render pipeline.
	/// 
	/// This script should be placed on a <see cref="Camera"/>. It calls <see cref="GenericPostProcess.OnRenderImage"/>
	/// on all children with <see cref="GenericPostProcess"/> of the <c>postProcessRoot</c> configured in this inspector.
	/// This allows you to have post-processes in separate objects and make it easier to manage if you are experimenting
	/// with many processes at the same time.
	///
	/// So to add a post process to your camera:
	///	1. Add this script to the camera.
	///	2. Add an empty GameObject in your scene and call it PostEffects (or similar).
	///	3. Add a child object for each post process you want to add (e.g. <see cref="AdjustGammaPostProcess"/>), and add the
	///		post process script to it. Any script that implements <see cref="IPostProcess"/> will work.
	/// </remarks>
	[ExecuteInEditMode]
	public sealed class PostProcessRunner : GLMonoBehaviour
	{
		[ValidateNotNull]
		[SerializeField] private Transform postProcessRoot = null;

#if UNITY_EDITOR // Only for debugging
		// Not serialized in older versions of Unity. 
		// ReSharper disable once Unity.RedundantSerializeFieldAttribute  
		[SerializeField, ReadOnly]
#endif
		private IPostProcess[] postProcesses;
		
		public void OnRenderImage(RenderTexture sourceTexture, RenderTexture destinationTexture)
		{
			if (postProcessRoot == null)
			{
				Graphics.Blit(sourceTexture, destinationTexture);
				return;
			}

			postProcesses = postProcessRoot
				.GetComponentsInChildren<MonoBehaviour>(includeInactive: false)
				.Where(component => component.enabled)
				.Where(component => component is IPostProcess)
				.Cast<IPostProcess>()
				.ToArray();

			if (!postProcesses.Any())
			{
				Graphics.Blit(sourceTexture, destinationTexture);
				return;
			}

			var currentSource = sourceTexture;

			foreach (var postProcess in postProcesses)
			{
				var temporaryTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height);
				postProcess.OnRenderImage(currentSource, temporaryTexture);

				// Release the current source if it was a temporary texture
				if (currentSource != sourceTexture)
				{
					RenderTexture.ReleaseTemporary(currentSource);
				}

				// Swap textures
				currentSource = temporaryTexture;
			}

			// Blit the final texture to the destination
			Graphics.Blit(currentSource, destinationTexture);

			// Release the last temporary texture
			if (currentSource != sourceTexture)
			{
				RenderTexture.ReleaseTemporary(currentSource);
			}
		}
	}
}
