using Beamable.UI.Buss;
using System.Collections.Generic;

namespace Beamable.Editor.UI.Buss
{
	public class ThemeInspectorModel : ThemeModel
	{
		public override BussElement SelectedElement { get; set; }
		protected sealed override List<BussStyleSheet> SceneStyleSheets { get; } = new List<BussStyleSheet>();
		public override Dictionary<BussStyleRule, BussStyleSheet> FilteredRules => Filter.GetFiltered(SceneStyleSheets[0]);
		public override List<BussStyleSheet> WritableStyleSheets => SceneStyleSheets;

		public ThemeInspectorModel(BussStyleSheet styleSheet)
		{
			Filter = new BussCardFilter();
			SceneStyleSheets.Add(styleSheet);
		}
	}
}
