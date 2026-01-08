using Gamelogic.Extensions;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing
{
	/// <summary>
	/// Allows you to use an arbitrary shader as a full screen post process effect and configure its properties in the
	/// inspector.
	/// </summary>
	/// <remarks>
	/// This class is useful for experimenting while developing post process shaders, since you need not add a new
	/// component for each new shader, or modify C# code if you change shader properties.  
	/// 
	/// You need to add the properties the shader expects manually in the inspector, or they will have their default
	/// values. 
	///
	/// There is little error checking on properties, and the shader will not work if the type of the property is
	/// set incorrectly.
	///
	/// Changing the type of a property can lead to problems, so when creating a new property change the type before
	/// changing the name.
	///
	/// You can add this script directly on a camera, or use the <see cref="PostProcessRunner"/> to run it from another
	/// place.
	///
	/// </remarks>
	public sealed class GenericPostProcess : PostProcess 
	{
		[SerializeField] private Shader shader = null;
		[SerializeField] private ShaderProperty[] shaderProperties = null;
		
		/// <inheritancedoc />
		protected override Shader Shader => shader;

		/// <inheritancedoc />
		protected override void SetMaterialProperties(Material effectMaterial) => EffectMaterial.Set(shaderProperties);

		[InspectorButton]
		private void SetMaterialPropertiesButton() => SetMaterialProperties(EffectMaterial);
	}
}
