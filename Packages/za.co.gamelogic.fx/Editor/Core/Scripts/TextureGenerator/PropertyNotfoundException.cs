using System;
using UnityEditor;

namespace Gamelogic.Fx.EditorTextureGenerator
{
	/// <summary>
	/// Thrown when looking for a property (for example, in a serialized object) and the property is not found.
	/// </summary>
	// Reuse candidate
	public sealed class PropertyNotfoundException : Exception
	{
		public PropertyNotfoundException(string propertyName, SerializedObject property) 
			: base($"Property {propertyName} not found in {property}.")
		{}
	}
}
