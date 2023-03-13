using Beamable.Editor.Microservice.UI.Components;
using Beamable.Server.Editor;
using Beamable.Server.Editor.UI.Components;
using System;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Components
{
	public class PublishStatusVisualElement : MicroserviceComponent
	{
		public new class UxmlFactory : UxmlFactory<PublishStatusVisualElement, UxmlTraits>
		{ }

		public PublishStatusVisualElement() : base(nameof(PublishStatusVisualElement))
		{ }

		private const int MILISECOND_PER_UPDATE = 250;
		private readonly string[] topMessageUpdateTexts =
			{"Deploying   ", "Deploying.  ", "Deploying.. ", "Deploying..."};

		Label _label;
		int _topMessageCounter = 0;
		private DateTime _lastUpdateTime;
		private bool _topMessageUpdating = false;

		public override void Refresh()
		{
			base.Refresh();
			_lastUpdateTime = DateTime.Now;
			_label = Root.Q<Label>("value");

			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();

			serviceRegistry.OnDeploySuccess -= HandleDeploySuccess;
			serviceRegistry.OnDeploySuccess += HandleDeploySuccess;
			serviceRegistry.OnServiceDeployStatusChanged -= HandleServiceDeployStatusChanged;
			serviceRegistry.OnServiceDeployStatusChanged += HandleServiceDeployStatusChanged;
			HandleServiceDeployStatusChanged(null, ServicePublishState.Unpublished);
		}

		public void HandleSubmitClicked()
		{
			_label.text = topMessageUpdateTexts[0];
			if (!_topMessageUpdating)
			{
				EditorApplication.update -= UpdateTopMessageText;
				EditorApplication.update += UpdateTopMessageText;
				_topMessageUpdating = true;
			}
		}

		private void HandleServiceDeployStatusChanged(IDescriptor descriptor, ServicePublishState state)
		{
			switch (state)
			{
				case ServicePublishState.Unpublished:
					_label.text = "Are you sure you want to deploy your microservices?";
					return;
				case ServicePublishState.Failed:
					_label.text = $"Oh no! Errors appears during publishing of {descriptor.Name}. Please check the log for detailed information.";
					EditorApplication.update -= UpdateTopMessageText;
					_topMessageUpdating = false;
					break;
			}
		}

		private void UpdateTopMessageText()
		{
			if (!_topMessageUpdating)
				return;
			var currentTime = DateTime.Now;
			if (_lastUpdateTime.AddMilliseconds(MILISECOND_PER_UPDATE) > currentTime)
				return;
			_lastUpdateTime = currentTime;
			_topMessageCounter++;
			var currentTextValue =
				topMessageUpdateTexts[_topMessageCounter % topMessageUpdateTexts.Length];
			_label.text = currentTextValue;
		}

		private void HandleDeploySuccess(ManifestModel manifest, int servicesAmount)
		{
			_topMessageUpdating = false;
			const string oneService = "service";
			const string multipleServices = "services";
			_label.text = $"Congratulations! You have successfully published {servicesAmount} {(servicesAmount == 1 ? oneService : multipleServices)}!";
		}
	}
}
