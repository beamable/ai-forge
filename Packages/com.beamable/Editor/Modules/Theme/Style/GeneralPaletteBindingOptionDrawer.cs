using Beamable.Theme;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Style
{
	[CustomPropertyDrawer(typeof(GeneralPaletteBinding), true)]
	public class GeneralPaletteBindingOptionDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var selector = ThemeConfiguration.Instance;
			var names = ThemeConfiguration.Instance.Style.GetPaletteStyleNames(fieldInfo.FieldType);
			var prop = property.FindPropertyRelative(nameof(GeneralPaletteBinding.Name));
			var index = names.ToList().IndexOf(prop.stringValue) + 1;
			names.Insert(0, GeneralPaletteBinding.NAME_NONE);
			var output = EditorGUI.Popup(position, fieldInfo.Name, index, names.ToArray());
			if (output != index)
			{
				prop.stringValue = names[output];
			}
		}
	}
}
