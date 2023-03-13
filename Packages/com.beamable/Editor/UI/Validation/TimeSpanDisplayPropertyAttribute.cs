using Beamable.Common.Content.Validation;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.UI.Validation
{
	[CustomPropertyDrawer(typeof(TimeSpanDisplayAttribute))]
	public class TimeSpanDisplayPropertyAttribute : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight + 5;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var attr = attribute as TimeSpanDisplayAttribute;
			if (attr == null) return;

			var parts = property.propertyPath.Split('.');
			var newParts = new string[parts.Length];
			for (var i = 0; i < parts.Length - 1; i++)
			{
				newParts[i] = parts[i];
			}
			newParts[parts.Length - 1] = attr.FieldName;
			var newPath = string.Join(".", newParts);
			var prop = property.serializedObject.FindProperty(newPath);

			var labelRect = new Rect(position.x, position.y + 5, position.width,
									 EditorGUIUtility.singleLineHeight);
			if (MustBeTimeSpanDuration.TryParseTimeSpan(prop.stringValue, out var span, out var readable))
			{
				var style = EditorStyles.miniLabel;
				var content = new GUIContent("~" + readable);
				content.tooltip = $"This will cycle about every {readable}, or {span.ToString("c")}";
				EditorGUI.LabelField(labelRect, new GUIContent(" "), content, style);
			}
			else
			{
				var indentedRect = EditorGUI.IndentedRect(labelRect);
				indentedRect.width = indentedRect.width - EditorGUIUtility.labelWidth;
				indentedRect.x += EditorGUIUtility.labelWidth + 1;
				if (GUI.Button(indentedRect, "Enter a valid ISO 8601 Period Code"))
				{
					Application.OpenURL("https://en.wikipedia.org/wiki/ISO_8601#Durations");
				}
			}

		}
	}
}
