using Beamable.Api.Sessions;
using Beamable.Editor.Content;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEditor.Experimental;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Modules.Sessions
{
	[CustomPropertyDrawer(typeof(SessionOption), true)]
	public class DeviceOptionPropertyDrawer
	   :
#if UNITY_2019_1_OR_NEWER
      PropertyDrawer
#else
	  UIElementsPropertyDrawer
#endif
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var subProperty = property.FindPropertyRelative(nameof(SessionOption.UserEnabled));

			var obj = ContentRefPropertyDrawer.GetTargetObjectOfProperty(property);
			var option = obj as SessionOption;

			if (option?.ForceEnabled ?? false)
			{
				return;
			}

			EditorGUI.PropertyField(position, subProperty, label);
		}

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var subProperty = property.FindPropertyRelative(nameof(SessionOption.UserEnabled));
			var field = new PropertyField(subProperty, property.displayName);

			var obj = ContentRefPropertyDrawer.GetTargetObjectOfProperty(property);
			var option = obj as SessionOption;

			if (option?.ForceEnabled ?? false)
			{
				return new VisualElement(); // nothing to do; its always enabled
			}

			return field;
		}
	}
}
