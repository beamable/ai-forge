using Beamable.Common;
using Beamable.Common.Runtime;
using Beamable.Editor.UI.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
	public class SearchabledDropdownVisualElement : BeamableVisualElement
	{
		public static readonly string ComponentPath = $"{Directories.COMMON_COMPONENTS_PATH}/{nameof(SearchabledDropdownVisualElement)}/{nameof(SearchabledDropdownVisualElement)}";

		private string _switchText;

		private VisualElement _root;
		private ISearchableElement _selectedElement;
		private List<ISearchableElement> _elementViews;
		private LoadingIndicatorVisualElement _loadingIndicator;
		private VisualElement _mainContent;
		private SearchBarVisualElement _searchBar;
		private Button _refreshButton;

		public ISearchableModel Model { get; set; }

#pragma warning disable 67
		public event Action<ISearchableElement> OnElementDelete;
		public event Action<ISearchableElement> OnElementSelected;
#pragma warning restore 67

		public SearchabledDropdownVisualElement(string switchText = null) : base(ComponentPath)
		{
			this._switchText = switchText;
		}

		protected override void OnDetach()
		{
			Model.OnAvailableElementsChanged -= OnUpdated;
			Model.OnElementChanged -= OnActiveChanged;
			base.OnDetach();
		}

		private void OnActiveChanged(ISearchableElement element)
		{
			_selectedElement = element;
			SetList(_elementViews, _root);
		}

		public override void Refresh()
		{
			base.Refresh();

			_loadingIndicator = Root.Q<LoadingIndicatorVisualElement>();
			_mainContent = Root.Q<VisualElement>("mainBlockedContent");
			_searchBar = Root.Q<SearchBarVisualElement>();
			_refreshButton = Root.Q<Button>("refreshButton");
			_root = Root.Q<VisualElement>("elementsList");

			_loadingIndicator.SetPromise(Model.RefreshAvailable(), _mainContent);
			_refreshButton.clickable.clicked += () =>
				{
					_loadingIndicator.SetPromise(Model.RefreshAvailable(), _mainContent);
				};

			_selectedElement = Model.Current;
			Model.OnAvailableElementsChanged -= OnUpdated;
			Model.OnAvailableElementsChanged += OnUpdated;
			Model.OnElementChanged -= OnActiveChanged;
			Model.OnElementChanged += OnActiveChanged;

			_searchBar.OnSearchChanged += filter =>
			{
				SetList(Model.Elements, _root, filter.ToLower());
			};
			_searchBar.DoFocus();
			OnUpdated(Model.Elements);
		}

		private void OnUpdated(List<ISearchableElement> elements)
		{
			_elementViews = elements;
			SetList(_elementViews, _root);
		}

		private void SetList(IEnumerable<ISearchableElement> elements, VisualElement listRoot, string filter = null)
		{
			listRoot.Clear();
			if (elements == null) return;

			elements = elements.Where(r => r.IsAvailable()).OrderBy(r => r.GetOrder());
			foreach (var singleElement in elements)
			{
				if (singleElement.IsToSkip(filter))
					continue;

				var selectButton = new Button();
				selectButton.text = singleElement.DisplayName;
				selectButton.clickable.clicked += () =>
				{
					_loadingIndicator.SetText(_switchText);
					_loadingIndicator.SetPromise(new Promise<int>(), _mainContent, _refreshButton);
					_searchBar.SetEnabled(false);
					EditorApplication.delayCall += () => OnElementSelected?.Invoke(singleElement);
				};

				if (string.Equals(singleElement?.DisplayName, _selectedElement?.DisplayName))
				{
					selectButton.AddToClassList("selected");
					selectButton.SetEnabled(false);
				}
				else
				{
					if (this.OnElementDelete != null)
					{
						var deleteButton = new Button();
						selectButton.Add(deleteButton);
						deleteButton.AddToClassList("deleteButton");
						deleteButton.clickable.clicked += () =>
						{
							OnElementDelete?.Invoke(singleElement);
						};
					}
				}

				var classNameToAdd = singleElement.GetClassNameToAdd();

				if (!string.IsNullOrEmpty(classNameToAdd))
				{
					selectButton.AddToClassList(classNameToAdd);
				}

				listRoot.Add(selectButton);
			}
		}
	}
}
