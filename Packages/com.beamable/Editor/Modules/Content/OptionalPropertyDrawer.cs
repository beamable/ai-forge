using Beamable.Common.Content;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
#if !BEAMABLE_NO_OPTIONAL_DRAWERS
	[CustomPropertyDrawer(typeof(Optional), true)]
	public class OptionalPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var hasValueProp = property.FindPropertyRelative(nameof(Optional.HasValue));
			var valueProp = property.FindPropertyRelative("Value");
			if (hasValueProp.boolValue)
			{
				return EditorGUI.GetPropertyHeight(valueProp);
			}
			else
			{
				return EditorGUIUtility.singleLineHeight;
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = PropertyDrawerHelper.SetTooltipWithFallback(fieldInfo, property);

			var hasValueProp = property.FindPropertyRelative(nameof(Optional.HasValue));
			var valueProp = property.FindPropertyRelative("Value");
			var fieldRect = new Rect(position.x, position.y, position.width - 15, position.height);

			var checkRect = new Rect(position.xMax - 15, position.y, 15, EditorGUIUtility.singleLineHeight);

			if (property.isExpanded && !property.isArray)
			{
				property.isExpanded = false;
			}

			label.tooltip = PropertyDrawerHelper.SetTooltipWithFallback(fieldInfo, property);

			var labelClone = new GUIContent(label);
			EditorGUI.BeginProperty(position, label, property);

			EditorGUI.BeginChangeCheck();
			var checkStyle = new GUIStyle("toggle");
			checkStyle.alignment = TextAnchor.MiddleRight;
			var next = EditorGUI.Toggle(checkRect, hasValueProp.boolValue, checkStyle);
			if (EditorGUI.EndChangeCheck())
			{
				hasValueProp.boolValue = next;
				hasValueProp.serializedObject.ApplyModifiedProperties();
			}

			if (hasValueProp.boolValue)
			{
				var subHeight = EditorGUI.GetPropertyHeight(valueProp);
				var isPropertySingleLine = subHeight <= EditorGUIUtility.singleLineHeight;
				var rect = isPropertySingleLine ? fieldRect : position;
				EditorGUI.PropertyField(rect, valueProp, labelClone, true);
			}
			else
			{
				var wasEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUI.TextField(fieldRect, labelClone, "optional value not set");

				GUI.enabled = wasEnabled;
			}


			EditorGUI.EndProperty();

		}


	}
#endif

}
