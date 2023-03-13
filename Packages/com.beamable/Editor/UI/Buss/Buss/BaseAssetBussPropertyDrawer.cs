using Beamable.UI.Buss;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.UI.Buss
{
	[CustomPropertyDrawer(typeof(BaseAssetProperty), true)]
	public class BaseAssetBussPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var parentObject = property.GetParentObject();
			var fieldInfo = property.GetFieldInfo();
			var assetProperty = fieldInfo.GetValue(parentObject) as BaseAssetProperty;
			DrawAssetProperty(label, position.ToRectController(), assetProperty);
		}

		public static BaseAssetProperty DrawAssetProperty(GUIContent label,
														  EditorGUIRectController rc,
														  BaseAssetProperty assetProperty)
		{
			EditorGUI.LabelField(rc.ReserveSingleLine(), label);
			rc.MoveIndent(1);
			assetProperty.GenericAsset = EditorGUI.ObjectField(rc.ReserveSingleLine(), "Asset", assetProperty.GenericAsset,
															   assetProperty.GetAssetType(), false);
			rc.MoveIndent(-1);

			return assetProperty;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return GetPropertyHeight();
		}

		public static float GetPropertyHeight()
		{
			return EditorGUIUtility.singleLineHeight * 2f;
		}
	}
}
