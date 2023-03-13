using Beamable.Common.Content;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor
{
	[CustomPropertyDrawer(typeof(ReadonlyIfAttribute))]
	public class ReadonlyIfDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{

			var attribute = this.attribute as ReadonlyIfAttribute;
			var conditionPath = attribute.conditionPath;
			var parent = property.GetParentObject();
			var parentType = parent.GetType();
			var conditionProperty = parentType.GetProperty(conditionPath);
			var condition = false;
			if (conditionProperty != null)
			{
				condition = (bool)conditionProperty.GetValue(parent);
			}
			else
			{
				var conditionField = parentType.GetField(conditionPath);
				if (conditionField != null)
				{
					condition = (bool)conditionField.GetValue(parent);
				}
			}

			if (attribute.negate)
			{
				condition = !condition;
			}

			EditorGUI.BeginDisabledGroup(condition);
			Draw(position, property, label, attribute.specialDrawer);
			EditorGUI.EndDisabledGroup();
		}

		private void Draw(Rect position, SerializedProperty property, GUIContent label,
			ReadonlyIfAttribute.SpecialDrawer specialDrawer)
		{
			switch (specialDrawer)
			{
				case ReadonlyIfAttribute.SpecialDrawer.DelayedString:
					EditorGUI.DelayedTextField(position, property, label);
					break;
				default:
					EditorGUI.PropertyField(position, property, label);
					break;
			}
		}
	}
}
