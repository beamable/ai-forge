#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Login.UI.Components
{
	public class LegalCopyVisualElement : LoginBaseComponent
	{
		public LegalCopyVisualElement() : base(nameof(LegalCopyVisualElement))
		{
		}
		public override string GetMessage()
		{
			return "TERMS OF USE";
		}
		public override void Refresh()
		{
			base.Refresh();

			var backButton = Root.Q<Button>("cancel");
			backButton.clickable.clicked += Manager.GoToPreviousPage;

			Root.Q<Label>("legalDoc")?.AddTextWrapStyle();

			var toggle = Root.Q<Toggle>();
			toggle.SetValueWithoutNotify(Model.ReadLegalCopy);
			toggle.RegisterValueChangedCallback(evt => Model.ReadLegalCopy = evt.newValue);
		}
	}
}
