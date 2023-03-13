using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Editor.Environment;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{
	[CustomPropertyDrawer(typeof(HideUnlessServerPackageInstalled))]
	public class HideServerPropertyDrawer : PropertyDrawer
	{
		private static Promise<bool> _check;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (!CanDisplay()) return 0;

			var drawer = PropertyDrawerFinder.FindDrawerForProperty(property, typeof(HideServerPropertyDrawer));
			if (drawer == null)
			{
				return EditorGUI.GetPropertyHeight(property);
			}
			else
			{
				return drawer.GetPropertyHeight(property, label);
			}

		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (!CanDisplay()) return;

			var isOptional = (typeof(Optional).IsAssignableFrom(fieldInfo.FieldType));
			if (isOptional)
			{
				var optionalDrawer = new OptionalPropertyDrawer();
				optionalDrawer.OnGUI(position, property, label);
				return;
			}

			var drawer = PropertyDrawerFinder.FindDrawerForProperty(property);
			if (drawer == null)
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
			else
			{
				var height = drawer.GetPropertyHeight(property, label);
				position.height = height;
				drawer.OnGUI(position, property, label);
			}
		}

		private bool CanDisplay()
		{
			if (_check == null)
			{
				_check = HasMicroservicePackage();
			}

			if (_check.IsCompleted)
			{
				return _check.GetResult();
			}

			return false;
		}

		private async Promise<bool> HasMicroservicePackage()
		{
			var hasPackage = false;
			try
			{
				var result = await BeamablePackages.ServerPackageMeta;
				hasPackage = result.IsPackageAvailable;
			}
			catch
			{
				// its okay, don't do anything.
			}

			return hasPackage;
		}
	}
}
