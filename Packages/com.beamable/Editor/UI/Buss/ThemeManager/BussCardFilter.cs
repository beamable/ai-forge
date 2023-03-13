using Beamable.UI.Buss;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beamable.Editor.UI.Buss
{
	public class BussCardFilter
	{
		public string CurrentFilter { get; set; } = String.Empty;

		public Dictionary<BussStyleRule, BussStyleSheet> GetFiltered(BussStyleSheet styleSheet)
		{
			var unsortedRules = new List<(BussStyleRule, BussStyleSheet)>();
			foreach (var rule in styleSheet.Styles)
			{
				unsortedRules.Add((rule, styleSheet));
			}

			unsortedRules.Sort((a, b) =>
			{
				var forcedOrder = b.Item1.ForcedVisualPriority.CompareTo(a.Item1.ForcedVisualPriority);

				return forcedOrder;
			});


			var sortedRules = unsortedRules.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
			return sortedRules;
		}

		public Dictionary<BussStyleRule, BussStyleSheet> GetFiltered(List<BussStyleSheet> styleSheets,
																	 BussElement selectedElement)
		{
			Dictionary<BussStyleRule, BussStyleSheet> rules = new Dictionary<BussStyleRule, BussStyleSheet>();

			var ruleSet = new HashSet<(BussStyleRule, BussStyleSheet, int)>();

			foreach (var styleSheet in styleSheets)
			{
				foreach (var rule in styleSheet.Styles)
				{
					if (!CardFilter(rule, selectedElement, out var parentDistance))
					{
						continue;
					}
					ruleSet.Add((rule, styleSheet, parentDistance));
				}
			}

			if (selectedElement == null)
			{
				return rules;
			}
			var unsortedRules = ruleSet.ToList();
			unsortedRules.Sort((a, b) =>
			{
				var forcedOrder = b.Item1.ForcedVisualPriority.CompareTo(a.Item1.ForcedVisualPriority);
				if (forcedOrder != 0) return forcedOrder;

				// first, sort by exact matches. Inherited styles always play second fiddle 
				var exactMatchComparison = a.Item3.CompareTo(b.Item3);
				if (exactMatchComparison != 0) return exactMatchComparison;

				// then amongst items inherited and matched elements, prefer selector specificity 
				var weightComparison = b.Item1.Selector.GetWeight().CompareTo(a.Item1.Selector.GetWeight());
				if (weightComparison != 0) return weightComparison;

				// and finally, if there is still a tie, prefer customer sheets
				var styleSheetComparison = a.Item2.IsReadOnly.CompareTo(b.Item2.IsReadOnly);
				return styleSheetComparison;
			});


			var sortedRules = unsortedRules.ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);
			return sortedRules;
		}

		private bool CardFilter(BussStyleRule styleRule, BussElement selectedElement, out int parentDistance)
		{
			bool contains = styleRule.Properties.Any(property => property.Key.ToLower().Contains(CurrentFilter)) ||
							styleRule.Properties.Count == 0;

			parentDistance = 100;
			return selectedElement == null
				? CurrentFilter.Length <= 0 || contains
				: styleRule.Selector != null && styleRule.Selector.IsElementIncludedInSelector(selectedElement, out _, out parentDistance) && contains;
		}
	}
}
