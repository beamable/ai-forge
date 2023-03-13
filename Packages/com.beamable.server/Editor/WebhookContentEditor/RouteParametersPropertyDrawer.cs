using Beamable.Common.Content;
using Beamable.Editor;
using Beamable.Server.Editor.CodeGen;
using Beamable.Server.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Beamable.Server.Editor
{
	[CustomPropertyDrawer(typeof(RouteParameters))]
	public class RouteParametersPropertyDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var apiProperty = property.FindPropertyRelative(nameof(RouteParameters.ApiContent));

			if (apiProperty == null || apiProperty.serializedObject == null)
			{
				return EditorGUIUtility.singleLineHeight;
			}

			var serviceRouteProperty = apiProperty.serializedObject.FindProperty(nameof(ApiContent.ServiceRoute));
			var serviceNameProperty = serviceRouteProperty.FindPropertyRelative(nameof(ServiceRoute.Service));
			var endpointProperty = serviceRouteProperty.FindPropertyRelative(nameof(ServiceRoute.Endpoint));

			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			var descriptor = serviceRegistry.Descriptors.FirstOrDefault(d => d.Name.Equals(serviceNameProperty.stringValue));
			if (descriptor == null)
			{
				return EditorGUIUtility.singleLineHeight;
			}
			var method = descriptor.Methods.FirstOrDefault(m => m.Path.Equals(endpointProperty.stringValue));

			var routeProperties = GetRouteProperties(descriptor, method, property);
			var totalPropertyHeight = routeProperties
			   .Select(p => p.IsUsingVariable
				  ? EditorGUIUtility.singleLineHeight
				  : EditorGUI.GetPropertyHeight(p.property) + 2).Sum();
			if (routeProperties.Count == 0)
			{
				totalPropertyHeight = EditorGUIUtility.singleLineHeight + 2;
			}
			return totalPropertyHeight + EditorGUIUtility.singleLineHeight;
		}


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{

			var apiProperty = property.FindPropertyRelative(nameof(RouteParameters.ApiContent));

			if (apiProperty == null || apiProperty.serializedObject == null)
			{
				EditorGUI.LabelField(position, "could not find parent api content.");
				return;
			}

			var serviceRouteProperty = apiProperty.serializedObject.FindProperty(nameof(ApiContent.ServiceRoute));
			var serviceNameProperty = serviceRouteProperty.FindPropertyRelative(nameof(ServiceRoute.Service));
			var endpointProperty = serviceRouteProperty.FindPropertyRelative(nameof(ServiceRoute.Endpoint));
			var variablesProperty = apiProperty.serializedObject.FindProperty("_variables");
			var variablesArrayProperty = variablesProperty.FindPropertyRelative(nameof(RouteVariables.Variables));

			var hasAnyVariables = variablesArrayProperty.arraySize > 0;
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			var descriptor = serviceRegistry.Descriptors.FirstOrDefault(d => d.Name.Equals(serviceNameProperty.stringValue));
			if (descriptor == null) return;

			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(position, "Route Parameters", new GUIStyle(EditorStyles.label) { font = EditorStyles.boldFont });
			position.y += EditorGUIUtility.singleLineHeight + 2;
			EditorGUI.indentLevel += 1;

			var method = descriptor.Methods.FirstOrDefault(m => m.Path.Equals(endpointProperty.stringValue));
			var routeProperties = GetRouteProperties(descriptor, method, property);
			if (routeProperties.Count == 0)
			{
				EditorGUI.LabelField(position, "This route has no parameters", new GUIStyle(EditorStyles.label));
			}
			foreach (var info in routeProperties)
			{
				info.typeProperty.stringValue = ApiVariable.GetTypeName(info.ParameterType);
				info.nameProperty.stringValue = info.Name;
				EditorGUI.BeginChangeCheck();

				var height = info.IsUsingVariable
				   ? EditorGUIUtility.singleLineHeight
				   : EditorGUI.GetPropertyHeight(info.property);
				var infoLabel = new GUIContent(info.Name);
				var rightWidth = hasAnyVariables ? 30 : 0;
				var fieldPosition = new Rect(position.x, position.y, position.width - rightWidth, height);
				var buttonButton = new Rect(position.xMax - 20, position.y, 20, EditorGUIUtility.singleLineHeight);
				position.height = height;
				position.y += height + 2;

				GUIStyle iconButtonStyle = GUI.skin.FindStyle("IconButton") ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("IconButton");
				GUIContent content = EditorGUIUtility.IconContent("_Popup");
				if (hasAnyVariables && EditorGUI.DropdownButton(buttonButton, content, FocusType.Keyboard,
				   iconButtonStyle))
				{
					GenericMenu menu = new GenericMenu();
					menu.AddItem(new GUIContent("Use Variable"), info.IsUsingVariable, () =>
					{
						info.ToggleVariable();
					});
					menu.ShowAsContext();
				}

				if (info.IsUsingVariable)
				{
					var options = new List<GUIContent>();
					var parameterTypeValue = info.typeProperty.stringValue;

					for (var i = 0; i < variablesArrayProperty.arraySize; i++)
					{
						var variableName = variablesArrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(ApiVariable.Name)).stringValue;
						var variableType = variablesArrayProperty.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(ApiVariable.TypeName)).stringValue;

						if (!string.Equals(variableType, parameterTypeValue))
						{
							continue;
						}

						options.Add(new GUIContent(variableName));
					}

					var existingVariableName = info.variableValueProperty.stringValue;
					var selectedIndex = options.FindIndex(o => o.text.Equals(existingVariableName));

					var originalSelectedIndex = selectedIndex;
					var force = false;
					if (originalSelectedIndex == -1)
					{
						if (string.IsNullOrEmpty(existingVariableName))
						{
							if (options.Count == 1)
							{
								originalSelectedIndex = 0; // automatically fix the issue and pick the first available variable.
								selectedIndex = 0;
								force = true;
							}
							else
							{
								options.Insert(0, new GUIContent("<none>"));
								selectedIndex = 0;
							}
						}
						else
						{
							options.Insert(0, new GUIContent($"{existingVariableName} (missing)"));
							selectedIndex = 0;
						}
					}

					EditorGUI.BeginChangeCheck();
					var nextSelectedIndex = EditorGUI.Popup(fieldPosition, infoLabel, selectedIndex, options.ToArray());
					if (EditorGUI.EndChangeCheck() || force)
					{
						if (originalSelectedIndex == -1 && nextSelectedIndex == 0) continue;

						info.variableValueProperty.stringValue = options[nextSelectedIndex].text;
					}

					continue;
				}


				EditorGUI.PropertyField(fieldPosition, info.property, infoLabel, true);
				var hasModifiedProperties = info.property.serializedObject.hasModifiedProperties;
				if (!EditorGUI.EndChangeCheck() && !hasModifiedProperties && !info.forceUpdate) continue;
				info.forceUpdate = false;
				info.property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				var nextValue = info.Type.GetField("Data", BindingFlags.Instance | BindingFlags.Public).GetValue(info.instance);

				EditorDebouncer.Debounce("api-content-route-parameter", () =>
				{
					var json = (string)typeof(MicroserviceClientHelper)
										.GetMethod(nameof(MicroserviceClientHelper.SerializeArgument), BindingFlags.Static | BindingFlags.Public)
										.Invoke(null, new object[] { nextValue });
					info.rawProperty.stringValue = json;
					info.rawProperty.serializedObject.ApplyModifiedProperties();
					info.rawProperty.serializedObject.Update();
					EditorUtility.SetDirty(info.rawProperty.serializedObject.targetObject);
				});
			}

			EditorGUI.indentLevel -= 1;
		}


		private List<SerializedRouteParameterInfo> GetRouteProperties(MicroserviceDescriptor descriptor, ClientCallableDescriptor method, SerializedProperty property)
		{
			var output = new List<SerializedRouteParameterInfo>();

			var parametersProperty = property.FindPropertyRelative(nameof(RouteParameters.Parameters));
			if (parametersProperty == null || method == null) return output;

			var oldLength = parametersProperty.arraySize;
			parametersProperty.arraySize = method.Parameters.Length;

			for (var i = 0; i < method.Parameters.Length; i++)
			{
				var isNew = i >= oldLength;

				var parameter = method.Parameters[i];
				var parameterProperty = parametersProperty.GetArrayElementAtIndex(i);

				var rawProperty = parameterProperty.FindPropertyRelative(nameof(RouteParameter.Data));
				if (isNew)
				{
					rawProperty.stringValue = "";
				}
				var nameProperty = parameterProperty.FindPropertyRelative(nameof(RouteParameter.Name));
				var typeProperty = parameterProperty.FindPropertyRelative(nameof(RouteParameter.TypeName));
				var variableOptionProperty = parameterProperty.FindPropertyRelative(nameof(RouteParameter.variableReference));
				var variableHasValueProperty = variableOptionProperty.FindPropertyRelative(nameof(Optional.HasValue));
				var variableValueProperty = variableOptionProperty.FindPropertyRelative(nameof(Optional<string>.Value)).FindPropertyRelative(nameof(ApiVariableReference.Name));

				var info = new SerializedRouteParameterInfo
				{
					Name = parameter.Name,
					property = property,
					rawProperty = rawProperty,
					nameProperty = nameProperty,
					isUsingVariableProperty = variableHasValueProperty,
					variableValueProperty = variableValueProperty,
					typeProperty = typeProperty
				};

				try
				{
					var type = ClientCodeGenerator.GetDataWrapperTypeForParameter(descriptor, parameter.Type);

					// TODO: somehow cache this???
					var instance = ScriptableObject.CreateInstance(type);

					try
					{
						var value = typeof(MicroserviceClientHelper)
						   .GetMethod(nameof(MicroserviceClientHelper.DeserializeResult), BindingFlags.Static | BindingFlags.Public)
						   .MakeGenericMethod(parameter.Type).Invoke(null, new[] { rawProperty.stringValue });
						if (value is string strValue && strValue.StartsWith("\"") && strValue.EndsWith("\""))
						{
							value = strValue.Substring(1, strValue.Length - 2);
						}
						type.GetField("Data", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, value);
					}
					catch
					{
						// its okay to ignore this exception and present the default view.
						rawProperty.stringValue = String.Empty;
						info.forceUpdate = true;
					}
					// deserialize the raw data string and set it.

					var serialized = new SerializedObject(instance);
					info.property = serialized.FindProperty("Data");
					info.instance = instance;
				}
				catch
				{
					info.property = property;
					info.instance = null;
				}

				output.Add(info);

			}

			return output;
		}


		[Serializable]
		private class SerializedRouteParameterInfo
		{
			public string Name;
			public ScriptableObject instance;
			public SerializedProperty property;
			public SerializedProperty rawProperty;
			public SerializedProperty variableValueProperty;
			public SerializedProperty isUsingVariableProperty;
			public SerializedProperty typeProperty;
			public SerializedProperty nameProperty;
			public bool forceUpdate;

			public Type Type => instance?.GetType();
			public Type ParameterType => Type.BaseType.GetGenericArguments()[0];

			public bool ToggleVariable()
			{
				isUsingVariableProperty.boolValue = !isUsingVariableProperty.boolValue;

				if (!isUsingVariableProperty.boolValue)
				{
					variableValueProperty.stringValue = "";
				}
				isUsingVariableProperty.serializedObject.ApplyModifiedProperties();
				return IsUsingVariable;
			}

			public bool IsUsingVariable => isUsingVariableProperty.boolValue;
		}

	}
}
