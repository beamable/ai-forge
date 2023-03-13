using Beamable.Editor.Config.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Config
{
	public class ConfigWindow : EditorWindow
	{
		public static void CreateFields(VisualElement rootElement, ConfigQuery query, List<ConfigOption> options, bool useFoldout)
		{
			rootElement.Clear();

			ConfigOption lastOption = null;
			Foldout currFoldout = null;
			var i = 0;

			var asmName = typeof(PropertyDrawer).AssemblyQualifiedName;
			var t2 = Type.GetType(asmName.Replace("UnityEditor.PropertyDrawer", "UnityEditor.ScriptAttributeUtility"));
			var getFieldMethod = t2.GetMethod("GetFieldInfoFromProperty", BindingFlags.Static | BindingFlags.NonPublic);

			foreach (var option in options)
			{

				if (query != null && !query.Accepts(option))
				{
					continue;
				}

				if (useFoldout && (lastOption == null || lastOption.Module != option.Module))
				{
					currFoldout = new Foldout();
					currFoldout.style.SetLeft((option.Property.depth) * 28);
					currFoldout.text = option.Module.Replace("Configuration", "");
					currFoldout.value = true;
					currFoldout.AddToClassList("category");
					rootElement.Add(currFoldout);

				}
				try
				{
					var field = new PropertyField(option.Property);
					field.Bind(option.Object);
					if (i % 2 == 1)
					{
						field.AddToClassList("oddRow");
					}

					field.Query<PropertyField>().ForEach(propField =>
					{
						var serializedPropField =
					  typeof(PropertyField).GetField("m_SerializedProperty", BindingFlags.Instance | BindingFlags.NonPublic)
						 ?.GetValue(propField) as SerializedProperty;

						if (serializedPropField == null) return;

						var parameters = new[] { serializedPropField, null };
						var fieldObj = (FieldInfo)getFieldMethod.Invoke(null, parameters);
						var help = serializedPropField.tooltip;
						if (fieldObj != null)
						{
							var tooltipAttr = fieldObj.GetCustomAttribute<TooltipAttribute>();
							if (tooltipAttr != null)
							{
								help = tooltipAttr.tooltip;
							}
						}
						if (serializedPropField.depth == 0 && useFoldout)
						{
							propField.style.SetMarginLeft(10);
						}
						propField.tooltip = help;
					});

					field.Query<Label>().ForEach(label =>
					{
						label.tooltip = option.Help;
						label.style.minWidth = EditorGUIUtility.labelWidth;
						label.style.maxWidth = EditorGUIUtility.labelWidth;
						label.AddTextWrapStyle();
					});
					if (useFoldout)
					{
						currFoldout.Add(field);
					}
					else
					{
						rootElement.Add(field);
					}
				}
				catch (Exception ex)
				{
					Debug.LogError($"Failed to show config option. module=[{option.Module}] property=[{option.Name}] message=[{ex.Message}]");
					Debug.LogException(ex);
				}

				lastOption = option;
				i++;

			}
		}

	}
}
