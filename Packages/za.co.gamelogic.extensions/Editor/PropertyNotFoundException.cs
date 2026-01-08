using System;
using UnityEditor;
using Gamelogic.Extensions.Internal;

namespace Gamelogic.Extensions.Editor
{
	/// <summary>
	/// Thrown when looking for a property (for example, in a serialized object) and the property is not found.
	/// </summary>
	[Version(4, 5, 0)]
	public sealed class PropertyNotFoundException : Exception
	{
		public PropertyNotFoundException(string propertyName, SerializedObject property) 
			: base($"Property {propertyName} not found in {property}.")
		{}
	}
}