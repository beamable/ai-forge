using Beamable.Common;
using Beamable.Common.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Beamable.UI.Buss
{
	public class BussConfiguration : ModuleConfigurationObject, IVariablesProvider
	{
		public static Optional<BussConfiguration> OptionalInstance
		{
			get
			{
				try
				{
					return new Optional<BussConfiguration> { Value = Instance, HasValue = true };
				}
				catch (Exception)
				{
					return new Optional<BussConfiguration>();
				}
			}
		}

		private static BussConfiguration Instance => Get<BussConfiguration>();
		private readonly List<BussElement> _rootBussElements = new List<BussElement>();

		public List<BussStyleSheet> FactoryStyleSheets
		{
			get
			{
				BussStyleSheet[] bussStyleSheets = Resources
												   .LoadAll<BussStyleSheet>(
													   Constants.Features.Buss.Paths.FACTORY_STYLES_RESOURCES_PATH)
												   .Where(styleSheet => styleSheet.IsReadOnly).ToArray();

				return bussStyleSheets.OrderBy(s => s.SortingOrder).ToList();
			}
		}

		public List<BussStyleSheet> DeveloperStyleSheets =>
			Resources.LoadAll<BussStyleSheet>("")
					 .Where(styleSheet => !styleSheet.IsReadOnly).ToList();

		public List<BussElement> RootBussElements => _rootBussElements;

		public static void UseConfig(Action<BussConfiguration> callback)
		{
			OptionalInstance.DoIfExists(callback);
		}

		public void RegisterObserver(BussElement bussElement)
		{
			// TODO: serve case when user adds (by Add Component option, not by changing hierarchy) BUSSStyleProvider
			// component somewhere "above" currently topmost BUSSStyleProvider(s) causing to change whole hierarchy

			if (!_rootBussElements.Contains(bussElement))
			{
				_rootBussElements.Add(bussElement);
			}
		}

		public void UnregisterObserver(BussElement bussElement)
		{
			_rootBussElements.Remove(bussElement);
		}

		public void UpdateStyleSheet(BussStyleSheet styleSheet)
		{
			// This should happen only in editor
			if (styleSheet == null) return;

			if (FactoryStyleSheets.Contains(styleSheet) || DeveloperStyleSheets.Contains(styleSheet))
			{
				foreach (BussElement bussElement in _rootBussElements)
				{
					bussElement.OnStyleChanged();
				}
			}
			else
			{
				foreach (BussElement bussElement in _rootBussElements)
				{
					OnStyleSheetChanged(bussElement, styleSheet);
				}
			}
		}

		public void ForceRefresh()
		{
			foreach (var element in _rootBussElements)
			{
				element.OnStyleChanged();
			}
		}

		private void OnStyleSheetChanged(BussElement element, BussStyleSheet styleSheet)
		{
			if (element == null) return;
			if (element.StyleSheet == styleSheet)
			{
				element.OnStyleChanged();
			}
			else
			{
				foreach (BussElement child in element.Children)
				{
					OnStyleSheetChanged(child, styleSheet);
				}
			}
		}

#if UNITY_EDITOR
		static BussConfiguration()
		{
			// temporary solution to refresh the list of BussElements on scene change
			EditorSceneManager.sceneOpened += (scene, mode) => UseConfig(config => config.RefreshBussElements());
			EditorSceneManager.sceneClosed += scene => UseConfig(config => config.RefreshBussElements());
		}

		void RefreshBussElements()
		{
			_rootBussElements.Clear();
			foreach (BussElement element in FindObjectsOfType<BussElement>())
			{
				element.CheckParent();
			}

			EditorUtility.SetDirty(this);
		}
#endif

		#region Styles parsing

		public void RecalculateStyle(BussElement element)
		{
			element.Style.Inherit(element?.Parent?.Style);
			element.Sources.Recalculate();
			foreach (var key in element.Sources.GetKeys())
			{
				element.Style[key] = (element.Sources.GetUsedPropertyProvider(key, out _)?.GetProperty()) ??
									 BussStyle.GetDefaultValue(key);
			}
			element.ApplyStyle();
		}

		#endregion

		public List<BussStyleSheet> GetStylesheets()
		{
			var list = new List<BussStyleSheet>();
			list.AddRange(FactoryStyleSheets);
			list.AddRange(DeveloperStyleSheets);
			return list;
		}
	}
}
