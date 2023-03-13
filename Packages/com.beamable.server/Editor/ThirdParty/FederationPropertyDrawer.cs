using Beamable.Common.Content;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Content;

namespace Beamable.Server.Editor
{
	[CustomPropertyDrawer(typeof(Federation))]
	public class FederationPropertyDrawer : PropertyDrawer
	{
		private const int PADDING = 2;

		private List<MicroserviceDescriptor> _filteredDescriptors;
		private readonly List<FederationOption> _options = new List<FederationOption>();

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 3 + PADDING * 2;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();

			if (_filteredDescriptors == null)
			{
				_filteredDescriptors = serviceRegistry.Descriptors
													  .FindAll(descriptor => descriptor.IsUsedForFederation)
													  .ToList();
			}

			if (_filteredDescriptors.Count == 0)
			{
				position = EditorGUI.PrefixLabel(position, label);
				EditorGUI.SelectableLabel(
					position,
					"You must create a Microservice implementing IFederatedLogin interface to configure a Federation",
					EditorStyles.wordWrappedLabel);
				return;
			}

			BuildOptions(_filteredDescriptors);

			var routeInfoPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(routeInfoPosition, "Federation",
								 new GUIStyle(EditorStyles.label) { font = EditorStyles.boldFont });
			EditorGUI.indentLevel += 1;

			var servicesGuiContents = _options
									  .Select(opt => new GUIContent(opt.ToString()))
									  .ToList();

			var nextRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + PADDING,
									position.width, EditorGUIUtility.singleLineHeight);

			SerializedProperty serviceProperty = property.FindPropertyRelative(nameof(Federation.Service));
			SerializedProperty namespaceProperty = property.FindPropertyRelative(nameof(Federation.Namespace));
			var originalServiceIndex = _options.FindIndex(opt => opt.Microservice == serviceProperty.stringValue &&
																 opt.Namespace == namespaceProperty.stringValue);

			if (originalServiceIndex == -1)
			{
				if (string.IsNullOrEmpty(serviceProperty.stringValue) ||
					string.IsNullOrEmpty(namespaceProperty.stringValue))
				{
					servicesGuiContents.Insert(0, new GUIContent("<none>"));
					originalServiceIndex = 0;
				}
				else
				{
					servicesGuiContents.Insert(0, new GUIContent(serviceProperty.stringValue));
					originalServiceIndex = 0;
					if (!serviceProperty.stringValue.EndsWith(MISSING_SUFFIX))
					{
						serviceProperty.stringValue += MISSING_SUFFIX;
					}
				}
			}

			EditorGUI.BeginChangeCheck();
			var nextServiceIndex = EditorGUI.Popup(nextRect, new GUIContent("Federation"), originalServiceIndex,
												   servicesGuiContents.ToArray(), EditorStyles.popup);
			if (EditorGUI.EndChangeCheck())
			{
				var option =
					_options.FirstOrDefault(opt => opt.ToString().Equals(servicesGuiContents[nextServiceIndex].text));
				serviceProperty.stringValue = option?.Microservice;
				namespaceProperty.stringValue = option?.Namespace;
			}
		}

		private void BuildOptions(List<MicroserviceDescriptor> descriptors)
		{
			_options.Clear();

			foreach (var descriptor in descriptors)
			{
				foreach (var federatedNamespace in descriptor.FederatedNamespaces)
				{
					_options.Add(new FederationOption { Microservice = descriptor.Name, Namespace = federatedNamespace });
				}
			}
		}

		[System.Serializable]
		private class FederationOption
		{
			public string Microservice { get; set; }
			public string Namespace { get; set; }

			public override string ToString()
			{
				return $"{Microservice} / {Namespace}";
			}
		}
	}
}
