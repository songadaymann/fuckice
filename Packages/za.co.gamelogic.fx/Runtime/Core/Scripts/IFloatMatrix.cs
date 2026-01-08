using System.Collections.Generic;
using Unity.Mathematics;

namespace Gamelogic.Fx
{
	public interface IFloatMatrix
	{
		void SetFrom(IFloatMatrix other);
		int Width { get;  }
		int Height { get; }
		int Length { get; }
		IEnumerable<float> Values { get; }
		bool IsValid();
		float this[int x, int y] { get; }
		float this[int2 index] { get; }
	}
}