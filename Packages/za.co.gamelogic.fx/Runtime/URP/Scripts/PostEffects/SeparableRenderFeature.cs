#if GAMELOGIC_HAS_URP
using Gamelogic.Fx.Internal;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gamelogic.Fx.URP.PostProcessing
{
	/// <summary>
	/// Base class for URP post-processing features that use a separable two-pass filter.
	/// </summary>
	/// <remarks>
	/// This feature automatically creates:
	/// <list type="bullet">
	/// <item><description>A horizontal pass with direction <c>(1, 0)</c></description></item>
	/// <item><description>A vertical pass with direction <c>(0, 1)</c></description></item>
	/// </list>
	/// 
	/// Both passes use the same shader, each with its own material instance.  
	/// The kernel parameters are provided via <see cref="kernelInfo"/> and are set on
	/// each material before execution.
	/// 
	/// Subclasses must:
	/// <list type="bullet">
	/// <item><description>Provide the shader name by overriding <see cref="ShaderName"/>.</description></item>
	/// <item><description>Optionally override <see cref="SetMaterialProperties"/> to set effect-specific
	/// shader properties.</description></item>
	/// </list>
	/// 
	/// This class handles:
	/// <list type="bullet">
	/// <item><description>Creating and configuring materials</description></item>
	/// <item><description>Creating the two directional passes</description></item>
	/// <item><description>Injecting both passes into the URP pipeline</description></item>
	/// </list>
	/// </remarks>
	public abstract class SeparableRenderFeature : ScriptableRendererFeature
	{
		[ReadOnly] [SerializeField] private Shader shader;
		
		/// <summary>
		/// Determines when the passes are inserted into the URP render pipeline.
		/// Defaults to <see cref="RenderPassEvent.AfterRenderingPostProcessing"/>.
		/// </summary>
		[SerializeField] private RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
	
		/// <summary>
		/// Kernel configuration shared by both passes.
		/// The offset is automatically centered because of the <see cref="CenterKernelAttribute"/>.
		/// </summary>
		[CenterKernel]
		[SerializeField] private KernelInfo kernelInfo;
		
		private Material horizontalMaterial;
		private Material verticalMaterial;

		private ScriptableRenderPass horizontalPass;
		private ScriptableRenderPass verticalPass;

		/// <summary>
		/// Must return the name of the shader used by both the horizontal and vertical passes.
		/// </summary>
		protected abstract string ShaderName { get; }
		
		/// <summary>
		/// Allows subclasses to set additional shader properties that vary per effect.
		/// Called just before each pass is executed.
		/// </summary>
		/// <param name="material">The material associated with the pass.</param>
		protected virtual void SetMaterialProperties(Material material){}

		/// <summary>
		/// Creates the materials and the two directional render passes.
		/// </summary>
		public override void Create()
		{
			shader = Shader.Find(ShaderName);
			if(shader == null)
			{
				Debug.LogError($"Shader not found: {ShaderName}");
				return;
			}

			horizontalMaterial = new Material(shader);
			verticalMaterial = new Material(shader);

			horizontalPass = new SeparablePassWrapper(
				"Separable Horizontal",
				injectionPoint,
				horizontalMaterial,
				new Vector2(1, 0),
				SetKernelAndOtherProperties
			);

			verticalPass = new SeparablePassWrapper(
				"Separable Vertical",
				injectionPoint,
				verticalMaterial,
				new Vector2(0, 1),
				SetKernelAndOtherProperties
			);
		}
		
		/// <summary>
		/// Enqueues both the horizontal and vertical passes into the renderer.
		/// </summary>
		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
		{
			((PostEffectPass)horizontalPass).SetRenderer(renderer);
			renderer.EnqueuePass(horizontalPass);

			((PostEffectPass)verticalPass).SetRenderer(renderer);
			renderer.EnqueuePass(verticalPass);
		}
		
		/// <summary>
		/// Assigns kernel parameters and effect-specific shader properties to the given material.
		/// This is executed separately for the horizontal and vertical passes.
		/// </summary>
		private void SetKernelAndOtherProperties(Material material)
		{
			material.SetKernel(kernelInfo);
			SetMaterialProperties(material);
		}
	}
}
#endif
