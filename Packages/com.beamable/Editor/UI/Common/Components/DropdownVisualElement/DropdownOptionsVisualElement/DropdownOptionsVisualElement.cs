using System;
using System.Collections.Generic;
using UnityEngine;
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
	public class DropdownOptionsVisualElement : BeamableVisualElement
	{
		private VisualElement _mainContainer;
		private readonly List<DropdownSingleOptionVisualElement> _allOptions = new List<DropdownSingleOptionVisualElement>();
		private Action _onDestroy;

		public new class UxmlFactory : UxmlFactory<DropdownOptionsVisualElement, UxmlTraits>
		{
		}

		public DropdownOptionsVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(DropdownVisualElement)}/{nameof(DropdownOptionsVisualElement)}/{nameof(DropdownOptionsVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_mainContainer = Root.Q<VisualElement>("mainVisualElement");

			RenderOptions();
		}

		protected override void OnDestroy()
		{
			_onDestroy?.Invoke();
		}

		public DropdownOptionsVisualElement Setup(List<DropdownSingleOptionVisualElement> options, Action onDestroy)
		{
			_allOptions.Clear();
			_allOptions.AddRange(options);

			_onDestroy = onDestroy;
			return this;
		}

		public float GetHeight()
		{
			float overallHeight = 0.0f;

			foreach (DropdownSingleOptionVisualElement option in _allOptions)
			{
				overallHeight += option.Height;
				if (option.LineBelow)
				{
					overallHeight += 10;
				}
			}

			return overallHeight;
		}

		private void RenderOptions()
		{
			foreach (VisualElement child in _mainContainer.Children())
			{
				_mainContainer.Remove(child);
			}

			foreach (DropdownSingleOptionVisualElement option in _allOptions)
			{
				_mainContainer.Add(option);
				if (option.LineBelow)
				{
					var spacer = new VisualElement();
					spacer.AddToClassList("dropdown-spacer");

					_mainContainer.Add(spacer);
				}
				option.Refresh();
			}

			_mainContainer.style.SetHeight(GetHeight());
		}
	}
}
