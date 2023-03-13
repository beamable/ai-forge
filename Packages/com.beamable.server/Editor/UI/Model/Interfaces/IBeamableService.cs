using Beamable.Server.Editor;
using System;
using System.Threading.Tasks;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Model
{
	public interface IBeamableService
	{
		bool IsSelected { get; set; }
		bool IsRunning { get; }
		bool AreLogsAttached { get; }
		LogMessageStore Logs { get; }
		ServiceType ServiceType { get; }
		IDescriptor Descriptor { get; }
		string Name { get; }
		bool IsArchived { get; }

		Action OnLogsDetached { get; set; }
		Action OnLogsAttached { get; set; }
		Action<bool> OnLogsAttachmentChanged { get; set; }
		Action<bool> OnSelectionChanged { get; set; }
		Action OnSortChanged { get; set; }
		Action<float, long, long> OnDeployProgress { get; set; }

		event Action<Task> OnStart;
		event Action<Task> OnStop;

		void Refresh(IDescriptor descriptor);
		void DetachLogs();
		void AttachLogs();
		void PopulateMoreDropdown(ContextualMenuPopulateEvent evt);
		Task Start();
		Task Stop();
	}
}
