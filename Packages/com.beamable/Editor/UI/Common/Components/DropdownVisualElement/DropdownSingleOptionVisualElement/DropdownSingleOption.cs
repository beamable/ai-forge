using System;

namespace Beamable.Editor.UI.Components
{
	public class DropdownSingleOption
	{
		public int Id { get; }
		public string Label { get; }
		public Action<string> OnClick { get; }
		public bool LineBelow { get; set; }

		public DropdownSingleOption(int id, string label, Action<string> onClick)
		{
			Id = id;
			Label = label;
			OnClick = onClick;
		}
	}
}
