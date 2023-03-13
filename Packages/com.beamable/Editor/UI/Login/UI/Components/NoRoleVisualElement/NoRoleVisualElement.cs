#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
namespace Beamable.Editor.Login.UI.Components
{
	public class NoRoleVisualElement : LoginBaseComponent
	{
		public NoRoleVisualElement() : base(nameof(NoRoleVisualElement))
		{
		}

		public override string GetMessage()
		{
			return "One moment!";
		}

		public override void Refresh()
		{
			base.Refresh();

			var message = Root.Q<Label>();
			message.AddTextWrapStyle();

			var button = Root.Q<Button>();
			button.clickable.clicked += Manager.GotoSummary;
		}
	}
}
