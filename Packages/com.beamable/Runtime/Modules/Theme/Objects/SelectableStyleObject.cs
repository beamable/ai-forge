using System;
using UnityEngine.UI;

namespace Beamable.Theme.Objects
{
	[Serializable]
	public class SelectableStyleObject : StyleObject<Selectable>
	{
		public ButtonStyleData Data;

		protected override void Apply(Selectable target)
		{
			target.transition = Data.Transition;
			switch (target.transition)
			{
				case Selectable.Transition.ColorTint:
					target.colors = Data.Colors;
					break;
				case Selectable.Transition.SpriteSwap:
					target.spriteState = Data.SpriteState;
					break;
				case Selectable.Transition.Animation:
					target.animationTriggers = Data.AnimationTriggers;
					break;
			}

		}
	}

	[Serializable]
	public class ButtonStyleData
	{
		public Selectable.Transition Transition;
		public ColorBlock Colors = ColorBlock.defaultColorBlock;
		public SpriteState SpriteState;
		public AnimationTriggers AnimationTriggers;
	}
}
