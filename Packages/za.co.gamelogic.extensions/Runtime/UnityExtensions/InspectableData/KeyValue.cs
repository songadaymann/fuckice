using System;
using System.Collections.Generic;
using Gamelogic.Extensions.Internal;

namespace Gamelogic.Extensions
{
	/// <summary>
	/// Represent a key-value pair used for the implementation of <see cref="FixedKeyDictionary{TKey,TValue}"/>.
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	/// <typeparam name="TValue">The type of the value.</typeparam>
	/// <remarks>
	/// We would almost be able to get away with standard <see cref="KeyValuePair{TKey,TValue}"/> but that is
	/// not serializable by Unity.
	/// </remarks>
	[Version(4, 5, 0)]
	[Serializable]
	public class KeyValue<TKey, TValue> //TODO: better name
	{
		public TKey key;
		public TValue value;

		public KeyValue(TKey key, TValue value)
		{
			this.key = key;
			this.value = value;
		}
		
		public void Deconstruct(out TKey outKey, out TValue outValue)
		{
			outKey = key;
			outValue = value;
		}
	}
}
