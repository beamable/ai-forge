using Beamable.UI.Buss;
using Beamable.UI.Sdf;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Editor.UI.Buss
{
	[CustomEditor(typeof(SdfImage))]
	public class SDFImageInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var bussElement = ((SdfImage)serializedObject.targetObject).GetComponent<BussElement>();
			var useStyles = (bussElement != null) && bussElement.enabled;

			if (useStyles)
			{
				EditorGUILayout.HelpBox("Image properties are currently controller by BUSSElement.", MessageType.Info);
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"), new GUIContent("Mode"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Sprite"), new GUIContent("Sprite"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("colorRect"), new GUIContent("Color"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("threshold"), new GUIContent("Threshold"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("secondaryTexture"),
					new GUIContent("Background"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundMode"), new GUIContent("Background Mode"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("meshFrame"), new GUIContent("Frame"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("imageType"), new GUIContent("Type"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("rounding"),
					new GUIContent("Round Corners"));

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Outline");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("outlineWidth"),
					new GUIContent("Outline Width"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("outlineColor"),
					new GUIContent("Outline Color"));

				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Shadow");
				EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowColor"),
					new GUIContent("Shadow Color"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowThreshold"),
					new GUIContent("Shadow Threshold"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowSoftness"),
					new GUIContent("Shadow Softness"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowOffset"),
					new GUIContent("Shadow Offset"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("shadowMode"), new GUIContent("Shadow Mode"));

				if (EditorGUI.EndChangeCheck())
				{
					serializedObject.ApplyModifiedProperties();
					foreach (var sdfImage in serializedObject.targetObjects.Cast<SdfImage>())
					{
						sdfImage.Rebuild(CanvasUpdate.Layout);
					}
				}
			}
		}
	}
}
