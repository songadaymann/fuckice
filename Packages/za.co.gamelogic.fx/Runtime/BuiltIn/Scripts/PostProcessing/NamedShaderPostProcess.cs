using Gamelogic.Extensions;
using UnityEngine;

namespace Gamelogic.Fx.BuiltIn.PostProcessing
{
	
	/// <summary>
	/// A post process shader that gets the shader to use from the name. 
	/// </summary>
	[ExecuteInEditMode]
	public abstract class NamedShaderPostProcess : PostProcess
	{
		[ReadOnly] 
		[SerializeField] private Shader shader;
		
		/// <inheritancedoc />
		protected override Shader Shader => shader;
		
		/// <summary>
		/// The name of the shader to use for this post process.
		/// </summary>
		/// <remarks>
		/// Implementors: The shader name should be the same as used in the shader's "Shader" declaration, for example, to use the shader
		/// below, the shader name would be "Gamelogic/Fx/YourShaderName".
		///
		/// <![CDATA[
		/// Shader "Gamelogic/Fx/YourShaderName"
		/// {
		///	...
		/// }
		/// ]]>
		/// </remarks>
		protected abstract string ShaderName { get; }

		public void OnEnable() => ReloadShader();

		/// <summary>
		/// Gets the shader by name. 
		/// </summary>
		/// <remarks>
		/// This is helpful while developing shaders, to get a new copy of it. 
		/// </remarks>
		[InspectorButton]
		public void ReloadShader() => shader = GetShader(ShaderName);
	}
}
