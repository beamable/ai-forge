using Beamable.UI.Layouts;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Style
{
	[CustomPropertyDrawer(typeof(MediaQueryObject), true)]
	public class MediaQueryObjectPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var guids = BeamableAssetDatabase.FindAssets<MediaQueryObject>();
			var names = new string[guids.Length + 1];
			names[0] = "<none>";

			var mediaQueryObjects = new MediaQueryObject[guids.Length];
			var selectedIndex = 0;
			for (int i = 0; i < guids.Length; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(guids[i]);
				string name = Path.GetFileNameWithoutExtension(path);
				var mediaQueryObject = AssetDatabase.LoadAssetAtPath<MediaQueryObject>(path);
				names[i + 1] = name;
				mediaQueryObjects[i] = mediaQueryObject;

				if (mediaQueryObject == property.objectReferenceValue)
				{
					selectedIndex = i + 1;
				}

			}

			var output = EditorGUI.Popup(position, fieldInfo.Name, selectedIndex, names.ToArray());
			if (output != selectedIndex)
			{
				property.objectReferenceValue = output == 0 ? null : mediaQueryObjects[output - 1];
			}
		}
	}
}
