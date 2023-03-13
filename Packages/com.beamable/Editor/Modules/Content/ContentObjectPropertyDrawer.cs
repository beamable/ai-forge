using Beamable.Common.Content;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
	[CustomPropertyDrawer(typeof(ContentObject), true)]
	public class ContentObjectPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = PropertyDrawerHelper.SetTooltipWithFallback(fieldInfo, property);

			if (Application.isPlaying)
			{
				//EditorGUILayout.LabelField("TODO: Show fields...");

				EditorGUI.PropertyField(position, property, label);
			}
			else
			{
				EditorGUI.LabelField(position, property.name, "Only Available in play mode. Use a Content Reference to set.");
			}
		}
	}
}
