using Beamable.Common.Content;
using UnityEditor;
using UnityEngine;

namespace Beamable.Server.Editor
{
	[CustomPropertyDrawer(typeof(RouteVariables))]
	public class RouteVariablesPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var variablesProperty = property.FindPropertyRelative(nameof(RouteVariables.Variables));
			if (variablesProperty.arraySize == 0)
			{
				return 0;
			}
			else if (!property.isExpanded)
			{
				return EditorGUIUtility.singleLineHeight;
			}
			else
			{
				return EditorGUIUtility.singleLineHeight * (1 + variablesProperty.arraySize) + (2 * variablesProperty.arraySize);
			}
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var variablesProperty = property.FindPropertyRelative(nameof(RouteVariables.Variables));
			if (variablesProperty.arraySize == 0)
			{
				return;
			}

			property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, "Variables",
			   new GUIStyle(EditorStyles.foldout) { font = EditorStyles.boldFont });

			if (!property.isExpanded) return;

			EditorGUI.indentLevel += 1;

			for (var i = 0; i < variablesProperty.arraySize; i++)
			{
				position = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width,
				   EditorGUIUtility.singleLineHeight);
				var elemProperty = variablesProperty.GetArrayElementAtIndex(i);
				var nameProperty = elemProperty.FindPropertyRelative(nameof(ApiVariable.Name));
				var typeProperty = elemProperty.FindPropertyRelative(nameof(ApiVariable.TypeName));
				var labelRect = EditorGUI.PrefixLabel(position, new GUIContent(typeProperty.stringValue));
				EditorGUI.SelectableLabel(labelRect, nameProperty.stringValue);
			}

			EditorGUI.indentLevel -= 1;
		}
	}
}
