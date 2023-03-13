using Beamable.Editor.Environment;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
namespace Beamable.Editor.Toolbox.UI.Components
{
	public class InstallServerVisualElement : ToolboxComponent
	{
		public event Action OnClose;
		public event Action OnInfo;
		public event Action OnDone;

		public InstallServerVisualElement() : base(nameof(InstallServerVisualElement))
		{

		}

		public BeamablePackageMeta Model { get; set; }

		private Button _downloadButton;

		public override void Refresh()
		{
			base.Refresh();

			var lbl = Root.Q<Label>();
			lbl.AddTextWrapStyle();

			_downloadButton = Root.Q<Button>("download");

			Root.Q<Button>("cancel").clickable.clicked += () => OnClose?.Invoke();
			Root.Q<Button>("docs").clickable.clicked += () => OnInfo?.Invoke();
			_downloadButton.clickable.clicked += DownloadClicked;
		}

		private void DownloadClicked()
		{
			_downloadButton.SetEnabled(false);
			BeamablePackages.DownloadServer(Model).Then(_ => OnDone?.Invoke());
		}
	}
}
