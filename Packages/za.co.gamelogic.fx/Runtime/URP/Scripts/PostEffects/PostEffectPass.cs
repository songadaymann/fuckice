#if GAMELOGIC_HAS_URP

using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gamelogic.Fx.URP.PostProcessing
{
	/// <summary>
	/// Implements a basic pass that for <see cref="PostProcessRenderFeature"/>.
	/// </summary>
	/// <remarks>
	/// Although this class is easy to extend, in many cases <see cref="PostProcessRenderFeature.CreateSimplePass"/>
	/// will be sufficient.
	/// </remarks>
	public abstract class PostEffectPass : ScriptableRenderPass
	{
		private readonly Material material;
		private readonly CommandBuffer commandBuffer = new CommandBuffer();
		private RTHandle temporaryRT;

		private ScriptableRenderer renderer;

		/// <summary>
		/// Initializes a new instance of the <see cref="PostEffectPass"/> class.
		/// </summary>
		/// <param name="material">The material to use for the effect.</param>
		/// <param name="injectionPoint">The render pass event to inject the effect at.</param>
		protected PostEffectPass(Material material, RenderPassEvent injectionPoint)
		{
			this.material = material;
			renderPassEvent = injectionPoint;
		}

	
		/// <summary>
		/// The renderer that will render this pass. 
		/// </summary>
		/// <param name="newRenderer"></param>
		/// <remarks>
		/// <see cref="PostProcessRenderFeature"/> uses this to pass the renderer, so that we can get the target texture
		/// etc. here. 
		/// </remarks>
		internal void SetRenderer(ScriptableRenderer newRenderer) => renderer = newRenderer;

		/// <inheritdoc/>
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (material == null)
			{
				return;
			}

			commandBuffer.name = CommandName;

			SetMaterialProperties(material);

			// camera buffer info
			var desc = renderingData.cameraData.cameraTargetDescriptor;
			desc.depthBufferBits = 0;

			GraphicsFormat gfxFormat = GraphicsFormatUtility.GetGraphicsFormat(
				desc.colorFormat,
				renderingData.cameraData.isHdrEnabled
					? RenderTextureReadWrite.Linear
					: RenderTextureReadWrite.sRGB
			);

			// Allocate RTHandle only if needed
			if (temporaryRT == null 
				|| temporaryRT.rt.width != desc.width 
				|| temporaryRT.rt.height != desc.height 
				|| temporaryRT.rt.graphicsFormat != gfxFormat)
			{
				temporaryRT?.Release();

				temporaryRT = RTHandles.Alloc(
					desc.width,
					desc.height,
					slices: 1,
					depthBufferBits: 0,
					colorFormat: gfxFormat,
					filterMode: FilterMode.Bilinear,
					wrapMode: TextureWrapMode.Clamp,
					name: "_TempTex"
				);
			}

			// Unity 2021 camera color is NOT an RTHandle.
			// Therefore, Blitter cannot be used; only CommandBuffer.Blit.

			// source → temp
			#if UNITY_2022_1_OR_NEWER
						var targetId = renderer.cameraColorTargetHandle;
			#else
						var targetId = renderer.cameraColorTarget;
			#endif
			
			commandBuffer.Blit(targetId, temporaryRT, material);

			// temp → source
			commandBuffer.Blit(temporaryRT, targetId);

			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
		}

		/// <summary>
		/// The name of the command buffer used for this pass.
		/// </summary>
		protected abstract string CommandName { get; }
		
		/// <summary>
		/// This sets properties on the given material.
		/// </summary>
		/// <param name="material">The material to set properties on.</param>
		/// <remarks>
		/// Implementors: override this to set the properties needed for your effect.
		/// </remarks>
		protected abstract void SetMaterialProperties(Material material);
	}
}
#endif
