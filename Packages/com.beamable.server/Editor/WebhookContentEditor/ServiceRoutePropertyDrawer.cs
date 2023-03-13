using Beamable.Common.Content;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.Features.Content;

namespace Beamable.Server.Editor
{
	[CustomPropertyDrawer(typeof(ServiceRoute))]
	public class ServiceRoutePropertyDrawer : PropertyDrawer
	{
		private const int PADDING = 2;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 3 + PADDING * 2;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			// need to show a dropdown for the available services...
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			var descriptors = serviceRegistry.Descriptors;

			if (descriptors.Count == 0)
			{
				position = EditorGUI.PrefixLabel(position, label);
				EditorGUI.SelectableLabel(position, "You must create a Microservice to configure a Webhook Content", EditorStyles.wordWrappedLabel);
				return;
			}

			var routeInfoPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(routeInfoPosition, "Route Information", new GUIStyle(EditorStyles.label) { font = EditorStyles.boldFont });
			EditorGUI.indentLevel += 1;

			var serviceGuiContents = descriptors
				.Select(d => new GUIContent(d.Name))
				.ToList();


			var nextRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + PADDING, position.width, EditorGUIUtility.singleLineHeight);

			var serviceProperty = property.FindPropertyRelative(nameof(ServiceRoute.Service));
			var originalServiceIndex = descriptors.FindIndex(d => d.Name.Equals(serviceProperty.stringValue));

			if (originalServiceIndex == -1)
			{
				if (string.IsNullOrEmpty(serviceProperty.stringValue))
				{
					serviceGuiContents.Insert(0, new GUIContent("<none>"));
					originalServiceIndex = 0;
				}
				else
				{
					serviceGuiContents.Insert(0, new GUIContent(serviceProperty.stringValue));
					originalServiceIndex = 0;
					if (!serviceProperty.stringValue.EndsWith(MISSING_SUFFIX))
					{
						serviceProperty.stringValue += MISSING_SUFFIX;
					}
				}
			}

			EditorGUI.BeginChangeCheck();
			var nextServiceIndex = EditorGUI.Popup(nextRect, new GUIContent("Microservice"), originalServiceIndex, serviceGuiContents.ToArray(), EditorStyles.popup);
			if (EditorGUI.EndChangeCheck())
			{
				serviceProperty.stringValue = descriptors
											  .FirstOrDefault(
												  d => d.Name.Equals(serviceGuiContents[nextServiceIndex].text)).Name;
			}


			nextRect = new Rect(nextRect.x, nextRect.y + EditorGUIUtility.singleLineHeight + PADDING, nextRect.width, EditorGUIUtility.singleLineHeight);
			var service = descriptors.FirstOrDefault(d => d.Name.Equals(serviceProperty.stringValue));
			if (service == null)
			{
				nextRect = EditorGUI.PrefixLabel(nextRect, new GUIContent("Client Callable"));
				EditorGUI.SelectableLabel(nextRect, "You must select a valid service first", EditorStyles.wordWrappedLabel);
				return;
			}

			var clientCallableGuis = service.Methods
			   .Select(m => new GUIContent(m.Path))
			   .ToList();

			var routeProperty = property.FindPropertyRelative(nameof(ServiceRoute.Endpoint));
			var originalRouteIndex = service.Methods.FindIndex(d => d.Path.Equals(routeProperty.stringValue));

			var forceRoute = false;
			if (originalRouteIndex == -1)
			{
				var hasSuffix = routeProperty.stringValue.EndsWith(MISSING_SUFFIX);
				var withoutSuffix = hasSuffix
					? routeProperty.stringValue.Substring(
						0, routeProperty.stringValue.Length - MISSING_SUFFIX.Length)
					: routeProperty.stringValue;
				var existing = clientCallableGuis.ToList().FindIndex(m => m.text.Equals(withoutSuffix));

				if (existing != -1 && hasSuffix)
				{
					originalRouteIndex = existing;
					forceRoute = true;
				}
				else if (clientCallableGuis.Count == 1)
				{
					originalRouteIndex = 0;
					forceRoute = true;
				}
				else if (string.IsNullOrEmpty(routeProperty.stringValue))
				{
					clientCallableGuis.Insert(0, new GUIContent("<none>"));
					originalRouteIndex = 0;
				}
				else
				{
					clientCallableGuis.Insert(0, new GUIContent(routeProperty.stringValue));
					originalRouteIndex = 0;
					if (!routeProperty.stringValue.EndsWith(MISSING_SUFFIX))
					{
						routeProperty.stringValue += MISSING_SUFFIX;
					}
				}
			}

			EditorGUI.BeginChangeCheck();
			var routeIndex = EditorGUI.Popup(nextRect, new GUIContent("Method"), originalRouteIndex, clientCallableGuis.ToArray(), EditorStyles.popup);
			if (EditorGUI.EndChangeCheck() || forceRoute)
			{
				routeProperty.stringValue = service.Methods.FirstOrDefault(m => m.Path.Equals(clientCallableGuis[routeIndex].text)).Path;
			}

			EditorGUI.indentLevel -= 1;

		}
	}
}
