using Beamable.Common;
using Beamable.Editor.UI.Model;
using Beamable.Server.Editor.ManagerClient;
using System.Threading.Tasks;
using static Beamable.Common.Constants.Features.Services;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

using static Beamable.Common.Constants;

namespace Beamable.Editor.Microservice.UI.Components
{
	public class StorageObjectVisualElement : ServiceBaseVisualElement
	{
		public new class UxmlFactory : UxmlFactory<StorageObjectVisualElement, UxmlTraits> { }

		protected override string ScriptName => nameof(StorageObjectVisualElement);
		protected override bool IsRemoteEnabled => _mongoStorageModel.RemoteReference?.enabled ?? false;

		private MongoStorageModel _mongoStorageModel;

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if (_mongoStorageModel == null) return;

			_mongoStorageModel.OnRemoteReferenceEnriched -= OnServiceReferenceChanged;
			_mongoStorageModel.ServiceBuilder.OnBuildingFinished -= OnBuildingFinished;
			_mongoStorageModel.ServiceBuilder.OnIsRunningChanged -= OnIsRunningChanged;
		}

		protected override void UpdateVisualElements()
		{
			base.UpdateVisualElements();
			_startButton.clickable.clicked -= HandleStartClicked;
			_startButton.clickable.clicked += HandleStartClicked;
			_mongoStorageModel.OnRemoteReferenceEnriched -= OnServiceReferenceChanged;
			_mongoStorageModel.OnRemoteReferenceEnriched += OnServiceReferenceChanged;
			_mongoStorageModel.ServiceBuilder.OnBuildingFinished -= OnBuildingFinished;
			_mongoStorageModel.ServiceBuilder.OnBuildingFinished += OnBuildingFinished;
			_mongoStorageModel.ServiceBuilder.OnIsRunningChanged -= OnIsRunningChanged;
			_mongoStorageModel.ServiceBuilder.OnIsRunningChanged += OnIsRunningChanged;
		}

		private void HandleStartClicked()
		{
			if (_mongoStorageModel.IsRunning)
			{
				_mongoStorageModel.Stop();
			}
			else
			{
				_mongoStorageModel.Start();
			}
		}

		protected override void UpdateLocalStatus()
		{
			base.UpdateLocalStatus();
			UpdateLocalStatusIcon(_mongoStorageModel.IsRunning, false);
		}

		private void OnIsRunningChanged(bool isRunning)
		{
			UpdateRemoteStatusIcon();
			UpdateLocalStatus();
		}

		private void OnBuildingFinished(bool isFinished)
		{
			UpdateRemoteStatusIcon();
			UpdateLocalStatus();
		}

		private void OnServiceReferenceChanged(ServiceStorageReference serviceReference)
		{
			UpdateRemoteStatusIcon();
		}

		protected override void SetupProgressBarForStart(Task _)
		{
			// left blank for no loading bar
		}

		protected override void SetupProgressBarForStop(Task _)
		{
			// left blank for no loading bar
		}

		protected override void QueryVisualElements()
		{
			base.QueryVisualElements();
			_mongoStorageModel = (MongoStorageModel)Model;
		}

		protected override void UpdateButtons()
		{
			base.UpdateButtons();

			_startButton.tooltip = Model.IsRunning ? STOP : Tooltips.Microservice.PLAY_STORAGE;
		}
	}
}
