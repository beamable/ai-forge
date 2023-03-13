using Beamable.Common.Assistant;
using Beamable.Editor.Assistant;
using Beamable.Editor.Microservice.UI;
using Beamable.Editor.Reflection;
using Beamable.Editor.UI;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.DockerCommands;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;

#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Server.Editor
{
	public class MicroservicesBeamHintDetailsConverterProvider : BeamHintDetailConverterProvider
	{
		/// <summary>
		/// Converter that handles the <see cref="BeamHintIds.ID_DOCKER_PROCESS_NOT_RUNNING"/> hint.
		/// </summary>
		[BeamHintDetailConverter(typeof(BeamHintReflectionCache.DefaultConverter),
								 BeamHintType.Validation, "", "DockerProcessNotRunning",
								 "HintDetailsSingleTextButton")]
		public static void DockerNotRunningConverter(in BeamHint hint, in BeamHintTextMap textMap, BeamHintVisualsInjectionBag injectionBag)
		{
			var validationIntro = textMap != null && textMap.TryGetHintIntroText(hint.Header, out var intro) ? intro : hint.Header.Id;
			injectionBag.SetLabel(validationIntro, "hintText");
			injectionBag.SetButtonLabel("Try to Open Docker Desktop", "hintButton");
			injectionBag.SetButtonClicked(() =>
			{
				_ = DockerCommand.RunDockerProcess();
			}, "hintButton");
		}

		/// <summary>
		/// Converter that handles the <see cref="BeamHintIds.ID_DOCKER_PROCESS_NOT_RUNNING"/> hint.
		/// </summary>
		[BeamHintDetailConverter(typeof(BeamHintReflectionCache.DefaultConverter),
								 BeamHintType.Validation, "", "InstallDockerProcess",
								 "HintDetailsSingleTextButton")]
		public static void InstallDockerProcessConverter(in BeamHint hint, in BeamHintTextMap textMap, BeamHintVisualsInjectionBag injectionBag)
		{
			var validationIntro = textMap != null && textMap.TryGetHintIntroText(hint.Header, out var intro) ? intro : hint.Header.Id;
			injectionBag.SetLabel(validationIntro, "hintText");
			injectionBag.SetButtonLabel("Go to Docker and Docker Desktop's Installation Guide", "hintButton");
			injectionBag.SetButtonClicked(() =>
			{
				Application.OpenURL("https://docs.docker.com/get-docker/");
			}, "hintButton");
		}

		/// <summary>
		/// Converter that handles the <see cref="BeamHintIds.ID_DOCKER_PROCESS_NOT_RUNNING"/> hint.
		/// </summary>
		[BeamHintDetailConverter(typeof(BeamHintReflectionCache.DefaultConverter),
								 BeamHintType.Validation, "", "ChangesNotDeployedToLocalDocker",
								 "HintDetailsSingleTextDynamicElements")]
		public static void ChangesNotDeployedToLocalDockerConverter(in BeamHint hint, in BeamHintTextMap textMap, BeamHintVisualsInjectionBag injectionBag)
		{
			var validationIntro = textMap != null && textMap.TryGetHintIntroText(hint.Header, out var intro) ? intro : hint.Header.Id;
			injectionBag.SetLabel(validationIntro, "hintText");

			var registry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			var listOfServiceCodeHandlesThatNeedRebuilding = (List<BeamServiceCodeHandle>)hint.ContextObject;
			var rebuildNecessaryDescriptors = registry.Descriptors.Where(desc => listOfServiceCodeHandlesThatNeedRebuilding.Any(a => a.ServiceName == desc.Name));

			var buttonsToAdd = rebuildNecessaryDescriptors.Select(desc =>
			{
				var btn = new Button() { text = $"Re-Run {desc.Name}" };
				btn.clickable.clicked += ClickEvent;

				async void ClickEvent()
				{
					var msWindow = await MicroserviceWindow.GetFullyInitializedWindow();
					msWindow.Show();
					msWindow.RefreshWindowContent();

					var model = MicroservicesDataModel.Instance.GetModel<MicroserviceModel>(desc);
					await model.BuildAndRestart();
					btn.clickable.clicked -= ClickEvent;
				}
				return btn;
			});

			foreach (Button button in buttonsToAdd)
				injectionBag.AddAsChild(button, "hintDynamicElements");
		}
	}
}
