using Gamelogic.Fx.Internal;
using UnityEditor;

namespace Gamelogic.Fx.Editor.Internal
{
	[CustomPropertyDrawer(typeof(LockableVector2))]
	internal sealed class LockableVector2SliderDrawer : LockableVectorSliderDrawerBase
	{
		protected override int ComponentCount => 2;
	}
}
