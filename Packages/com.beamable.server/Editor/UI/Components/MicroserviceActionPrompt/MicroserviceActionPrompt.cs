using System.Collections.Generic;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Microservice.UI.Components
{
	public class MicroserviceActionPrompt : MicroserviceComponent
	{
		private VisualElement _container;
		private Button _closeButton;
		private Label _label;

		public new class UxmlFactory : UxmlFactory<MicroserviceActionPrompt, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			// Do we need this?
			UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription
			{ name = "custom-text", defaultValue = "nada" };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get { yield break; }
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				// Do we need this?
				var self = ve as MicroserviceActionPrompt;
			}
		}

		public MicroserviceActionPrompt() : base(nameof(MicroserviceActionPrompt))
		{
		}

		public override void Refresh()
		{
			base.Refresh();

			_container = Root.Q<VisualElement>("mainContainer");
			_label = Root.Q<Label>("label");
			_closeButton = Root.Q<Button>("closeButton");

			_closeButton.clickable.clicked += OnCloseButtonClicked;
		}

		private void OnCloseButtonClicked()
		{
			SetVisible(string.Empty, false);
		}

		public void SetVisible(string label, bool isVisible, bool success = true)
		{
			_label.text = label;

			if (isVisible)
			{
				_container.AddToClassList("visible");

				if (!success)
				{
					_container.AddToClassList("failed");
				}
			}
			else
			{
				_container.RemoveFromClassList("visible");
				_container.RemoveFromClassList("failed");
			}
		}
	}
}
