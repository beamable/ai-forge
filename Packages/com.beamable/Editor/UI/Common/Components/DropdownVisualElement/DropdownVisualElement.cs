using Beamable.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Beamable.Common.Constants;

namespace Beamable.Editor.UI.Components
{
	public class DropdownVisualElement : BeamableVisualElement
	{
		public new class UxmlFactory : UxmlFactory<DropdownVisualElement, UxmlTraits> { }

		private const float _SAFE_MIN_WIDTH = 1000;
		private const float _SAFE_MIN_HEIGHT = 24.0f;

		private readonly List<DropdownSingleOption> _optionModels;

		private VisualElement _button;
		private Label _label;

		private Action<int> _onSelection;
		private BeamablePopupWindow _optionsPopup;
		private VisualElement _root;
		private int _toTruncate;
		private string _value;

		private string Value
		{
			get => _value;
			set
			{
				_value = value;
				if (_label != null) _label.text = FormatString(_value);
			}
		}

		public DropdownVisualElement() : base(
			$"{Directories.COMMON_COMPONENTS_PATH}/{nameof(DropdownVisualElement)}/{nameof(DropdownVisualElement)}")
		{
			Value = String.Empty;
			_optionModels = new List<DropdownSingleOption>();
		}

		public override void Refresh()
		{
			base.Refresh();

			_root = Root.Q<VisualElement>("mainVisualElement");

			_label = Root.Q<Label>("value");
			_label.text = Value;

			_button = Root.Q<VisualElement>("button");
			_label.UnregisterCallback<MouseDownEvent>(async (e) => await OnButtonClicked(worldBound));
			_label.RegisterCallback<MouseDownEvent>(async (e) => await OnButtonClicked(worldBound));
			_button.UnregisterCallback<MouseDownEvent>(async (e) => await OnButtonClicked(worldBound));
			_button.RegisterCallback<MouseDownEvent>(async (e) => await OnButtonClicked(worldBound));

			_label.RegisterCallback<GeometryChangedEvent>(GeometryChanged);
		}

		public void Set(int id, bool invokeSelection = true)
		{
			OnOptionSelectedInternal(id);

			if (invokeSelection)
			{
				_onSelection?.Invoke(id);
			}
		}

		public void Setup(List<string> labels,
						  Action<int> onOptionSelected,
						  int initialIndex = 0,
						  bool invokeOnStart = true)
		{
			Setup(labels.Select(x => new DropdownEntry { DisplayName = x, LineBelow = false }).ToList(), onOptionSelected,
				  initialIndex, invokeOnStart);
		}

		public void Setup(List<DropdownEntry> entries,
						  Action<int> onOptionSelected,
						  int initialIndex = 0,
						  bool invokeOnStart = true)
		{
			_optionModels.Clear();
			_onSelection = onOptionSelected;
			for (var i = 0; i < entries.Count; i++)
			{
				var entry = entries[i];
				string label = entry.DisplayName;
				int currentId = i;
				DropdownSingleOption singleOption = new DropdownSingleOption(i, label, (s) =>
				{
					OnOptionSelectedInternal(currentId);
					onOptionSelected?.Invoke(currentId);
				})
				{ LineBelow = entry.LineBelow };

				_optionModels.Add(singleOption);
			}

			initialIndex = Mathf.Clamp(initialIndex, 0, _optionModels.Count - 1);

			Value = _optionModels[initialIndex].Label;

			if (invokeOnStart)
			{
				onOptionSelected?.Invoke(initialIndex);
			}
		}

		public void SetValueWithoutVerification(string value)
		{
			Value = value;
		}

		private void GeometryChanged(GeometryChangedEvent evt)
		{
			float safeSize = evt.newRect.width - 25.0f;
			float calculateTextSize = CalculateTextSize(_value);
			bool shouldTruncate = safeSize < calculateTextSize;

			var valueLength = _value.Length - 1;

			if (shouldTruncate)
			{
				for (int i = 0; i <= valueLength; i++)
				{
					string tempValue = _value.Remove(valueLength - i);
					float tempTextSize = CalculateTextSize(tempValue);

					if (tempTextSize < safeSize)
					{
						_toTruncate = i;
						break;
					}
				}
			}
			else
			{
				_toTruncate = 0;
			}

			if (_label != null) _label.text = FormatString(_value);
		}

		private string FormatString(string value)
		{
			return _toTruncate == 0 ? value : $"{value.Remove(value.Length - 1 - _toTruncate)}...";
		}

		private float CalculateTextSize(string value)
		{
			return _label.MeasureTextSize(value, _SAFE_MIN_WIDTH, MeasureMode.AtMost, _SAFE_MIN_HEIGHT,
										  MeasureMode.AtMost).x;
		}

		private async Promise OnButtonClicked(Rect bounds)
		{
			if (_optionsPopup != null)
			{
				_optionsPopup.Close();
				OnOptionsClosed();
				return;
			}

			if (_optionModels.Count == 0)
			{
				Debug.LogWarning("Dropdown has no options to render");
				return;
			}

			Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(bounds);
			popupWindowRect.y -= 1f;

			List<DropdownSingleOptionVisualElement> allOptions = new List<DropdownSingleOptionVisualElement>();

			foreach (DropdownSingleOption option in _optionModels)
			{
				var element = new DropdownSingleOptionVisualElement().Setup(option.Label,
																			option.OnClick, _root.localBound.width,
																			_root.localBound.height, option.LineBelow);
				allOptions.Add(element);
			}

			DropdownOptionsVisualElement optionsWindow =
				new DropdownOptionsVisualElement().Setup(allOptions, OnOptionsClosed);

			_optionsPopup = await BeamablePopupWindow.ShowDropdownAsync("", popupWindowRect,
																		new Vector2(
																			_root.localBound.width,
																			optionsWindow.GetHeight()), optionsWindow);
		}

		private void OnOptionsClosed()
		{
			_optionsPopup = null;
		}

		private void OnOptionSelectedInternal(int id)
		{
			Value = _optionModels.Find(opt => opt.Id == id).Label;

			if (!_optionsPopup || _optionsPopup == null)
			{
				return;
			}

			_optionsPopup.Close();
			OnOptionsClosed();
		}
	}

	public class DropdownEntry
	{
		public string DisplayName;
		public bool LineBelow;

		public DropdownEntry() { }

		public DropdownEntry(string name)
		{
			DisplayName = name;
		}

		public DropdownEntry(string name, bool lineBelow)
		{
			DisplayName = name;
			LineBelow = lineBelow;
		}
	}

	public static class DropdownEntryExtensions
	{
		public static DropdownEntry Add(this List<DropdownEntry> set, string name)
		{
			var entry = new DropdownEntry(name);
			set.Add(entry);
			return entry;
		}

		public static void AddRange(this List<DropdownEntry> set, IEnumerable<string> names)
		{
			foreach (var name in names)
			{
				set.Add(new DropdownEntry(name));
			}
		}
	}
}
