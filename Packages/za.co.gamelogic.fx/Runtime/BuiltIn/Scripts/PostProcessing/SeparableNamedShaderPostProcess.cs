using Gamelogic.Extensions;
using Gamelogic.Extensions.Internal;
using Gamelogic.Fx.Internal;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing
{
	public enum Direction
	{
		Horizontal = 0,
		Vertical = 1
	}
	
	/// <summary>
	/// Encapsulates a post process based on a separable filter, such as a box blur. 
	/// </summary>
	[Version(1, 1, 0)]
	[ExecuteInEditMode]
	public abstract class SeparableNamedShaderPostProcess : GLMonoBehaviour, IPostProcess
	{
		private static readonly Vector2 Horizontal = new Vector2(1, 0);
		private static readonly Vector2 Vertical = new Vector2(0, 1);
		private static readonly int DirectionID = Shader.PropertyToID("_Direction");

		[ReadOnly]
		[SerializeField] private Shader shader;
		
		[CenterKernel]
		[SerializeField] private KernelInfo kernel 
			= new KernelInfo { offset = -1, size = 3, jumpSize = 1 };
		
		[SerializeField] private bool setEachFrame = true;
		
		private Material screenMaterial;

		/* Why internal? So property drawers can do checks, similar to EffectMaterial in PostProcess.
		*/
		internal Material EffectMaterial
		{
			get
			{
				if (shader == null)
				{
					Debug.LogError("No shader set for post process " + name);
					return null;
				}

				if (screenMaterial == null)
				{
					screenMaterial = new Material(shader)
					{
						hideFlags = HideFlags.DontSave
					};
				}

				return screenMaterial;
			}
		}
		
		protected abstract string ShaderName { get; }
		
		public void Start()
		{
			if (shader == null || !shader.isSupported)
			{
				enabled = false;
			}
		}

		public void OnEnable() => ReloadShader();
		
		public void OnDisable()
		{
			if (screenMaterial != null)
			{
				DestroyImmediate(screenMaterial);
			}
		}
		
		/// <inheritdoc />
		public virtual void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
		{
			if (shader == null)
			{
				Graphics.Blit(sourceTexture, destTexture);
				return;
			}

			var renderTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, sourceTexture.format);

			Pass(Direction.Horizontal, sourceTexture, renderTexture);
			Pass(Direction.Vertical, renderTexture, destTexture);
			
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		
		/// <summary>
		/// Sets the 
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="effectMaterial"></param>
		/// <remarks>
		/// Implementors should override this method to set the shader's specific properties.
		///
		/// The method is called twice, once for each direction.
		/// The direction vector is automatically set to <c>(1, 0)</c> for the first pass and <c>(0, 1)</c> for the second
		/// pass, and so are the kernel properties: <c>KernelSize</c>, <c>KernelOffset</c>, <c>JumpSize</c>.  
		/// </remarks>
		protected virtual void SetMaterialProperties(Direction direction, Material effectMaterial)
		{
		}
		
		/// <summary>
		/// Gets the shader by name. 
		/// </summary>
		/// <remarks>
		/// This is helpful while developing shaders, to get a new copy of it. 
		/// </remarks>
		[InspectorButton]
		private void ReloadShader() => shader = PostProcess.GetShader(ShaderName);
		
		private void Pass(Direction direction, RenderTexture source, RenderTexture destination)
		{
			EffectMaterial.SetVector(DirectionID, direction == Direction.Horizontal ? Horizontal : Vertical);

			if (setEachFrame)
			{
				EffectMaterial.SetKernel(kernel);
				SetMaterialProperties(direction, EffectMaterial);
			}
			
			Graphics.Blit(source, destination, EffectMaterial);
		}
	}
}
