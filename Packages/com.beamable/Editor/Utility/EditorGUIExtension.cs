using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor
{
	public static class EditorGUIExtension
	{
		public static int IntFieldWithDropdown(Rect rect, int value, int[] dropdownValues, Action<int> onValueChange, string fieldFormat = null, GUIContent[] customDropdownLabels = null)
		{
			const float dropdownButtonWidth = 15f;
			const string darkModeDropdownIcon = "d_icon dropdown";
			const string lightModeDropdownIcon = "icon dropdown";
			var rc = new EditorGUIRectController(rect);
			value = GUIIntField(rc.ReserveWidth(rect.width - dropdownButtonWidth), value, fieldFormat);
			if (GUI.Button(rc.rect,
				EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? darkModeDropdownIcon : lightModeDropdownIcon)))
			{
				var hasCustomLabels =
					customDropdownLabels != null && customDropdownLabels.Length >= dropdownValues.Length;
				var generic = new GenericMenu();
				for (var i = 0; i < dropdownValues.Length; i++)
				{
					var dropdownValue = dropdownValues[i];
					var label = hasCustomLabels ? customDropdownLabels[i] : new GUIContent(dropdownValue.ToString(fieldFormat));
					generic.AddItem(label, false,
						() => onValueChange(dropdownValue));
				}

				generic.DropDown(rect);
			}

			return value;
		}

		public static int GUIIntField(Rect rect, int value, string format = null)
		{
			var text = value.ToString(format);
			text = EditorGUI.DelayedTextField(rect, text);
			if (int.TryParse(text, out int newValue))
			{
				return newValue;
			}

			return value;
		}

		public static FieldInfo GetFieldInfo(this SerializedProperty property)
		{
			var parentType = property.serializedObject.targetObject.GetType();
			FieldInfo fieldInfo = null;
			var pathParts = property.propertyPath.Split('.');
			for (int i = 0; i < pathParts.Length; i++)
			{
				if (typeof(IEnumerable).IsAssignableFrom(parentType))
				{
					parentType = parentType?.GetElementType() ?? parentType.GetGenericArguments()[0];
					i += 1;
				}
				else
				{
					fieldInfo = parentType.FindField(pathParts[i]);
					parentType = fieldInfo?.FieldType;
				}
			}

			return fieldInfo;
		}

		public static FieldInfo FindField(this Type type, string fieldName, Type baseTypeLimit = null)
		{
			if (baseTypeLimit == null)
			{
				baseTypeLimit = typeof(object);
			}

			while (type != null)
			{
				var field = type.GetField(
					fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					return field;
				}

				if (type != baseTypeLimit)
				{
					type = type.BaseType;
				}
				else
				{
					type = null;
				}
			}

			return null;
		}

		public static Type GetParentType(this SerializedProperty property)
		{ // TODO: read arrays
			var parentType = property.serializedObject.targetObject.GetType();
			var pathParts = property.propertyPath.Split('.');
			for (int i = 0; i < pathParts.Length - 1; i++)
			{
				var fieldInfo = parentType.FindField(pathParts[i]);
				parentType = fieldInfo.FieldType;
			}

			return parentType;
		}

		public static object GetParentObject(this SerializedProperty property)
		{
			object parent = property.serializedObject.targetObject;
			var pathParts = property.propertyPath.Split('.');
			for (int i = 0; i < pathParts.Length - 1; i++)
			{
				if (parent is IEnumerable enumerable)
				{
					i++;
					var index = int.Parse(pathParts[i].Split('[', ']')[1]);
					parent = enumerable.Cast<object>().ElementAt(index);
				}
				else
				{
					var fieldInfo = parent?.GetType().FindField(pathParts[i]);
					parent = fieldInfo?.GetValue(parent);
				}
			}

			return parent;
		}

		public static EditorGUIRectController ToRectController(this Rect rect) => new EditorGUIRectController(rect);
	}
}
