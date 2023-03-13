using Beamable.UI.Buss;
using Beamable.UI.Sdf;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
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
	public class VertexColorBussPropertyVisualElement : BussPropertyVisualElement<VertexColorBussProperty>
	{
		private DropdownVisualElement _drawerModeDropdown;
		private VisualElement _topRow;
		private VisualElement _bottomRow;
		private ColorField _bottomLeftColor;
		private ColorField _bottomRightColor;
		private ColorField _topLeftColor;
		private ColorField _topRightColor;

		private bool _doNotUpdateMode;

		private Mode DrawerMode
		{
			get => (Mode)ColorRect.EditorHelper.GetDrawerMode(Property.ColorRect);
			set
			{
				Property.ColorRect = ColorRect.EditorHelper.WithDrawerMode(Property.ColorRect, (int)value);
				TriggerStyleSheetChange();
			}
		}

		private static readonly FieldInfo _drawerModeField = typeof(ColorRect).GetField("_drawerMode", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly List<string> _dropdownLabels = new List<string>(new string[]
		{
			"Per Vertex Color",
			"Single Color",
			"Horizontal Gradient",
			"Vertical Gradient",
			"Diagonal Gradient",
			"Alternative Diagonal Gradient",
		});

		public VertexColorBussPropertyVisualElement(VertexColorBussProperty property) : base(property) { }

		public override void Init()
		{
			base.Init();

			Root.style.SetFlexDirection(FlexDirection.Column);

			CreateDropdown();

			_topRow = CreateRowContainer();
			_bottomRow = CreateRowContainer();

			_bottomLeftColor = CreateColorField(_bottomRow);
			_bottomRightColor = CreateColorField(_bottomRow);

			_topLeftColor = CreateColorField(_topRow);
			_topRightColor = CreateColorField(_topRow);

			OnPropertyChangedExternally();
		}

		private void CreateDropdown()
		{
			_drawerModeDropdown = new DropdownVisualElement();
			_doNotUpdateMode = true;
			_drawerModeDropdown.Setup(_dropdownLabels, OnDrawerModeSelected);
			_drawerModeDropdown.Set((int)DrawerMode);
			_doNotUpdateMode = false;
			Root.Add(_drawerModeDropdown);
			_drawerModeDropdown.Refresh();
		}

		private void OnDrawerModeSelected(int index)
		{
			if (_doNotUpdateMode)
			{
				if (_topRow != null)
				{
					SetColorFieldVisibility();
				}
				return;
			}
			var mode = (Mode)index;
			DrawerMode = mode;
			if (_topRow != null)
			{
				OnValueChange(null);
				SetColorFieldVisibility();
			}
		}

		private void SetColorFieldVisibility()
		{
			switch (DrawerMode)
			{
				case Mode.SingleColor:
					_bottomLeftColor.SetHidden(false);
					_bottomRightColor.SetHidden(true);
					_topLeftColor.SetHidden(true);
					_topRightColor.SetHidden(true);
					break;
				case Mode.HorizontalGradient:
				case Mode.VerticalGradient:
				case Mode.DiagonalGradient:
				case Mode.FlippedDiagonalGradient:
					_bottomLeftColor.SetHidden(false);
					_bottomRightColor.SetHidden(true);
					_topLeftColor.SetHidden(false);
					_topRightColor.SetHidden(true);
					break;
				case Mode.PerVertexColor:
					_bottomLeftColor.SetHidden(false);
					_bottomRightColor.SetHidden(false);
					_topLeftColor.SetHidden(false);
					_topRightColor.SetHidden(false);
					break;
			}
		}

		private VisualElement CreateRowContainer()
		{
			var ve = new VisualElement();
			AddBussPropertyFieldClass(ve);
			ve.style.SetFlexDirection(FlexDirection.Row);
			Root.Add(ve);
			return ve;
		}

		private ColorField CreateColorField(VisualElement container)
		{
			var cf = new ColorField();
			AddBussPropertyFieldClass(cf);
			cf.RegisterValueChangedCallback(OnValueChange);
			container.Add(cf);
			return cf;
		}

		private void OnValueChange(ChangeEvent<Color> evt)
		{
			Color blColor, brColor, tlColor, trColor;

			switch (DrawerMode)
			{
				case Mode.SingleColor:
					blColor = brColor = tlColor = trColor = _bottomLeftColor.value;
					break;
				case Mode.HorizontalGradient:
					blColor = tlColor = _topLeftColor.value;
					brColor = trColor = _bottomLeftColor.value;
					break;
				case Mode.VerticalGradient:
					trColor = tlColor = _topLeftColor.value;
					brColor = blColor = _bottomLeftColor.value;
					break;
				case Mode.DiagonalGradient:
					tlColor = _topLeftColor.value;
					brColor = _bottomLeftColor.value;
					trColor = blColor = Color.Lerp(_topLeftColor.value, _bottomLeftColor.value, .5f);
					break;
				case Mode.FlippedDiagonalGradient:
					blColor = _topLeftColor.value;
					trColor = _bottomLeftColor.value;
					brColor = tlColor = Color.Lerp(_topLeftColor.value, _bottomLeftColor.value, .5f);
					break;
				case Mode.PerVertexColor:
				default:
					blColor = _bottomLeftColor.value;
					brColor = _bottomRightColor.value;
					tlColor = _topLeftColor.value;
					trColor = _topRightColor.value;
					break;
			}

			var mode = DrawerMode;
			var rect = new ColorRect(
				blColor,
				brColor,
				tlColor,
				trColor);
			DrawerMode = mode;
			rect = ColorRect.EditorHelper.WithDrawerMode(rect, (int)DrawerMode);
			Property.ColorRect = rect;
			OnValueChanged?.Invoke(Property);
			TriggerStyleSheetChange();
		}

		public override void OnPropertyChangedExternally()
		{
			if (IsTriggeringStyleSheetChange) return;

			_doNotUpdateMode = true;
			_drawerModeDropdown.Set((int)DrawerMode);
			_doNotUpdateMode = false;

			var colorRect = Property.ColorRect;
			Color blColor = colorRect.BottomLeftColor,
				  brColor = colorRect.BottomRightColor,
				  tlColor = colorRect.TopLeftColor,
				  trColor = colorRect.TopRightColor;

			switch (DrawerMode)
			{
				case Mode.HorizontalGradient:
					blColor = colorRect.TopRightColor;
					break;
				case Mode.DiagonalGradient:
					blColor = colorRect.BottomRightColor;
					break;
				case Mode.FlippedDiagonalGradient:
					tlColor = colorRect.BottomLeftColor;
					blColor = colorRect.TopRightColor;
					break;
			}

			_bottomLeftColor.SetValueWithoutNotify(blColor);
			_bottomRightColor.SetValueWithoutNotify(brColor);
			_topLeftColor.SetValueWithoutNotify(tlColor);
			_topRightColor.SetValueWithoutNotify(trColor);
		}

		private enum Mode
		{
			PerVertexColor,
			SingleColor,
			HorizontalGradient,
			VerticalGradient,
			DiagonalGradient,
			FlippedDiagonalGradient,
		}
	}
}
