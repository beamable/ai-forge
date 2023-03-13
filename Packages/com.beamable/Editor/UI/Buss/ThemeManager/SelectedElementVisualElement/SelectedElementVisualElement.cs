using Beamable.Editor.UI.Buss;
using Beamable.Editor.UI.Common;
using Beamable.UI.Buss;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Components
{
	public class SelectedElementVisualElement : BeamableBasicVisualElement
	{
		private const float MIN_CONTENT_HEIGHT = 120.0f;
		private const float SINGLE_CLASS_ENTRY_HEIGHT = 24.0f;

		private VisualElement _contentContainer;
		private LabeledTextField _idField;
		private LabeledObjectField _currentStyleSheet;
		private ListView _classesList;

		private int? _selectedClassListIndex;
		private readonly ThemeManagerModel _model;

		// this structure is for holding the registered callbacks in the virtual class list.
		private Dictionary<TextField, EventCallback<ChangeEvent<string>>> _changeHandlers =
			new Dictionary<TextField, EventCallback<ChangeEvent<string>>>();

		public SelectedElementVisualElement(ThemeManagerModel model) : base(
			$"{BUSS_THEME_MANAGER_PATH}/{nameof(SelectedElementVisualElement)}/{nameof(SelectedElementVisualElement)}.uss")
		{
			_model = model;
		}

		public override void Init()
		{
			base.Init();

			VisualElement header = new VisualElement();
			header.AddToClassList("header");

			Image foldIcon = new Image { name = "foldIcon" };
			foldIcon.AddToClassList("unfolded");
			header.Add(foldIcon);

			TextElement label = new TextElement();
			label.AddToClassList("headerLabel");
			label.text = "Selected Element";
			header.Add(label);

			header.RegisterCallback<MouseDownEvent>(evt =>
			{
				_contentContainer.ToggleInClassList("hidden");
				foldIcon.ToggleInClassList("unfolded");
				foldIcon.ToggleInClassList("folded");
				RefreshHeight();
			});

			Root.Add(header);

			_contentContainer = new VisualElement();

			_idField = new LabeledTextField();
			_idField.Setup("Id", _model.SelectedElementId, _model.OnIdChanged, true);
			_idField.Refresh();
			_contentContainer.Add(_idField);

			Label classesLabel = new Label("Classes");
			classesLabel.AddToClassList("classesLabel");
			_contentContainer.Add(classesLabel);

			_classesList = CreateClassesList();
			_classesList.RefreshPolyfill();
			_contentContainer.Add(_classesList);

			CreateButtons();

			_currentStyleSheet = new LabeledObjectField();
			_currentStyleSheet.Setup("Style sheet", typeof(BussStyleSheet), _model.SelectedElementStyleSheet,
									 _model.OnStyleSheetSelected);
			_contentContainer.Add(_currentStyleSheet);
			Root.Add(_contentContainer);

			_model.Change += Refresh;
		}

		public override void Refresh()
		{
			_idField.SetWithoutNotify(_model.SelectedElementId);
			_currentStyleSheet.SetValue(_model.SelectedElementStyleSheet);

			_selectedClassListIndex = null;

			RefreshClassesList();
			RefreshHeight();
		}

		private void CreateButtons()
		{
			VisualElement buttonsContainer = new VisualElement { name = "buttonsContainer" };

			VisualElement removeButton = new VisualElement { name = "removeButton" };
			removeButton.AddToClassList("button");
			removeButton.RegisterCallback<MouseDownEvent>(RemoveClassButtonClicked);
			buttonsContainer.Add(removeButton);

			VisualElement addButton = new VisualElement { name = "addButton" };
			addButton.AddToClassList("button");
			addButton.RegisterCallback<MouseDownEvent>(AddClassButtonClicked);
			buttonsContainer.Add(addButton);

			_contentContainer.Add(buttonsContainer);
		}

		private void AddClassButtonClicked(MouseDownEvent evt)
		{
			if (_model.SelectedElement == null)
			{
				return;
			}

			Undo.RecordObject(_model.SelectedElement, "Add Class");
			_model.SelectedElement.AddClass("");
			RefreshClassesList();
			RefreshHeight();

			EditorUtility.SetDirty(_model.SelectedElement);
			_model.ForceRefresh();
		}

		private void RemoveClassButtonClicked(MouseDownEvent evt)
		{
			if (_selectedClassListIndex == null || _model.SelectedElement == null)
			{
				return;
			}

			string className = (string)_classesList.itemsSource[(int)_selectedClassListIndex];

			if (className.StartsWith("."))
			{
				className = className.Remove(0, 1);
			}

			Undo.RecordObject(_model.SelectedElement, "Remove Class");
			_model.SelectedElement.RemoveClass(className);
			RefreshClassesList();
			RefreshHeight();

			EditorUtility.SetDirty(_model.SelectedElement);
			_model.ForceRefresh();
		}

		private void RefreshHeight()
		{
			_classesList.style.SetHeight(0.0f);

			float height = MIN_CONTENT_HEIGHT;

			if (_contentContainer.ClassListContains("hidden"))
			{
				_contentContainer.style.SetHeight(0.0f);
				return;
			}

			if (_model.SelectedElement != null)
			{
				float allClassesHeight = SINGLE_CLASS_ENTRY_HEIGHT * _model.SelectedElement.Classes.Count();
				height += allClassesHeight;

				_classesList.style.SetHeight(allClassesHeight);
			}

			_contentContainer.style.SetHeight(height);
		}

		private ListView CreateClassesList()
		{
			ListView view = new ListView
			{
				makeItem = CreateListViewElement,
				bindItem = BindListViewElement,
				selectionType = SelectionType.Single,
				itemsSource = _model.SelectedElement != null
					? _model.SelectedElement.Classes.ToList()
					: new List<string>()
			};
			view.SetItemHeight(SINGLE_CLASS_ENTRY_HEIGHT);
			view.name = "classesList";

#if UNITY_2020_1_OR_NEWER
			view.onSelectionChange += SelectionChanged;
#else
			view.onSelectionChanged += SelectionChanged;
#endif

			return view;
		}

#if UNITY_2020_1_OR_NEWER
		private void SelectionChanged(IEnumerable<object> obj)
		{
			List<string> list = (List<string>)_classesList.itemsSource;
			List<object> objects = obj.ToList();
			_selectedClassListIndex = list.FindIndex(el => el == (string)objects[0]);
		}
#else
		private void SelectionChanged(List<object> obj)
		{
			List<string> list = (List<string>)_classesList.itemsSource;
			_selectedClassListIndex = list.FindIndex(el => el == (string)obj[0]);
		}
#endif

		private void RefreshClassesList()
		{
			_classesList.itemsSource = _model.SelectedElement
				? BussNameUtility.AsClassesList(_model.SelectedElement.Classes.ToList())
				: new List<string>();
			_classesList.RefreshPolyfill();
		}

		private VisualElement CreateListViewElement()
		{
			VisualElement classElement = new VisualElement { name = "classElement" };
			classElement.Add(new VisualElement { name = "space" });
			classElement.Add(new TextField());
			return classElement;
		}

		private void BindListViewElement(VisualElement element, int index)
		{
			TextField textField = (TextField)element.Children().ToList()[1];
			textField.value = BussNameUtility.AsClassSelector(_classesList.itemsSource[index] as string);
			textField.isDelayed = true;
			BindTextfieldCallback(textField, ClassValueChanged);
			void ClassValueChanged(ChangeEvent<string> evt)
			{
				string newValue = BussNameUtility.AsClassSelector(evt.newValue);
				_classesList.itemsSource[index] = newValue;
				textField.SetValueWithoutNotify(newValue);
				Undo.RecordObject(_model.SelectedElement, "Change classes");
				_model.SelectedElement.UpdateClasses(BussNameUtility.AsCleanList((List<string>)_classesList.itemsSource));
				EditorUtility.SetDirty(_model.SelectedElement);
				_model.ForceRefresh();
			}
		}

		private void BindTextfieldCallback(TextField textField, EventCallback<ChangeEvent<string>> cb)
		{
			// before we can add the new callback, we need to unregister the old one, other
			//  we end up with many many callbacks on each element, which leads to perf issues
			//  and editor crashes
			if (_changeHandlers.TryGetValue(textField, out var existing))
			{
				textField.UnregisterCallback(existing);
				_changeHandlers[textField] = cb;
			}
			else
			{
				_changeHandlers.Add(textField, cb);
			}
			textField.RegisterValueChangedCallback(cb);

		}

		protected override void OnDestroy()
		{
			_model.Change -= Refresh;
			_selectedClassListIndex = null;
		}
	}
}
