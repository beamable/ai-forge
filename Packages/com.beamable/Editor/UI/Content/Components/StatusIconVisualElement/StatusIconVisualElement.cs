using Beamable.Editor.Content.Models;
using System.Collections.Generic;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
	public class StatusIconVisualElement : ContentManagerComponent
	{

		public new class UxmlFactory : UxmlFactory<StatusIconVisualElement, UxmlTraits> { }
		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription { name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}
			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var self = ve as StatusIconVisualElement;
			}
		}

		private VisualElement _imageServer, _imageLocal;
		public HostStatus LocalStatus { get; internal set; }
		public HostStatus ServerStatus { get; internal set; }

		public StatusIconVisualElement() : base(nameof(StatusIconVisualElement))
		{

		}

		public override void Refresh()
		{
			base.Refresh();

			_imageServer = Root.Q<VisualElement>("imageServer");
			_imageLocal = Root.Q<VisualElement>("imageLocal");

			switch (ServerStatus)
			{
				case HostStatus.AVAILABLE:
					_imageServer.AddToClassList("server-available");
					break;
				case HostStatus.NOT_AVAILABLE:
					_imageServer.AddToClassList("server-not-available");
					break;
				case HostStatus.UNKNOWN:
					_imageServer.AddToClassList("server-unknown");
					break;
			}

			switch (LocalStatus)
			{
				case HostStatus.AVAILABLE:
					_imageLocal.AddToClassList("local-available");
					break;
				case HostStatus.NOT_AVAILABLE:
					_imageLocal.AddToClassList("local-not-available");
					break;
				case HostStatus.UNKNOWN:
					_imageLocal.AddToClassList("local-unknown");
					break;
			}

			//TODO: Set image based on Status
			//var iconPath = "Assets/DemoGame/Textures/icons/setting.png
			//var iconAsset = Resources.Load<Texture2D>(iconPath);
			//_image.image = iconAsset;
		}
	}
}
