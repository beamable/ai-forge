using Beamable.UI.Buss;
using TMPro;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Components
{
	public class TextAlignmentBussPropertyVisualElement : BussPropertyVisualElement<TextAlignmentOptionsBussProperty>
	{
		private VisualElement _horizontalContainer;
		private VisualElement _verticalContainer;

		public TextAlignmentBussPropertyVisualElement(TextAlignmentOptionsBussProperty property) : base(property) { }

		public override void Init()
		{
			base.Init();

			Root.style.SetFlexDirection(FlexDirection.Column);
			_horizontalContainer = new VisualElement();
			_horizontalContainer.style.SetFlexDirection(FlexDirection.Row);
			Root.Add(_horizontalContainer);
			_verticalContainer = new VisualElement();
			_verticalContainer.style.SetFlexDirection(FlexDirection.Row);
			Root.Add(_verticalContainer);
			for (int i = 0; i < 6; i++)
			{
				var boxedI = i;
				var button = new ToolbarButton(() =>
				{
					SetHorizontalValue(boxedI);
				});
				var image = new Image();
				image.SetImage(TextMeshProHelper.UIStyleManager.alignContentA[i].image);
				button.Add(image);
				_horizontalContainer.Add(button);
			}

			for (int i = 0; i < 6; i++)
			{
				var boxedI = i;
				var button = new ToolbarButton(() =>
				{
					SetVerticalValue(boxedI);
				});
				var image = new Image();
				image.SetImage(TextMeshProHelper.UIStyleManager.alignContentB[i].image);
				image.scaleMode = ScaleMode.ScaleToFit;
				button.Add(image);
				_verticalContainer.Add(button);
			}

			UpdateHorizontalToggle();
			UpdateVerticalToggle();
		}

		private void SetHorizontalValue(int value)
		{
			OnBeforeChange?.Invoke();
			var intValue = (int)Property.Enum;
			Property.Enum = (TextAlignmentOptions)GetValue(value, GetVerticalAlignmentGridValue(intValue));
			TriggerStyleSheetChange();
			UpdateHorizontalToggle();
		}

		private void SetVerticalValue(int value)
		{
			OnBeforeChange?.Invoke();
			var intValue = (int)Property.Enum;
			Property.Enum = (TextAlignmentOptions)GetValue(GetHorizontalAlignmentGridValue(intValue), value);
			TriggerStyleSheetChange();
			UpdateVerticalToggle();
		}

		private void UpdateHorizontalToggle()
		{
			var value = GetHorizontalAlignmentGridValue((int)Property.Enum);
			int i = 0;
			foreach (VisualElement element in _horizontalContainer.Children())
			{
				var tb = (ToolbarButton)element;
				tb.SetSelected(value == i++);
			}
		}

		private void UpdateVerticalToggle()
		{
			var value = GetVerticalAlignmentGridValue((int)Property.Enum);
			int i = 0;
			foreach (VisualElement element in _verticalContainer.Children())
			{
				var tb = (ToolbarButton)element;
				tb.SetSelected(value == i++);
			}
		}

		public override void OnPropertyChangedExternally()
		{
			UpdateHorizontalToggle();
			UpdateVerticalToggle();
		}

		#region TMP Helper Methods

		/// <summary>
		/// Function to return the horizontal alignment grid value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static int GetHorizontalAlignmentGridValue(int value)
		{
			if ((value & 0x1) == 0x1)
				return 0;
			else if ((value & 0x2) == 0x2)
				return 1;
			else if ((value & 0x4) == 0x4)
				return 2;
			else if ((value & 0x8) == 0x8)
				return 3;
			else if ((value & 0x10) == 0x10)
				return 4;
			else if ((value & 0x20) == 0x20)
				return 5;

			return 0;
		}

		/// <summary>
		/// Function to return the vertical alignment grid value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private static int GetVerticalAlignmentGridValue(int value)
		{
			if ((value & 0x100) == 0x100)
				return 0;
			if ((value & 0x200) == 0x200)
				return 1;
			if ((value & 0x400) == 0x400)
				return 2;
			if ((value & 0x800) == 0x800)
				return 3;
			if ((value & 0x1000) == 0x1000)
				return 4;
			if ((value & 0x2000) == 0x2000)
				return 5;

			return 0;
		}

		private static int GetValue(int horizontalValue, int verticalValue)
		{
			return (0x1 << horizontalValue) | (0x100 << verticalValue);
		}

		#endregion
	}
}
