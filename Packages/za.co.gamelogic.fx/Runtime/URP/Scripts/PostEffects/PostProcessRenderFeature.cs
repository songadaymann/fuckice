#if GAMELOGIC_HAS_URP

using System;
using Gamelogic.Extensions;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gamelogic.Fx.URP.PostProcessing
{
	/// <summary>
	/// Handles texture and material creations for simple effects that can be implemented with one shader and one pass. 
	/// </summary>
	public abstract class PostProcessRenderFeature : ScriptableRendererFeature
	{
		private sealed class SimplePassImpl : PostEffectPass
		{
			private readonly Action<Material> setMaterialProperties;

			public SimplePassImpl(string name, RenderPassEvent e, Material m, Action<Material> setMaterialProperties)
				: base(m, e)
			{
				CommandName = name; 
				this.setMaterialProperties = setMaterialProperties;
			}

			protected override string CommandName { get; }

			protected override void SetMaterialProperties(Material material) => setMaterialProperties(material);
		}
		
		[SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;

		[ReadOnly] [SerializeField] private Shader shader;
		
		private Material material;
		private PostEffectPass pass;

		/// <summary>
		/// Gets the name of the shader that implements this feature. 
		/// </summary>
		/// <remarks>
		/// Implementers: override this to specify the shader to use for the implementation of the effect. 
		/// </remarks>
		protected abstract string ShaderName { get; }
		
		public void OnEnable()
		{
			shader = Shader.Find(ShaderName);
		}

		/// <inheritdoc/>
		public override void Create()
		{
			if(shader == null)
			{
				Debug.LogError($"Shader not found: {ShaderName}. PostProcessRenderFeature {GetType().Name} will not be created.");
				return;
			}
			
			material = new Material(shader);
			pass = GetPass();
		}
		
		/// <inheritdoc/>
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			pass.SetRenderer(renderer);
			renderer.EnqueuePass(pass);
		}
		
		/// <summary>
		/// Creates a pass from a name, and a function that sets the material properties.  
		/// </summary>
		/// <param name="passName">The name of the pass, used for command buffer naming.</param>
		/// <param name="setMaterialProperties">A function that sets the material properties for the effect.</param>
		/// <remarks>
		/// You can use this function to implement <see cref="GetPass"/>
		/// </remarks>
		protected PostEffectPass CreateSimplePass(string passName, Action<Material> setMaterialProperties) 
			=> new SimplePassImpl(passName, injectionPoint, material, setMaterialProperties);

		/// <summary>
		/// Gets the pass used to implement this effect.
		/// </summary>
		/// <remarks>
		/// This assumes effects are single pass.
		///
		/// Implementors: return the pass that implements the effect you are creating. 
		/// For simple passes (that only need to set material properties), you can use <see cref="CreateSimplePass"/> to
		/// create the pass.
		/// </remarks>
		protected abstract PostEffectPass GetPass();
	}
}
#endif
