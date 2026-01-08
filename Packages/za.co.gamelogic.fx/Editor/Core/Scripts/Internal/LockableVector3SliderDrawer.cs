using Gamelogic.Fx.Internal;
using UnityEditor;

namespace Gamelogic.Fx.Editor.Internal
{
	[CustomPropertyDrawer(typeof(LockableVector3))]
	internal sealed class LockableVector3SliderDrawer : LockableVectorSliderDrawerBase
	{
		protected override int ComponentCount => 3;
	}
}
