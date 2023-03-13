using Beamable.Editor.Microservice.UI.Components;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor;
using System.Collections.Generic;
using System.Linq;
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
	public class PublishLoggerVisualElement : MicroserviceComponent
	{
		public new class UxmlFactory : UxmlFactory<PublishLoggerVisualElement, UxmlTraits>
		{ }

		public PublishLoggerVisualElement() : base(nameof(PublishLoggerVisualElement)) { }

		private VisualElement _logListRoot;
		private ListView _listView;
		private List<LogMessage> _logMessages;
		private IBeamableService _service;

		public override void Refresh()
		{
			base.Refresh();

			_logListRoot = Root.Q("logListRoot");
			_logMessages = new List<LogMessage>();
			_listView = new ListView()
			{
				makeItem = () => new ConsoleLogVisualElement(),
				bindItem = BindListViewElement,
				selectionType = SelectionType.Single,
				itemsSource = _logMessages
			};
			_listView.SetItemHeight(24);
			_listView.RefreshPolyfill();
			_logListRoot.Add(_listView);

			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();

			serviceRegistry.OnServiceDeployStatusChanged -= MicroservicesOnOnServiceDeployStatusChanged;
			serviceRegistry.OnServiceDeployStatusChanged += MicroservicesOnOnServiceDeployStatusChanged;
		}

		protected override void OnDestroy()
		{
			var serviceRegistry = BeamEditor.GetReflectionSystem<MicroserviceReflectionCache.Registry>();
			serviceRegistry.OnServiceDeployStatusChanged -= MicroservicesOnOnServiceDeployStatusChanged;
			base.OnDestroy();
			if (_service != null)
			{
				_service.Logs.OnMessagesUpdated -= HandleMessagesUpdated;
			}
		}

		private void MicroservicesOnOnServiceDeployStatusChanged(IDescriptor descriptor, ServicePublishState state)
		{
			if (state != ServicePublishState.InProgress)
				return;
			if (!MicroservicesDataModel.Instance.ContainsModel(descriptor.Name))
				return;
			var model = MicroservicesDataModel.Instance.GetModel<IBeamableService>(descriptor.Name);
			if (model == null || model.Name.Equals(_service?.Name))
				return;

			if (_service != null)
			{
				_service.Logs.OnMessagesUpdated -= HandleMessagesUpdated;
			}

			_service = model;
			_service.Logs.OnMessagesUpdated -= HandleMessagesUpdated;
			_service.Logs.OnMessagesUpdated += HandleMessagesUpdated;
		}

		private void HandleMessagesUpdated()
		{
			var message = _service.Logs.Messages.LastOrDefault();
			if (message == null) return;
			_logMessages.Add(message);
			EditorApplication.delayCall += () =>
			{
				_listView.RefreshPolyfill();
				_listView.MarkDirtyRepaint();
			};
		}

		void BindListViewElement(VisualElement elem, int index)
		{
			ConsoleLogVisualElement consoleLogVisualElement = (ConsoleLogVisualElement)elem;
			consoleLogVisualElement.Refresh();
			consoleLogVisualElement.SetNewModel(_listView.itemsSource[index] as LogMessage);
			if (index % 2 == 0)
			{
				consoleLogVisualElement.RemoveFromClassList("oddRow");
			}
			else
			{
				consoleLogVisualElement.AddToClassList("oddRow");
			}
			consoleLogVisualElement.MarkDirtyRepaint();
		}
	}
}
