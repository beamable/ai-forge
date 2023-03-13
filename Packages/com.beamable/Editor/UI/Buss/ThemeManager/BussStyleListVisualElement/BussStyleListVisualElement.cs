using Beamable.Editor.UI.Common;
using Beamable.Editor.UI.Components;
using Beamable.UI.Buss;
using System.Collections.Generic;
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Buss
{
	public class BussStyleListVisualElement : BeamableBasicVisualElement
	{
		private readonly ThemeModel _model;

		private readonly List<StyleCardVisualElement> _styleCardsVisualElements =
			new List<StyleCardVisualElement>();

		private bool _inStyleSheetChangedLoop;

		public BussStyleListVisualElement(ThemeModel model) : base(
			$"{BUSS_THEME_MANAGER_PATH}/{nameof(BussStyleListVisualElement)}/{nameof(BussStyleListVisualElement)}.uss",
			false)
		{
			_model = model;
			_model.Change += Refresh;
		}

		public override void Refresh()
		{
			RefreshCards();
		}

		protected override void OnDestroy()
		{
			_model.Change -= Refresh;

			ClearCards();

			_model.PropertyDatabase.Discard();
		}

		private void AddStyleCard(BussStyleSheet styleSheet, BussStyleRule styleRule)
		{
			bool isSelected = _model.SelectedElement != null && styleRule.Selector.CheckMatch(_model.SelectedElement);
			StyleCardModel model =
				new StyleCardModel(styleSheet, styleRule, _model.SelectedElement, isSelected,
								   _model.PropertyDatabase, _model.WritableStyleSheets,
								   _model.ForceRefresh, _model.DisplayFilter);
			model.SelectorChanged += () =>
			{
				foreach (var card in _styleCardsVisualElements)
				{
					card.RepaintProperties();
				}
			};
			StyleCardVisualElement styleCard = new StyleCardVisualElement(model);
			styleCard.Refresh();

			_styleCardsVisualElements.Add(styleCard);
			Root.Add(styleCard);
		}

		private void ClearCards()
		{
			foreach (var element in _styleCardsVisualElements)
			{
				RemoveStyleCard(element);
			}

			_styleCardsVisualElements.Clear();
		}

		private void RefreshCards()
		{
			ClearCards();

			foreach (var pair in _model.FilteredRules)
			{
				var styleSheet = pair.Value;
				var styleRule = pair.Key;

				AddStyleCard(styleSheet, styleRule);
			}
		}

		private void RemoveStyleCard(StyleCardVisualElement card)
		{
			card.RemoveFromHierarchy();
			card.Destroy();
		}
	}
}
