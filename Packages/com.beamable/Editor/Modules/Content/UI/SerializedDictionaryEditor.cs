using Beamable.Common.Content;
using Beamable.Player;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content.UI
{

#if !BEAMABLE_NO_DICT_DRAWERS
	[CustomPropertyDrawer(typeof(SerializableDictionaryStringToSomething<>), true)]
#endif
	public class SerializedDictionaryStringToSomethingEditor2 : PropertyDrawer
	{
		private static Dictionary<Type, Func<object>> _typeToDefaultInstance = new Dictionary<Type, Func<object>>()
		{
			[typeof(int)] = () => default(int),
			[typeof(bool)] = () => default(bool),
			[typeof(long)] = () => default(long),
			[typeof(short)] = () => default(short),
			[typeof(byte)] = () => default(byte),
			[typeof(string)] = () => "",
			[typeof(double)] = () => default(double),
			[typeof(float)] = () => default(float),
			[typeof(ulong)] = () => default(ulong),
			[typeof(ushort)] = () => default(ushort),
			[typeof(uint)] = () => default(uint),
			[typeof(sbyte)] = () => default(sbyte),
		};

		private string _addKey;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var target =
				ContentRefPropertyDrawer.GetTargetObjectOfProperty(property) as IDictionaryWithValue;
			if (target == null) return EditorGUIUtility.singleLineHeight * 3;

			if (!property.isExpanded)
				return EditorGUIUtility.singleLineHeight;

			var valueProps = property.FindPropertyRelative("values");
			Event e = Event.current;

			var accumulatedHeight = 0f;
			for (var i = 0; i < valueProps.arraySize; i++)
			{
				var valueProp = valueProps.GetArrayElementAtIndex(i);
				var elementHeight = EditorGUI.GetPropertyHeight(valueProp, true);
				accumulatedHeight += elementHeight;
			}

			return (EditorGUIUtility.singleLineHeight * 3) + accumulatedHeight; // the title, labels, buttons, and rows.
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var target =
				ContentRefPropertyDrawer.GetTargetObjectOfProperty(property) as IDictionaryWithValue;
			if (target == null) return;

			var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			var nextFoldout = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
			property.isExpanded = nextFoldout;

			if (!nextFoldout) return;

			var labelRect = new Rect(position.x + 10, foldoutRect.yMax, position.width - 10, foldoutRect.height);
			EditorGUI.LabelField(labelRect, "(keys)", "(values)");

			var kvpY = labelRect.yMax;

			var keyProps = property.FindPropertyRelative("keys");
			var valueProps = property.FindPropertyRelative("values");
			Event e = Event.current;
			GenericMenu context = null;
			SerializedProperty contextKeyProp = null;

			for (var i = 0; i < keyProps.arraySize && i < valueProps.arraySize; i++)
			{
				var keyProp = keyProps.GetArrayElementAtIndex(i);
				var valueProp = valueProps.GetArrayElementAtIndex(i);

				EditorGUI.BeginChangeCheck();

				var kvpRect = new Rect(labelRect.x, kvpY, labelRect.width - 20, EditorGUIUtility.singleLineHeight);

				kvpY += EditorGUI.GetPropertyHeight(valueProp, true);

				var key = keyProp.stringValue;
				EditorGUI.PropertyField(kvpRect, valueProp, new GUIContent(key), true);

				var xButtonRect = new Rect(kvpRect.xMax, kvpRect.yMin, 18, kvpRect.height);
				var shouldDeleteKey = GUI.Button(xButtonRect, "X");

				if (e.type == EventType.MouseDown && e.button == 1 && kvpRect.Contains(e.mousePosition))
				{

					contextKeyProp = keyProp;
					context = new GenericMenu();

					context.AddItem(new GUIContent("Remove"), false, Delete);
					context.AddItem(new GUIContent("Duplicate"), false, () =>
					{
						// TODO: create a better key...
						var count = 1;
						var nextKey = keyProp.stringValue;
						while (target.Contains(nextKey + count))
						{
							count++;
						}

						AddKey(nextKey + count, ContentRefPropertyDrawer.GetTargetObjectOfProperty(valueProp));
					});
					e.Use();
				}

				void Delete()
				{
					Undo.RecordObjects(property.serializedObject.targetObjects, $"Remove {key} property");
					target.Remove(key);

					MarkDirty(property);
				}

				if (shouldDeleteKey)
				{
					Delete();
					break;
				}
			}


			var newKeyRect = new Rect(labelRect.x, kvpY, labelRect.width - 84, EditorGUIUtility.singleLineHeight);
			_addKey = EditorGUI.TextField(newKeyRect, " ", _addKey);
			var isValidKey = !string.IsNullOrEmpty(_addKey) && !target.Contains(_addKey);
			var wasEnabled = GUI.enabled;
			GUI.enabled = isValidKey;

			if (context != null)
			{
				if (isValidKey)
				{
					context.AddItem(new GUIContent("Rename"), false, (data) =>
					{
						var castData = (ValueTuple<string, SerializedProperty>)data;
						castData.Item2.stringValue = castData.Item1;
						property.serializedObject.ApplyModifiedProperties();
						MarkDirty(property);
					}, (_addKey, contextKeyProp));
				}
				else
				{
					context.AddDisabledItem(
						new GUIContent("Rename", "use the AddKey text input to create a valid key first"));
				}

				context.ShowAsContext();
			}

			var addKeyButtonRect = new Rect(newKeyRect.xMax + 2, kvpY, 80, newKeyRect.height);
			if (GUI.Button(addKeyButtonRect, "Add Key"))
			{
				AddKey(_addKey);
			}

			GUI.enabled = wasEnabled;


			void AddKey(string key, object value = null)
			{
				value = value ?? GetDefaultValue(target.ValueType);
				Undo.RecordObjects(property.serializedObject.targetObjects, $"Add {key} property");
				target.Add(key, value);
				_addKey = null;
				MarkDirty(property);
			}
		}

		/// <summary>
		/// Get the default instance for some type.
		/// We'll use the Unity serialization to get the default instance by letting Unity try to deserialize an empty instance of the object.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		object GetDefaultValue(Type type)
		{
			// there may be primitive types Unity can't handle :/
			if (_typeToDefaultInstance.TryGetValue(type, out var generator))
			{
				return generator();
			}

			return JsonUtility.FromJson("{}", type);
		}

		void MarkDirty(SerializedProperty prop)
		{
			EditorUtility.SetDirty(prop.serializedObject.targetObject);
			if (prop.serializedObject.targetObject is ContentObject contentObject)
			{
				contentObject.ForceValidate();
			}
		}
	}
}
