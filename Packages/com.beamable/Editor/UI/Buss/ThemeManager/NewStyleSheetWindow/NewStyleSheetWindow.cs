using Beamable.UI.Buss;
using System.Collections.Generic;
using UnityEngine;

namespace Beamable.Editor.UI.Buss
{
	public class NewStyleSheetWindow : WindowBase<NewStyleSheetWindow, NewStyleSheetVisualElement>
	{
		private List<BussStyleRule> _initialRules;

		public void Init(List<BussStyleRule> initialRules)
		{
			titleContent = new GUIContent("New Style Sheet Window");
			minSize = new Vector2(720, 200);
			maxSize = minSize;
			position = new Rect((Screen.width + minSize.x) * 0.5f, Screen.width * 0.5f, minSize.x, minSize.y);

			_initialRules = initialRules;

			Refresh();
		}

		protected override NewStyleSheetVisualElement GetVisualElement() => new NewStyleSheetVisualElement(_initialRules);
	}
}
