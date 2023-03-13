using System.Linq;
using UnityEngine.UIElements;
using static Beamable.Common.Constants;
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Components
{
	public class StyleCardVisualElement : BeamableVisualElement
	{
		private readonly StyleCardModel _model;

		private BussSelectorLabelVisualElement _selectorLabelComponent;
		private VisualElement _addRuleButton;
		private VisualElement _addVariableButton;
		private VisualElement _cleanAllButton;
		private VisualElement _colorBlock;
		private VisualElement _optionsButton;
		private VisualElement _propertiesParent;
		private VisualElement _selectorLabelParent;
		private VisualElement _showAllButton;
		private TextElement _showAllButtonText;
		// TODO: restore while doing BEAM-3122
		// private VisualElement _undoButton;
		private VisualElement _variablesParent;
		private Image _foldIcon;

		public StyleCardVisualElement(StyleCardModel model) : base(
			$"{BUSS_THEME_MANAGER_PATH}/{nameof(StyleCardVisualElement)}/{nameof(StyleCardVisualElement)}")
		{
			_model = model;
		}

		public override void Refresh()
		{
			base.Refresh();

			_selectorLabelParent = Root.Q<VisualElement>("selectorLabelParent");
			_variablesParent = Root.Q<VisualElement>("variables");
			_propertiesParent = Root.Q<VisualElement>("properties");
			_colorBlock = Root.Q<VisualElement>("foldIconParent");

			_foldIcon = new Image { name = "foldIcon" };
			_colorBlock.Add(_foldIcon);

			_optionsButton = Root.Q<VisualElement>("optionsButton");
			_optionsButton.tooltip = Tooltips.Buss.OPTIONS;

			// TODO: restore while doing BEAM-3122
			// _undoButton = Root.Q<VisualElement>("undoButton");
			// _undoButton.tooltip = Tooltips.Buss.UNDO;

			_cleanAllButton = Root.Q<VisualElement>("cleanAllButton");
			_cleanAllButton.tooltip = Tooltips.Buss.ERASE_ALL_STYLE;

			_addVariableButton = Root.Q<VisualElement>("addVariableButton");
			_addRuleButton = Root.Q<VisualElement>("addRuleButton");
			_showAllButton = Root.Q<VisualElement>("showAllButton");
			_showAllButtonText = Root.Q<TextElement>("showAllButtonText");

			RegisterButtonActions();
			CreateSelectorLabel();
			RefreshProperties();
			UpdateShowAllStatus();
			RefreshButtons();
			SetFold();

			_colorBlock.EnableInClassList("active", _model.IsSelected);
			_model.Change += OnChange;
			_colorBlock.RegisterCallback<MouseDownEvent>(_model.FoldButtonClicked);
		}


		public void RepaintProperties()
		{
			SetSelectorStatus();
			foreach (var property in Root.Query<StylePropertyVisualElement>().ToList())
			{
				property.Refresh();
			}
		}

		private void SetSelectorStatus()
		{
			var appliedStatus = _model.RuleAppliedStatus;
			_selectorLabelComponent.EnableInClassList("is-exact", appliedStatus == RuleAppliedStatus.Exact);
			_selectorLabelComponent.EnableInClassList("is-inherited", appliedStatus == RuleAppliedStatus.Inherited);
			_selectorLabelComponent.EnableInClassList("is-not-applied", appliedStatus == RuleAppliedStatus.NotApplied);

		}

		private void SetFold()
		{
			_foldIcon.ToggleInClassList(_model.IsFolded ? "folded" : "unfolded");

			if (!_model.IsFolded)
			{
				return;
			}

			_variablesParent.AddToClassList("hidden");
			_propertiesParent.AddToClassList("hidden");
		}

		protected override void OnDestroy()
		{
			_model.Change -= OnChange;
			ClearButtonActions();
		}

		private void ClearButtonActions()
		{
			// TODO: restore while doing BEAM-3122
			// _undoButton?.UnregisterCallback<MouseDownEvent>(_model.UndoButtonClicked);
			_cleanAllButton?.UnregisterCallback<MouseDownEvent>(_model.ClearAllButtonClicked);
			_addVariableButton?.UnregisterCallback<MouseDownEvent>(_model.AddVariableButtonClicked);
			_addRuleButton?.UnregisterCallback<MouseDownEvent>(_model.AddRuleButtonClicked);
			_showAllButton?.UnregisterCallback<MouseDownEvent>(_model.ShowAllButtonClicked);
			_optionsButton?.UnregisterCallback<MouseDownEvent>(_model.OptionsButtonClicked);
		}

		private void ClearSpawnedProperties()
		{
			while (_propertiesParent.Children().Count() > 1)
			{
				var currentCount = _propertiesParent.Children().Count();
				_propertiesParent.RemoveAt(currentCount - 1);
			}

			while (_variablesParent.Children().Count() > 1)
			{
				var currentCount = _variablesParent.Children().Count();
				_variablesParent.RemoveAt(currentCount - 1);
			}
		}

		private void CreateSelectorLabel()
		{
			_selectorLabelComponent?.Destroy();
			_selectorLabelParent.Clear();

			_selectorLabelComponent = new BussSelectorLabelVisualElement();

			_selectorLabelComponent.Setup(_model.StyleRule, _model.StyleSheet, _model.OnSelectorChanged, _model.PrepareCommands);
			_selectorLabelParent.Add(_selectorLabelComponent);
			SetSelectorStatus();
		}

		private void OnChange()
		{
			RefreshProperties();
			UpdateShowAllStatus();
			RefreshButtons();
		}

		private void RefreshButtons()
		{
			// TODO: restore while doing BEAM-3122
			// _undoButton.SetEnabled(_model.IsWritable);
			_cleanAllButton.SetEnabled(_model.IsWritable);
			_addVariableButton.SetEnabled(_model.IsWritable);
			_addRuleButton.SetEnabled(_model.IsWritable);
			_showAllButton.SetEnabled(_model.IsWritable);
			_optionsButton.SetEnabled(true);
		}

		private void RefreshProperties()
		{
			ClearSpawnedProperties();

			foreach (StylePropertyModel model in _model.GetProperties())
			{
				if (!_model.ShowAll && !model.IsInStyle)
				{
					continue;
				}

				StylePropertyVisualElement element = new StylePropertyVisualElement(model);
				element.Init();
				(model.IsVariable ? _variablesParent : _propertiesParent).Add(element);
			}
		}

		private void RegisterButtonActions()
		{
			// TODO: restore while doing BEAM-3122
			// _undoButton?.RegisterCallback<MouseDownEvent>(_model.UndoButtonClicked);
			_cleanAllButton?.RegisterCallback<MouseDownEvent>(_model.ClearAllButtonClicked);
			_addVariableButton?.RegisterCallback<MouseDownEvent>(_model.AddVariableButtonClicked);
			_addRuleButton?.RegisterCallback<MouseDownEvent>(_model.AddRuleButtonClicked);
			_showAllButton?.RegisterCallback<MouseDownEvent>(_model.ShowAllButtonClicked);
			_optionsButton?.RegisterCallback<MouseDownEvent>(_model.OptionsButtonClicked);
		}

		private void UpdateShowAllStatus()
		{
			EnableInClassList("showAllProperties", _model.ShowAll);
			_showAllButtonText.text = _model.ShowAll ? TOGGLE_HIDE_ALL : TOGGLE_SHOW_ALL;
			_showAllButton.EnableInClassList("clicked", _model.ShowAll);
		}
	}
}
