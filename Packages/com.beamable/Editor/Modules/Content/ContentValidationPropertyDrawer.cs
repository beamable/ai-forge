using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beamable.Editor.Content
{

#if !BEAMABLE_NO_VALIDATION_DRAWERS
	[CustomPropertyDrawer(typeof(ValidationAttribute), true)]
#endif
	public class ContentValidationPropertyDrawer : PropertyDrawer
	{
		private GUIStyle _lblStyle;
		private const int WIDTH = 3;
		private const int OFFSET = -10;


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			label.tooltip = PropertyDrawerHelper.SetTooltipWithFallback(fieldInfo, property);

			var baseHeight = RefEditorGUI.DefaultPropertyHeight(property, label);
			//         var baseHeight = EditorGUI.GetPropertyHeight(property, label);

			if (property.serializedObject.isEditingMultipleObjects || !BeamEditor.IsInitialized)
			{
				return baseHeight;
			}
			var ctx = BeamEditorContext.Default.ContentIO.GetValidationContext();

			var attributes = fieldInfo.GetCustomAttributes<ValidationAttribute>();
			var contentObj = property.serializedObject.targetObject as ContentObject;

			var exceptions = new List<ContentException>();
			var isArray = TryGetArrayIndex(property, out var arrayIndex);

			var newlineCount = 0;

			if (ctx.Initialized)
			{
				foreach (var attribute in attributes)
				{
					try
					{
						var value = ContentRefPropertyDrawer.GetTargetParentObjectOfProperty(property);
						var wrapper = new ValidationFieldWrapper(fieldInfo, value);
						attribute.Validate(ContentValidationArgs.Create(wrapper, contentObj, ctx, arrayIndex, isArray));
					}
					catch (ContentException ex)
					{
						newlineCount += 1 + ex.FriendlyMessage.Count(c => c == '\n');
						exceptions.Add(ex);
					}
				}
			}

			return baseHeight + EditorGUIUtility.singleLineHeight * newlineCount;
		}

		protected bool TryGetArrayIndex(SerializedProperty property, out int arrayIndex)
		{
			arrayIndex = 0;

			var rightBracketIndex = property.propertyPath.LastIndexOf(']');
			if (rightBracketIndex == property.propertyPath.Length - 1)
			{
				var leftBracketIndex = property.propertyPath.LastIndexOf('[');
				if (leftBracketIndex > 0 &&
					int.TryParse(property.propertyPath.Substring(leftBracketIndex + 1,
					   (rightBracketIndex - leftBracketIndex) - 1), out var index))
				{
					arrayIndex = index;
					return true;
				}
			}
			return false;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			label.tooltip = PropertyDrawerHelper.SetTooltipWithFallback(fieldInfo, property);

			if (property.serializedObject.isEditingMultipleObjects || !BeamEditor.IsInitialized)
			{
				RefEditorGUI.DefaultPropertyField(position, property, label);
				return; // don't support multiple edit.
			}

			var ctx = BeamEditorContext.Default.ContentIO.GetValidationContext();

			var parentValue = ContentRefPropertyDrawer.GetTargetParentObjectOfProperty(property);
			var value = ContentRefPropertyDrawer.GetTargetObjectOfProperty(property);
			//
			if (property.propertyPath.Contains("description"))
			{

			}
			//
			//         if (property.propertyPath.Contains("title"))
			//         {
			//
			//         }
			//
			//         // if the property is an optional, we need to forward the Value, or don't do anything...
			//         var baseProperty = property.Copy();
			//         if (typeof(DisplayableStringCollection).IsAssignableFrom(value.GetType()))
			//         {
			//            var valueProperty = baseProperty.FindPropertyRelative("rawData");
			//
			//            var propType = valueProperty.type;
			//            var actualValue = ContentRefPropertyDrawer.GetTargetObjectOfProperty(valueProperty);
			//
			//
			//         }


			var attributes = fieldInfo.GetCustomAttributes<ValidationAttribute>();
			var contentObj = property.serializedObject.targetObject as ContentObject;

			var isArray = TryGetArrayIndex(property, out var arrayIndex);
			var exceptions = new List<ContentException>();
			if (ctx.Initialized)
			{
				foreach (var attribute in attributes)
				{
					try
					{
						var wrapper = new ValidationFieldWrapper(fieldInfo, parentValue);
						attribute.Validate(ContentValidationArgs.Create(wrapper, contentObj, ctx, arrayIndex, isArray));
					}
					catch (ContentException ex)
					{
						exceptions.Add(ex);
					}
				}
			}

			RefEditorGUI.DefaultPropertyField(position, property, label);

			if (exceptions.Count > 0)
			{

				var maxY = 0f;
				if (_lblStyle == null)
				{
					_lblStyle = new GUIStyle(GUI.skin.label);
					_lblStyle.fontSize = (int)(_lblStyle.fontSize * .7f);
					_lblStyle.normal.textColor = Color.red;
					_lblStyle.hover.textColor = Color.red;
				}

				for (var i = 0; i < exceptions.Count; i++)
				{
					var ex = exceptions[i];
					var content = new GUIContent($"  {ex.FriendlyMessage}");
					var newlineCount = ex.FriendlyMessage.Count(c => c == '\n');
					EditorGUI.LabelField(new Rect(position.x, position.y + position.height + EditorGUIUtility.singleLineHeight * (i - (newlineCount + 1)), position.width, EditorGUIUtility.singleLineHeight * (newlineCount + 1)), content, _lblStyle);
					//EditorGUILayout.LabelField(content, _lblStyle);

					//maxY += _lblStyle.CalcSize(content).y;
				}
				var errRect = new Rect(position.x - WIDTH + OFFSET, position.y - 1, WIDTH, position.height + maxY + 2);

				EditorGUI.DrawRect(errRect, Color.red);
				// EditorGUI.
			}

			//         EditorGUI.EndProperty();
		}
	}

	public static class RefEditorGUI
	{
		public delegate bool DefaultPropertyFieldDelegate(Rect position, SerializedProperty property, GUIContent label);

		public delegate float DefaultPropertyFieldHeight(SerializedProperty property, GUIContent label);

		private static Dictionary<Type, Type> _fieldTypeToDrawerType;
		private static Type[] _propertyDrawerTypes;
		public static DefaultPropertyFieldDelegate DefaultPropertyField;
		public static DefaultPropertyFieldHeight DefaultPropertyHeight;
		public static DefaultPropertyFieldDelegate VanillaPropertyField;
		static RefEditorGUI()
		{
			var asmName = typeof(PropertyDrawer).AssemblyQualifiedName;
			var t2 = Type.GetType(asmName.Replace("UnityEditor.PropertyDrawer", "UnityEditor.EditorAssemblies"));
			var subClassMethod = t2.GetMethod("SubclassesOf", BindingFlags.Static | BindingFlags.NonPublic);

			var propertyDrawerTypesObj = subClassMethod?.Invoke(null, new object[] { typeof(PropertyDrawer) });
			_propertyDrawerTypes = propertyDrawerTypesObj as Type[];

			var t = typeof(EditorGUI);
			var delegateType = typeof(DefaultPropertyFieldDelegate);
			var m = t.GetMethod("DefaultPropertyField", BindingFlags.Static | BindingFlags.NonPublic);
			VanillaPropertyField = (DefaultPropertyFieldDelegate)System.Delegate.CreateDelegate(delegateType, m);

			_fieldTypeToDrawerType = new Dictionary<Type, Type>();
			DefaultPropertyHeight = (property, label) =>
			{
				var parentType = property.serializedObject.targetObject.GetType();
				var field = parentType.GetField(property.propertyPath);

				var fieldType = GetPropertyType(property);
				if (!_fieldTypeToDrawerType.ContainsKey(fieldType))
				{
					var drawerType = GetPropertyDrawerType(fieldType);
					_fieldTypeToDrawerType.Add(fieldType, drawerType);
				}

				var foundDrawerType = _fieldTypeToDrawerType[fieldType];
				if (foundDrawerType == null)
				{
					return EditorGUI.GetPropertyHeight(property, label);
				}
				else
				{
					var instance = (PropertyDrawer)Activator.CreateInstance(foundDrawerType);
					return instance.GetPropertyHeight(property, label);
				}
			};
			DefaultPropertyField = (position, property, label) =>
			{
				var parentType = property.serializedObject.targetObject.GetType();
				var field = parentType.GetField(property.propertyPath);

				var fieldType = GetPropertyType(property);
				if (!_fieldTypeToDrawerType.ContainsKey(fieldType))
				{
					var drawerType = GetPropertyDrawerType(fieldType);
					_fieldTypeToDrawerType.Add(fieldType, drawerType);
				}

				var foundDrawerType = _fieldTypeToDrawerType[fieldType];
				if (foundDrawerType == null)
				{
					EditorGUI.BeginProperty(position, label, property);
					EditorGUI.PropertyField(position, property, label, true);
					EditorGUI.EndProperty();
				}
				else
				{
					var instance = (PropertyDrawer)Activator.CreateInstance(foundDrawerType);
					instance.OnGUI(position, property, label);

				}
				return true;
			};

		}

		static Type GetPropertyType(SerializedProperty prop)
		{
			//gets parent type info
			string[] slices = prop.propertyPath.Split('.');
			System.Type type = prop.serializedObject.targetObject.GetType();

			for (int i = 0; i < slices.Length; i++)
			{
				if (slices[i] == "Array")
				{
					i++; //skips "data[x]"
					type = type.GetElementType() ?? type.GetGenericArguments()[0]; //gets info on array elements
				}

				//gets info on field and its type
				else
				{
					type = type.GetField(slices[i],
						  BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy |
						  BindingFlags.Instance)
					   .FieldType;

				}
			}

			return type;
		}


		static Type GetPropertyDrawerType(Type fieldType)
		{
			return _propertyDrawerTypes.FirstOrDefault(drawerType =>
			{
				var attributes = drawerType.GetCustomAttributes<CustomPropertyDrawer>();
				var attribute = attributes.FirstOrDefault();
				//var attribute = drawerType.GetCustomAttribute<CustomPropertyDrawer>();
				if (attribute == null) return false;

				var typeField = typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
				var useChildrenField = typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);
				var drawerTargetType = (Type)typeField?.GetValue(attribute);
				var drawerChildren = (bool)useChildrenField?.GetValue(attribute);

				bool match;
				if (drawerChildren)
				{
					match = drawerTargetType.IsAssignableFrom(fieldType);
				}
				else
				{
					match = fieldType == drawerTargetType;
				}
				return match;
			});
		}
	}
}
