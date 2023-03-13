using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.UI.Buss
{
	[CustomPropertyDrawer(typeof(BussIdAttribute))]
	[CustomPropertyDrawer(typeof(BussClassAttribute))]
	public class BussNamePropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var value = property.stringValue;
			EditorGUI.BeginChangeCheck();
			var newValue = EditorGUI.DelayedTextField(position, label, GetFormatted(value));
			if (EditorGUI.EndChangeCheck())
			{
				property.stringValue = Cleaned(newValue);
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		private string GetFormatted(string input)
		{
			if (attribute is BussIdAttribute)
			{
				return BussNameUtility.AsIdSelector(input);
			}

			if (attribute is BussClassAttribute)
			{
				return BussNameUtility.AsClassSelector(input);
			}

			return Cleaned(input);
		}

		private string Cleaned(string input)
		{
			return BussNameUtility.CleanString(input);
		}
	}
}
