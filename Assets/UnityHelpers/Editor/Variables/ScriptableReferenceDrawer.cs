using UnityEditor;
using UnityEngine;

namespace Variables.Editor
{
	[CustomPropertyDrawer(typeof(FloatReference))]
	[CustomPropertyDrawer(typeof(IntReference))]
	public class ScriptableReferenceDrawer : PropertyDrawer
	{
		static readonly GUIStyle PopupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions")) {imagePosition = ImagePosition.ImageOnly};
		static readonly string[] Options = { "Constant", "Variable" };
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Draw label
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			EditorGUI.BeginChangeCheck();
			var useConstant = property.FindPropertyRelative("useConstant");
			var constant = property.FindPropertyRelative("constant");
			var variable = property.FindPropertyRelative("variable");

			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			Rect buttonRect = new Rect(position);
			buttonRect.yMin += PopupStyle.margin.top;
			buttonRect.width = PopupStyle.fixedWidth + PopupStyle.margin.right;
			position.xMin = buttonRect.xMax;
			int result = EditorGUI.Popup(buttonRect, useConstant.boolValue ? 0 : 1, Options, PopupStyle);

			useConstant.boolValue = result == 0;

			EditorGUI.PropertyField(position, useConstant.boolValue ? constant : variable, GUIContent.none);

			if (EditorGUI.EndChangeCheck())
				property.serializedObject.ApplyModifiedProperties();
			
			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}

	}
}
