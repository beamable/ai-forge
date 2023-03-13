#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Beamable.InputManagerIntegration;

namespace Beamable.Editor.Modules.Input
{
   [CustomPropertyDrawer(typeof(InputActionArg))]
   public class InputActionArgPropertyDrawer : PropertyDrawer
   {
      public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
      {
         return EditorGUIUtility.singleLineHeight * (property.isExpanded ? 3 : 1);
      }

      public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
      {
         var line1Rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

         property.isExpanded = EditorGUI.Foldout(line1Rect, property.isExpanded, label);
         if (!property.isExpanded) return;

         EditorGUI.indentLevel++;
         var line2Rect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
         var line3Rect = new Rect(position.x, position.y + 2*EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

         var assetProperty = property.FindPropertyRelative(nameof(InputActionArg.actionAsset));
         var namedInputAction = (InputActionArg) GetTargetObjectOfProperty(property);

         EditorGUI.PropertyField(line2Rect, assetProperty);

         var asset = assetProperty.objectReferenceValue as InputActionAsset;
         if (asset == null)
         {
            EditorGUI.LabelField(line3Rect, "Action", "(Select an action asset to continue)");
            return;
         }

         var actions = new List<InputAction>();
         foreach (var map in asset.actionMaps)
         {
            foreach (var action in map.actions)
            {
               actions.Add(action);
            }
         }

         var currentIndex = actions.FindIndex(action => action.id == namedInputAction.action.id);
         var actionDisplays = actions.Select(action => new GUIContent($"{action.actionMap.name}.{action.name}")).ToArray();
         EditorGUI.BeginChangeCheck();
         var nextIndex = EditorGUI.Popup(line3Rect, new GUIContent("Action"), currentIndex, actionDisplays);

         if (EditorGUI.EndChangeCheck())
         {
            var selectedAction = actions[nextIndex];
            Undo.RecordObject(property.serializedObject.targetObject, "Change input action");

            namedInputAction.action = selectedAction;
            EditorUtility.SetDirty(property.serializedObject.targetObject);

         }
         EditorGUI.indentLevel--;

      }

#region Util Methods
      public static object GetTargetObjectOfProperty(SerializedProperty prop)
      {
         if (prop == null) return null;

         var path = prop.propertyPath.Replace(".Array.data[", "[");
         object obj = prop.serializedObject.targetObject;
         var elements = path.Split('.');
         foreach (var element in elements)
         {
            if (element.Contains("["))
            {
               var elementName = element.Substring(0, element.IndexOf("["));
               var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "")
                  .Replace("]", ""));
               obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
               obj = GetValue_Imp(obj, element);
            }
         }

         return obj;
      }

      private static object GetValue_Imp(object source, string name)
      {
         if (source == null)
            return null;
         var type = source.GetType();

         while (type != null)
         {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
               return f.GetValue(source);

            var p = type.GetProperty(name,
               BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
               return p.GetValue(source, null);

            type = type.BaseType;
         }

         return null;
      }

      private static object GetValue_Imp(object source, string name, int index)
      {
         var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
         if (enumerable == null) return null;
         var enm = enumerable.GetEnumerator();
         //while (index-- >= 0)
         //    enm.MoveNext();
         //return enm.Current;

         for (int i = 0; i <= index; i++)
         {
            if (!enm.MoveNext()) return null;
         }

         return enm.Current;
      }
#endregion
   }
}
#endif
