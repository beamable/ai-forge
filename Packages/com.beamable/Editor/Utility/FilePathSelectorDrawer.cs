using Beamable.Common.Content;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor
{
	[CustomPropertyDrawer(typeof(FilePathSelectorAttribute))]
	public class FilePathSelectorDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var rc = position.ToRectController();
			EditorGUI.LabelField(rc.ReserveLabelRect(), label);
			EditorGUI.BeginChangeCheck();
			var editedPath = EditorGUI.DelayedTextField(rc.ReserveWidth(rc.rect.width - 60f), property.stringValue);
			if (EditorGUI.EndChangeCheck())
			{
				ApplyChanges(property, editedPath, attribute as FilePathSelectorAttribute);
			}

			if (GUI.Button(rc.rect, "Open..."))
			{
				var newPath =
					GetPathFromDialogWindow(property.stringValue, attribute as FilePathSelectorAttribute);
				if (!string.IsNullOrWhiteSpace(newPath))
				{
					ApplyChanges(property, newPath, attribute as FilePathSelectorAttribute);
				}
			}
		}

		private string GetPathFromDialogWindow(string currentPath, FilePathSelectorAttribute attribute)
		{
			string path = null;
			var openPath = currentPath;
			if (string.IsNullOrWhiteSpace(currentPath))
			{
				openPath = attribute.RootFolder;
			}

			if (attribute.OnlyFiles)
			{
				path = EditorUtility.OpenFilePanel(attribute.DialogTitle, openPath, attribute.FileExtension);
			}
			else
			{
				path = EditorUtility.OpenFolderPanel(attribute.DialogTitle, openPath, null);
			}

			if (!string.IsNullOrWhiteSpace(attribute.PathRelativeTo)
				&& Uri.IsWellFormedUriString(attribute.PathRelativeTo, UriKind.RelativeOrAbsolute)
				&& Uri.IsWellFormedUriString(path, UriKind.RelativeOrAbsolute))
			{
				var rootUri = new Uri(attribute.PathRelativeTo);
				var fullUri = new Uri(path);
				path = rootUri.MakeRelativeUri(fullUri).ToString();
			}

			return path;
		}

		private void ApplyChanges(SerializedProperty property, string newPath, FilePathSelectorAttribute attribute)
		{
			if (string.IsNullOrEmpty(attribute.PathRelativeTo))
			{
				property.stringValue = string.IsNullOrEmpty(newPath) ? newPath : Path.GetFullPath(newPath);
				property.serializedObject.ApplyModifiedProperties();
			}
			else
			{
				if (Uri.IsWellFormedUriString(newPath, UriKind.RelativeOrAbsolute))
				{
					property.stringValue = newPath;
					property.serializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}
