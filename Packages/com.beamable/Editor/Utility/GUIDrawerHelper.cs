using Beamable.Editor.UI.Buss;
using Beamable.UI.Buss;
using Beamable.UI.Sdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Beamable.Editor
{
	public static class GUIDrawerHelper
	{
		public static void DrawField(EditorGUIRectController rc, object target, FieldInfo fieldInfo, Dictionary<string, object> drawerData, string path)
		{
			if (fieldInfo.GetCustomAttribute<NonSerializedAttribute>() != null) return;
			if (!fieldInfo.IsPublic && fieldInfo.GetCustomAttribute<SerializeField>() == null) return;
			var fieldLabel = fieldInfo.Name;
			var value = fieldInfo.GetValue(target);
			var delayed = fieldInfo.GetCustomAttribute<DelayedAttribute>() != null;

			value = DrawObject(rc, new GUIContent(fieldLabel), value, drawerData, $"{path}.{fieldLabel}", delayed, fieldInfo.FieldType);
			fieldInfo.SetValue(target, value);
		}

		public static object DrawObject(EditorGUIRectController rc, GUIContent label, object value, Dictionary<string, object> drawerData, string path = "", bool delayed = false, Type enforcedType = null)
		{
			switch (value)
			{
				// --- Simple types
				case int i when delayed:
					return EditorGUI.DelayedIntField(rc.ReserveSingleLine(), label, i);
				case int i:
					return EditorGUI.IntField(rc.ReserveSingleLine(), label, i);
				case float f when delayed:
					return EditorGUI.DelayedFloatField(rc.ReserveSingleLine(), label, f);
				case float f:
					return EditorGUI.FloatField(rc.ReserveSingleLine(), label, f);
				case bool b:
					return EditorGUI.Toggle(rc.ReserveSingleLine(), label, b);
				case string s when delayed:
					return EditorGUI.DelayedTextField(rc.ReserveSingleLine(), label, s);
				case string s:
					return EditorGUI.TextField(rc.ReserveSingleLine(), label, s);
				case Color color:
					return EditorGUI.ColorField(rc.ReserveSingleLine(), label, color);
				case Vector2 v2:
					return EditorGUI.Vector2Field(rc.ReserveSingleLine(), label, v2);
				case Vector3 v3:
					return EditorGUI.Vector3Field(rc.ReserveSingleLine(), label, v3);
				case Vector4 v4:
					return EditorGUI.Vector4Field(rc.ReserveSingleLine(), label, v4);
				case Vector2Int v2:
					return EditorGUI.Vector2IntField(rc.ReserveSingleLine(), label, v2);
				case Vector3Int v3:
					return EditorGUI.Vector3IntField(rc.ReserveSingleLine(), label, v3);
				case Enum e:
					return EditorGUI.EnumPopup(rc.ReserveSingleLine(), label, e);

				// --- Arrays or lists
				case Array array:
					if (DrawFoldout(rc, label, drawerData, path))
					{
						rc.MoveIndent(1);
						for (int index = 0; index < array.Length; index++)
						{
							var elementPath = $"{path}_{index}";
							array.SetValue(DrawObject(rc, new GUIContent($"Element {index}"), array.GetValue(index),
								drawerData, elementPath, delayed), index);
						}
						rc.MoveIndent(-1);
					}
					return array;

				case IList list:
					if (DrawFoldout(rc, label, drawerData, path))
					{
						rc.MoveIndent(1);
						for (int index = 0; index < list.Count; index++)
						{
							var elementPath = $"{path}_{index}";
							list[index] = DrawObject(rc, new GUIContent($"Element {index++}"), list[index],
								drawerData, elementPath, delayed);
						}
						rc.MoveIndent(-1);
					}
					return list;

				// --- Types with custom drawers
				case ColorRect colorRect:
					var colorRectDrawer = GetDrawer<ColorRectDrawer>(path, drawerData);
					return colorRectDrawer.DrawColorRect(label, rc, colorRect);
				case BaseAssetProperty assetProperty:
					return BaseAssetBussPropertyDrawer.DrawAssetProperty(label, rc, assetProperty);
				// --- Unity Objects
				case Object obj:
					return EditorGUI.ObjectField(rc.ReserveSingleLine(), label, obj, enforcedType ?? typeof(Object), false);
				case null when enforcedType != null && enforcedType.IsSubclassOf(typeof(Object)):
					return EditorGUI.ObjectField(rc.ReserveSingleLine(), label, null, enforcedType ?? typeof(Object), false);
				// --- Other objects
				case null:
					EditorGUI.LabelField(rc.ReserveSingleLine(), label);
					return null;
				default:
					if (DrawFoldout(rc, label, drawerData, path))
					{
						rc.MoveIndent(1);
						foreach (var fieldInfo in value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy))
						{
							DrawField(rc, value, fieldInfo, drawerData, path);
						}
						rc.MoveIndent(-1);
					}
					return value;
			}
		}

		private static bool DrawFoldout(EditorGUIRectController rc, GUIContent label, Dictionary<string, object> drawerData, string path)
		{
			path += "_foldout";
			var expanded = false;
			if (drawerData.ContainsKey(path))
			{
				expanded = (bool)drawerData[path];
			}

			expanded = EditorGUI.Foldout(rc.ReserveSingleLine(), expanded, label);
			drawerData[path] = expanded;
			return expanded;
		}

		private static T GetDrawer<T>(string path, Dictionary<string, object> drawerData) where T : new()
		{
			if (drawerData.ContainsKey(path))
			{
				return (T)drawerData[path];
			}

			var newT = new T();
			drawerData[path] = newT;
			return new T();
		}
	}
}
