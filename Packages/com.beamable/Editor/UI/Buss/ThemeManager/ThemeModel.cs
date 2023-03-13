using Beamable.Common;
using Beamable.UI.Buss;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Buss.ThemeManager;

namespace Beamable.Editor.UI.Buss
{
	public abstract class ThemeModel
	{
		public event Action Change;

		public enum PropertyDisplayFilter
		{
			All,
			IgnoreOverridden
		}

		public readonly Dictionary<BussElement, int> FoundElements = new Dictionary<BussElement, int>();

		protected BussCardFilter Filter;

		public PropertyDisplayFilter DisplayFilter { get; set; }

		public abstract BussElement SelectedElement { get; set; }

		protected abstract List<BussStyleSheet> SceneStyleSheets { get; }

		public abstract Dictionary<BussStyleRule, BussStyleSheet> FilteredRules { get; }

		public abstract List<BussStyleSheet> WritableStyleSheets { get; }

		public PropertySourceDatabase PropertyDatabase { get; } = new PropertySourceDatabase();

		public void ForceRefresh()
		{
			Change?.Invoke();
		}

		public void NavigationElementClicked(BussElement element)
		{
			Selection.activeGameObject = Selection.activeGameObject == element.gameObject ? null : element.gameObject;
		}

		#region Action bar buttons' actions

		public void OnAddStyleButtonClicked()
		{
			int styleSheetCount = WritableStyleSheets.Count();

			if (styleSheetCount == 0)
			{
				return;
			}

			if (styleSheetCount == 1)
			{
				CreateEmptyStyle(WritableStyleSheets.First());
			}
			else if (styleSheetCount > 1)
			{
				OpenAddStyleMenu(WritableStyleSheets);
			}
		}

		private void OpenAddStyleMenu(IEnumerable<BussStyleSheet> bussStyleSheets)
		{
			GenericMenu context = new GenericMenu();
			context.AddItem(new GUIContent(ADD_STYLE_OPTIONS_HEADER), false, () => { });
			context.AddSeparator(string.Empty);
			foreach (BussStyleSheet styleSheet in bussStyleSheets)
			{
				context.AddItem(new GUIContent(styleSheet.name), false, () =>
				{
					CreateEmptyStyle(styleSheet);
				});
			}

			context.ShowAsContext();
		}

		private void CreateEmptyStyle(BussStyleSheet selectedStyleSheet, string selectorName = "*")
		{
			Undo.RecordObject(selectedStyleSheet, "Add style rule");

			if (SelectedElement != null)
			{
				selectorName = BussNameUtility.GetLabel(SelectedElement);
			}

			BussStyleRule selector = BussStyleRule.Create(selectorName, new List<BussPropertyProvider>());
			selector.SetForcedVisualPriority();
			selectedStyleSheet.Styles.Add(selector);
			selectedStyleSheet.TriggerChange();
			AssetDatabase.SaveAssets();
			selector.SetForcedVisualPriority();
			Change?.Invoke();
		}

		public void OnDocsButtonClicked()
		{
			Application.OpenURL(Constants.URLs.Documentations.URL_DOC_BUSS_THEME_MANAGER);
		}

		#endregion
	}
}
