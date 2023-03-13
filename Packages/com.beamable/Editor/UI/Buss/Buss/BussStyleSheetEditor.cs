using Beamable.Editor.UI.Components;
using Beamable.UI.Buss;
#if BEAMABLE_DEVELOPER
using System;
#endif
using UnityEditor;
using UnityEngine.UIElements;

namespace Beamable.Editor.UI.Buss
{
	[CustomEditor(typeof(BussStyleSheet))]
	public class BussStyleSheetEditor : UnityEditor.Editor
	{
		private BussStyleListVisualElement _list;
		private BussStyleSheet _styleSheet;
		private LabeledIntegerField _sortingOrder;
		private ThemeInspectorModel _model;

		public override VisualElement CreateInspectorGUI()
		{
			_styleSheet = (BussStyleSheet)target;
			VisualElement root = new VisualElement();

			if (!_styleSheet.IsWritable)
			{
				return root;
			}

			_model = new ThemeInspectorModel(_styleSheet);
			_list = new BussStyleListVisualElement(_model);
			_list.Init();
			_list.Refresh();

			_styleSheet.Change += _list.Refresh;

#if BEAMABLE_DEVELOPER
			LabeledCheckboxVisualElement readonlyCheckbox = new LabeledCheckboxVisualElement("Readonly");
			readonlyCheckbox.OnValueChanged -= OnReadonlyValueChanged;
			readonlyCheckbox.OnValueChanged += OnReadonlyValueChanged;
			readonlyCheckbox.Refresh();
			readonlyCheckbox.SetWithoutNotify(_styleSheet.IsReadOnly);
			root.Add(readonlyCheckbox);

			_sortingOrder = new LabeledIntegerField();
			_sortingOrder.Setup("Sorting Order", _styleSheet.SortingOrder, OnSortingOrderChanged, 0, Int32.MaxValue);
			root.Add(_sortingOrder);
#endif

			if (!_styleSheet.IsReadOnly)
			{
				AddSelectorButton(root);
			}

			root.Add(_list);
			return root;
		}

#if BEAMABLE_DEVELOPER
		private void OnReadonlyValueChanged(bool value)
		{
			_styleSheet.SetReadonly(value);
		}

		private void OnSortingOrderChanged()
		{
			_styleSheet.SetSortingOrder(_sortingOrder.Value);
		}
#endif

		private void AddSelectorButton(VisualElement parent)
		{
			AddStyleButton button = new AddStyleButton();
			button.Setup(_model.OnAddStyleButtonClicked);
			parent.Add(button);
		}

		private void OnDestroy()
		{
			if (_list != null)
			{
				_list.Destroy();
				_list = null;
			}
		}
	}
}
