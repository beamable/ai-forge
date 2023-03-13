using Beamable.Theme;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Modules.Theme
{

	[CustomPropertyDrawer(typeof(CanHideAttribute), true)]
	public class CanHideDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property,
		   GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

		public override void OnGUI(Rect position,
		   SerializedProperty property,
		   GUIContent label)
		{

			if (CanHideAttribute.Hide)
			{
				GUI.enabled = false;
				EditorGUI.PropertyField(position, property, label, true);
				GUI.enabled = true;
			}
			else
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
		}
	}
}
