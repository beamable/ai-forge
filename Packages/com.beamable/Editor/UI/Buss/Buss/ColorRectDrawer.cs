using Beamable.UI.Sdf;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.UI.Buss
{
	[CustomPropertyDrawer(typeof(ColorRect))]
	public class ColorRectDrawer : PropertyDrawer
	{
		private static readonly FieldInfo _drawerModeField =
			typeof(ColorRect).GetField("_drawerMode", BindingFlags.Instance | BindingFlags.NonPublic);

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetHeight((Mode)property.FindPropertyRelative("_drawerMode").intValue);
		}

		public float GetHeight(Mode mode)
		{
			return EditorGUIUtility.singleLineHeight * (mode == Mode.SingleColor ? 2 : 3);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var rc = new EditorGUIRectController(position);

			var bottomLeft = property.FindPropertyRelative("BottomLeftColor");
			var bottomRight = property.FindPropertyRelative("BottomRightColor");
			var topLeft = property.FindPropertyRelative("TopLeftColor");
			var topRight = property.FindPropertyRelative("TopRightColor");

			var colorRect = new ColorRect(
				bottomLeft.colorValue, bottomRight.colorValue,
				topLeft.colorValue, topRight.colorValue);

			EditorGUI.BeginChangeCheck();

			colorRect = DrawColorRect(label, rc, colorRect, property);

			if (EditorGUI.EndChangeCheck())
			{
				bottomLeft.colorValue = colorRect.BottomLeftColor;
				bottomRight.colorValue = colorRect.BottomRightColor;
				topLeft.colorValue = colorRect.TopLeftColor;
				topRight.colorValue = colorRect.TopRightColor;

				property.serializedObject.ApplyModifiedProperties();
			}
		}

		public ColorRect DrawColorRect(GUIContent label, EditorGUIRectController rc, ColorRect colorRect, SerializedProperty property = null)
		{
			var mode = property == null ? (Mode)_drawerModeField.GetValue(colorRect) : (Mode)property.FindPropertyRelative("_drawerMode").intValue;
			var newMode = (Mode)EditorGUI.EnumPopup(rc.ReserveSingleLine(), label, mode);

			if (newMode != mode)
			{
				mode = newMode;
				if (property != null)
				{
					property.FindPropertyRelative("_drawerMode").intValue = (int)mode;
				}
				// Using boxing here, it makes SetValue work for struct.
				object boxed = colorRect;
				_drawerModeField.SetValue(boxed, (int)mode);
				colorRect = (ColorRect)boxed;
			}

			rc.MoveIndent(1);

			colorRect = DrawColorFields(colorRect, rc, mode);

			rc.MoveIndent(-1);
			return colorRect;
		}

		private ColorRect DrawColorFields(ColorRect colorRect, EditorGUIRectController rc, Mode mode)
		{
			switch (mode)
			{
				case Mode.SingleColor:
					colorRect.BottomLeftColor = colorRect.BottomRightColor = colorRect.TopLeftColor = colorRect.TopRightColor =
						EditorGUI.ColorField(rc.ReserveSingleLine(), "Color", colorRect.TopLeftColor);
					break;
				case Mode.HorizontalGradient:
					colorRect.BottomLeftColor = colorRect.TopLeftColor =
						EditorGUI.ColorField(rc.ReserveSingleLine(), "Left", colorRect.TopLeftColor);
					colorRect.BottomRightColor = colorRect.TopRightColor =
						EditorGUI.ColorField(rc.ReserveSingleLine(), "Right", colorRect.TopRightColor);
					break;
				case Mode.VerticalGradient:
					colorRect.TopLeftColor = colorRect.TopRightColor =
						EditorGUI.ColorField(rc.ReserveSingleLine(), "Top", colorRect.TopLeftColor);
					colorRect.BottomLeftColor = colorRect.BottomRightColor =
						EditorGUI.ColorField(rc.ReserveSingleLine(), "Bottom", colorRect.BottomLeftColor);
					break;
				case Mode.DiagonalGradient:
					colorRect.TopLeftColor =
						EditorGUI.ColorField(rc.ReserveSingleLine(), "Start", colorRect.TopLeftColor);
					colorRect.BottomRightColor =
						EditorGUI.ColorField(rc.ReserveSingleLine(), "End", colorRect.BottomRightColor);
					colorRect.TopRightColor = colorRect.BottomLeftColor =
						Color.Lerp(colorRect.TopLeftColor, colorRect.BottomRightColor, .5f);
					break;
				case Mode.FlippedDiagonalGradient:
					colorRect.BottomLeftColor =
						EditorGUI.ColorField(rc.ReserveSingleLine(), "Start", colorRect.BottomLeftColor);
					colorRect.TopRightColor =
						EditorGUI.ColorField(rc.ReserveSingleLine(), "End", colorRect.TopRightColor);
					colorRect.BottomRightColor = colorRect.TopLeftColor =
						Color.Lerp(colorRect.BottomLeftColor, colorRect.TopRightColor, .5f);
					break;
				case Mode.PerVertexColor:
					var topRow = rc.ReserveSingleLine().ToRectController();
					var bottomRow = rc.ReserveSingleLine().ToRectController();
					colorRect.TopLeftColor =
						EditorGUI.ColorField(topRow.ReserveWidthByFraction(.5f), colorRect.TopLeftColor);
					colorRect.TopRightColor =
						EditorGUI.ColorField(topRow.rect, colorRect.TopRightColor);
					colorRect.BottomLeftColor =
						EditorGUI.ColorField(bottomRow.ReserveWidthByFraction(.5f), colorRect.BottomLeftColor);
					colorRect.BottomRightColor =
						EditorGUI.ColorField(bottomRow.rect, colorRect.BottomRightColor);
					break;
			}

			return colorRect;
		}

		public enum Mode
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
