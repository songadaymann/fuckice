using Gamelogic.Extensions;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing
{
	/// <summary>
	/// Contains basic functionality for a full screen post process effect.
	/// </summary>
	/// <remarks>
	/// Intended for the built-in render pipeline.
	/// </remarks>
	[ExecuteInEditMode]
	public abstract class PostProcess : GLMonoBehaviour, IPostProcess
	{
		[Tooltip("Whether the properties should be set each frame.")]
		[SerializeField] private bool setEachFrame = true;
		
		private bool hasBeenSet = false;
		private Material screenMaterial;
		
		/// <summary>
		/// The material used to apply the post process effect.
		/// </summary>
		internal Material EffectMaterial
		{
			get
			{
				if (Shader == null)
				{
					Debug.LogError("No shader set for post process " + name);
					return null;
				}

				if (screenMaterial == null)
				{
					screenMaterial = new Material(Shader)
					{
						hideFlags = HideFlags.DontSave
					};
				}

				return screenMaterial;
			}
		}

		/// <summary>
		/// The shader used for this post process effect.
		/// </summary>
		protected abstract Shader Shader { get; }

		public void Start()
		{
			if (Shader == null || !Shader.isSupported)
			{
				enabled = false;
			}
		}
		
		public virtual void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
		{
			if (Shader != null)
			{
				if (setEachFrame || !hasBeenSet)
				{
					SetMaterialProperties(EffectMaterial);
					hasBeenSet = true;
				}

				Graphics.Blit(sourceTexture, destTexture, EffectMaterial);
			}
			else
			{
				Graphics.Blit(sourceTexture, destTexture);
			}
		}

		public void OnDisable()
		{
			if (screenMaterial != null)
			{
				DestroyImmediate(screenMaterial);
			}
		}

		/// <summary>
		/// Applies all the properties defined for this post process in the inspector to the <see cref="EffectMaterial"/>. 
		/// </summary>
		protected virtual void SetMaterialProperties(Material effectMaterial)
		{
		}
		
		/// <summary>
		/// Gets a shader by name, logging an error if it cannot be found.
		/// </summary>
		/// <param name="name">The name of the shader.</param>
		/// <returns>The shader, or null if it could not be found.</returns>
		internal static Shader GetShader(string name)
		{
			var shader = Shader.Find(name);
		
			if (shader == null)
			{
				Debug.LogError($"Could not find shader {name}. Make sure it is included in the build.");
			}

			return shader;
		}
		
	}
}
