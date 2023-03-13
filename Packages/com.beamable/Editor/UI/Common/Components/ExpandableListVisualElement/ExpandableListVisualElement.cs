using System.Collections.Generic;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UIElements.StyleSheets;
#endif
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class ExpandableListVisualElement : BeamableVisualElement
	{
		private const string HIDDEN_CLASS = "hidden";
		private const string EXPANDED_CLASS = "expanded";

		private List<string> elements;
		private string[] displayValues;
		private bool expanded;
		private Label label;
		private Label plusLabel;
		private VisualElement arrowImage;
		private VisualElement valueContainer;

		public new class UxmlFactory : UxmlFactory<ExpandableListVisualElement, UxmlTraits>
		{
		}

		public ExpandableListVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(ExpandableListVisualElement)}/{nameof(ExpandableListVisualElement)}")
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			label = Root.Q<Label>("value");
			valueContainer = Root.Q<VisualElement>("valueContainer");
			arrowImage = Root.Q<VisualElement>("arrowImage");
			plusLabel = Root.Q<Label>("plusLabel");

			SetupLabel();

			var button = Root.Q<VisualElement>("mainContainer");
			button.RegisterCallback<MouseDownEvent>(_ => ToggleExpand());
		}

		private void OnLabelSizeChanged(GeometryChangedEvent evt)
		{
			float width = evt.newRect.width;
			int maxCharacters = GetMaxNumberOfCharacters(width);
			displayValues = new string[elements.Count];
			for (int i = 0; i < elements.Count; i++)
			{
				displayValues[i] = TrimString(elements[i], maxCharacters);
			}

			SetupLabel();
		}

		private int GetMaxNumberOfCharacters(float width)
		{
			float charactersNumberFactor = elements.Count > 1 ? width / 9 : width / 8;
			return Mathf.CeilToInt(charactersNumberFactor);
		}

		private void SetupLabel()
		{
			if (displayValues == null || displayValues.Length == 0)
			{
				label.text = "";
				plusLabel.text = "";
				arrowImage.AddToClassList(HIDDEN_CLASS);
				return;
			}

			label.text = displayValues[0];
			label.UnregisterCallback<GeometryChangedEvent>(OnLabelSizeChanged);
			label.RegisterCallback<GeometryChangedEvent>(OnLabelSizeChanged);

			if (displayValues.Length == 1)
			{
				arrowImage.AddToClassList("--positionHidden");
				plusLabel.AddToClassList("--positionHidden");
				return;
			}

			if (expanded)
			{
				for (int i = 1; i < displayValues.Length; i++)
				{
					label.text += "\n" + displayValues[i];
				}

#if UNITY_2019_1_OR_NEWER
				label.style.maxWidth = new StyleLength(GetMaxWidth(elements.ToArray()));
				valueContainer.style.maxWidth = new StyleLength(GetMaxWidth(elements.ToArray()));
#elif UNITY_2018
				label.style.maxWidth = new StyleValue<float>(GetMaxWidth(elements.ToArray()));
				valueContainer.style.maxWidth = new StyleValue<float>(GetMaxWidth(elements.ToArray()));
#endif
				arrowImage.AddToClassList(EXPANDED_CLASS);
			}
			else
			{
#if UNITY_2019_1_OR_NEWER
				label.style.maxWidth = new StyleLength(GetMaxWidth(elements[0]));
				valueContainer.style.maxWidth = new StyleLength(GetMaxWidth(elements[0]));
#elif UNITY_2018
				label.style.maxWidth = new StyleValue<float>(GetMaxWidth(elements[0]));
				valueContainer.style.maxWidth = new StyleValue<float>(GetMaxWidth(elements[0]));
#endif
				plusLabel.text = $"{displayValues.Length - 1}+";
			}
		}

		public void Setup(List<string> listElements, bool isExpanded = false)
		{
			elements = listElements;
			displayValues = listElements.ToArray();
			expanded = isExpanded;
			Refresh();
		}

		private void ToggleExpand()
		{
			expanded = !expanded;
			Refresh();
		}

		private string TrimString(string text, int maxCharacters)
		{
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}

			return text.Length > maxCharacters ?
				text.Substring(0, maxCharacters) + "..." : text;
		}

		private float GetMaxWidth(params string[] values)
		{
			int max = 0;
			foreach (var element in values)
			{
				if (element.Length > max)
				{
					max = element.Length;
				}
			}

			return max * 9;
		}
	}
}
