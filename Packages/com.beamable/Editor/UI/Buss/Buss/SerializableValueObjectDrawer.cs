using Beamable.UI.Buss;
using Beamable.UI.Sdf;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.UI.Buss
{
	[CustomPropertyDrawer(typeof(SerializableValueImplementsAttribute))]
	[CustomPropertyDrawer(typeof(SerializableValueObject))]
	public class SerializableValueObjectDrawer : PropertyDrawer
	{
		public Type baseTypeOverride;
		private static readonly Dictionary<string, object> _drawerData = new Dictionary<string, object>();
		private readonly Dictionary<string, float> _heightCache = new Dictionary<string, float>();

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var rc = new EditorGUIRectController(position);

			var typeProperty = property.FindPropertyRelative("type");
			var jsonProperty = property.FindPropertyRelative("json");
			var type = typeProperty.stringValue;
			var json = jsonProperty.stringValue;

			var hasChange = false;

			Type sysType = null;
			object value = null;

			var implementsAtt = (SerializableValueImplementsAttribute)attribute;
			if (baseTypeOverride != null || implementsAtt != null)
			{
				var data = SerializableValueImplementationHelper.GetWithSpecialRule("withVariableProperty",
					baseTypeOverride != null ? baseTypeOverride : implementsAtt.baseType, typeof(VariableProperty));
				if (data != null)
				{
					var dropdownRect = position;
					dropdownRect.x += EditorGUIUtility.labelWidth;
					dropdownRect.width -= EditorGUIUtility.labelWidth;
					dropdownRect.height = EditorGUIUtility.singleLineHeight;
					var types = data.subTypes;
					var dropdownIndex = Array.IndexOf(types, Type.GetType(type));
					var newIndex = EditorGUI.Popup(dropdownRect, dropdownIndex, data.labels);
					if (dropdownIndex != newIndex && newIndex != -1)
					{
						hasChange = true;
						sysType = types[newIndex];
						type = sysType?.AssemblyQualifiedName;
						if (sysType == null)
						{
							json = null;
						}
						else
						{
							value = Activator.CreateInstance(sysType);
							json = JsonUtility.ToJson(value);
						}
					}
				}
			}

			sysType = type == null ? null : Type.GetType(type);
			var valueField =
				typeof(SerializableValueObject).GetField("value", BindingFlags.Instance | BindingFlags.NonPublic);
			var obj = property?.GetFieldInfo()?.GetValue(property?.GetParentObject());
			if (value == null && obj != null)
			{
				value = valueField.GetValue(obj);
			}
			EditorGUI.BeginChangeCheck();
			value = GUIDrawerHelper.DrawObject(rc, label, value, _drawerData,
				property.serializedObject.targetObject.GetInstanceID() + ":" + property.propertyPath);
			hasChange |= EditorGUI.EndChangeCheck();

			if (hasChange)
			{
				typeProperty.stringValue = sysType?.AssemblyQualifiedName;
				jsonProperty.stringValue = value != null ? JsonUtility.ToJson(value) : null;
				property.serializedObject.ApplyModifiedProperties();
				property.serializedObject.Update();
			}

			_heightCache[property.propertyPath] = position.height - rc.rect.height;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (_heightCache.TryGetValue(property.propertyPath, out var he)) return he;
			return EditorGUIUtility.singleLineHeight;
		}
	}
}
