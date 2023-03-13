using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Beamable.Common.Constants.MenuItems.Windows;

namespace Beamable.Server.Editor
{
	public class CreateMicroservicePopup : EditorWindow
	{
		public List<string> microservicesNames;
		string _newMicroserviceName = "NewMicroservice";

		public static void Show(Vector2 pos)
		{
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();

			CreateMicroservicePopup window = ScriptableObject.CreateInstance<CreateMicroservicePopup>();
			window.position = new Rect(pos.x, pos.y, 250, 80);
			window.microservicesNames = serviceRegistry.Descriptors.Select(descriptor => descriptor.Name).ToList();
			window.ShowPopup();
		}

		[MenuItem(
		   Paths.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_MICROSERVICES + "/" +
		   "<Create New...>",
		   priority = Orders.MENU_ITEM_PATH_WINDOW_PRIORITY_3)]
		static void Init()
		{
			Show(new Vector2(Screen.width, Screen.height) / 2);
		}

		void OnGUI()
		{
			var oldName = _newMicroserviceName;

			#region Layout part
			EditorGUILayout.LabelField("Enter Name of new Microservice (only A-Z):", EditorStyles.wordWrappedLabel);
			_newMicroserviceName = GUILayout.TextField(_newMicroserviceName, 100);
			bool serviceAlreadyExist = microservicesNames.Any(s => s.Equals(_newMicroserviceName));
			EditorGUILayout.BeginHorizontal();
			bool shouldClose = GUILayout.Button("Cancel");
			EditorGUI.BeginDisabledGroup(serviceAlreadyExist || _newMicroserviceName.Length < 1);
			bool createNewMicroservice = GUILayout.Button("OK");
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
			#endregion

			_newMicroserviceName = _newMicroserviceName.All(c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
				? _newMicroserviceName
				: oldName;

			if (createNewMicroservice)
				MicroserviceEditor.CreateNewMicroservice(_newMicroserviceName);

			if (shouldClose || createNewMicroservice)
				Close();
		}
	}
}
