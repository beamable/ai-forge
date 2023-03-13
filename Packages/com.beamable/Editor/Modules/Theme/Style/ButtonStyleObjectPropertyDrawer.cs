using Beamable.Theme.Palettes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Editor.Style
{
	[CustomPropertyDrawer(typeof(ButtonStyleData), true)]
	public class ButtonStyleObjectPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var transitionProperty = property.FindPropertyRelative(nameof(ButtonStyleData.Transition));

			var height = EditorGUI.GetPropertyHeight(transitionProperty);

			switch (transitionProperty.enumNames[transitionProperty.enumValueIndex])
			{
				case nameof(Selectable.Transition.ColorTint):
					height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(ButtonStyleData.Colors)));
					break;
				case nameof(Selectable.Transition.SpriteSwap):
					height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(ButtonStyleData.SpriteState)));
					break;
				case nameof(Selectable.Transition.Animation):
					height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(nameof(ButtonStyleData.AnimationTriggers)));
					break;
			}

			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var transitionProperty = property.FindPropertyRelative(nameof(ButtonStyleData.Transition));
			EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, 10), transitionProperty, new GUIContent("Transition Type"));

			var height = 17f;

			var rect = new Rect(position);
			rect.y += height;

			switch (transitionProperty.enumNames[transitionProperty.enumValueIndex])
			{
				case nameof(Selectable.Transition.ColorTint):
					EditorGUI.PropertyField(rect, property.FindPropertyRelative(nameof(ButtonStyleData.Colors)), true);
					break;
				case nameof(Selectable.Transition.SpriteSwap):
					EditorGUI.PropertyField(rect, property.FindPropertyRelative(nameof(ButtonStyleData.SpriteState)));
					break;
				case nameof(Selectable.Transition.Animation):
					EditorGUI.PropertyField(rect, property.FindPropertyRelative(nameof(ButtonStyleData.AnimationTriggers)));
					break;
			}

			EditorGUI.EndProperty();
		}
	}
}
