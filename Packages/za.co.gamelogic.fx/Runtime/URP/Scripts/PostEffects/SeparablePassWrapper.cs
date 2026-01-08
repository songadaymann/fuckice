#if GAMELOGIC_HAS_URP
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Gamelogic.Fx.URP.PostProcessing
{
	/// <summary>
	/// Class used to wrap the two passes of a separable effect. 
	/// </summary>
	internal sealed class SeparablePassWrapper : PostEffectPass
	{
		private readonly Vector2 direction;
		private readonly int directionId = Shader.PropertyToID("_Direction");
		private readonly Action<Material> userProperties;

		public SeparablePassWrapper(
			string commandName,
			RenderPassEvent @event,
			Material material,
			Vector2 direction,
			
			Action<Material> userProperties
		) : base(material, @event)
		{
			CommandName = commandName;
			this.direction = direction;
			this.userProperties = userProperties;
		}

		protected override string CommandName { get; }

		protected override void SetMaterialProperties(Material material)
		{
			material.SetVector(directionId, direction);
			userProperties(material);
		}
	}
}
#endif
