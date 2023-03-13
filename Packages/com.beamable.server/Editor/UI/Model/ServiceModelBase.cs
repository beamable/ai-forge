using Beamable.Server.Editor;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Model
{
	[Serializable]
	public abstract class ServiceModelBase : IBeamableService
	{
		private const float DEFAULT_HEIGHT = 300.0f;

		public abstract bool IsRunning { get; }
		public bool AreLogsAttached
		{
			get => _areLogsAttached;
			protected set => _areLogsAttached = value;
		}

		[SerializeField] private bool _areLogsAttached = true;
		[SerializeField] private LogMessageStore _logs = new LogMessageStore();
		[SerializeField] private float _visualHeight = DEFAULT_HEIGHT;

		public LogMessageStore Logs => _logs;

		public float VisualElementHeight
		{
			get => _visualHeight;
			set => _visualHeight = value;
		}

		public abstract IDescriptor Descriptor { get; }
		public abstract IBeamableBuilder Builder { get; }
		public ServiceType ServiceType => Descriptor.ServiceType;
		public string Name => Descriptor.Name;
		public virtual bool IsArchived { get; protected set; }

		public bool IsSelected
		{
			get => _isSelected;
			set
			{
				_isSelected = value;
				OnSelectionChanged?.Invoke(value);
			}
		}

		[SerializeField]
		private bool _isSelected;

		public bool IsCollapsed
		{
			get => _isCollapsed;
			set => _isCollapsed = value;
		}
		[SerializeField] private bool _isCollapsed = false;

		public Action OnLogsDetached { get; set; }
		public Action OnLogsAttached { get; set; }
		public Action<bool> OnLogsAttachmentChanged { get; set; }
		public Action<bool> OnSelectionChanged { get; set; }
		public Action OnSortChanged { get; set; }
		public Action<float, long, long> OnDeployProgress { get; set; }

		public abstract event Action<Task> OnStart;
		public abstract event Action<Task> OnStop;

		public void DetachLogs()
		{
			if (!AreLogsAttached) return;

			AreLogsAttached = false;
			OnLogsDetached?.Invoke();
			OnLogsAttachmentChanged?.Invoke(false);
		}
		public void AttachLogs()
		{
			if (AreLogsAttached) return;
			AreLogsAttached = true;
			OnLogsAttached?.Invoke();
			OnLogsAttachmentChanged?.Invoke(true);
		}

		public abstract void PopulateMoreDropdown(ContextualMenuPopulateEvent evt);
		public abstract void Refresh(IDescriptor descriptor);
		public abstract Task Start();
		public abstract Task Stop();

		public async void Archive(bool deleteAllFiles)
		{
			await Stop();
			await BeamServicesCodeWatcher.StopClientSourceCodeGenerator(Descriptor);

			if (deleteAllFiles)
			{
				BeamEditorContext.Default.OnServiceDeleteProceed?.Invoke();
				MicroserviceEditor.DeleteServiceFiles(Descriptor);
			}
			else
			{
				IsArchived = true;
			}

			MicroserviceConfiguration.Instance.Save();
			BeamEditorContext.Default.OnServiceArchived?.Invoke();
		}
		public void Unarchive()
		{
			IsArchived = false;
			MicroserviceConfiguration.Instance.Save();
			BeamEditorContext.Default.OnServiceUnarchived?.Invoke();
		}

		public void OpenCode()
		{
			var path = Path.GetDirectoryName(AssemblyDefinitionHelper.ConvertToInfo(Descriptor).Location);
			var fileName = $@"{path}/{Descriptor.Name}.cs";
			var asset = AssetDatabase.LoadMainAssetAtPath(fileName);
			AssetDatabase.OpenAsset(asset);
		}

		public abstract void OpenDocs();
	}
}
