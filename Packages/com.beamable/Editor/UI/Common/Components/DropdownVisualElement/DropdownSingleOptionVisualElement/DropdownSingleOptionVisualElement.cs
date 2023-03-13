using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class DropdownSingleOptionVisualElement : BeamableVisualElement
	{
		private Label _label;
		private string _labelText;
		private float _width;
		private float _height;
		private Action<string> _onClick;
		private bool _lineBelow;

		public float Height => _height;
		public bool LineBelow => _lineBelow;

		public new class UxmlFactory : UxmlFactory<DropdownSingleOptionVisualElement, UxmlTraits>
		{
		}

		public DropdownSingleOptionVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(DropdownVisualElement)}/{nameof(DropdownSingleOptionVisualElement)}/{nameof(DropdownSingleOptionVisualElement)}")
		{
			_labelText = string.Empty;
		}

		public override void Refresh()
		{
			base.Refresh();
			_label = Root.Q<Label>("value");
			_label.style.SetHeight(_height);
			_label.style.SetWidth(_width);
			_label.text = _labelText;

			_label.RegisterCallback<MouseDownEvent>(Clicked);
		}

		private void Clicked(MouseDownEvent evt)
		{
			evt.StopPropagation();
			_onClick?.Invoke(_labelText);
		}

		public DropdownSingleOptionVisualElement Setup(string label, Action<string> onClick, float width, float height, bool lineBelow = false)
		{
			_lineBelow = lineBelow;
			_labelText = label;
			_height = height;
			_width = width;

			_onClick = onClick;
			return this;
		}
	}
}
