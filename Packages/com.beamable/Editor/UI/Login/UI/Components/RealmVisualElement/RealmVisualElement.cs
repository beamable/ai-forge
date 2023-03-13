using Beamable.Common.Api.Realms;
using System;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif


namespace Beamable.Editor.Login.UI.Components
{
	public class RealmVisualElement : LoginBaseComponent
	{

		public event Action<RealmView> OnSelected;
		public RealmVisualElement() : base(nameof(RealmVisualElement))
		{
		}

		public RealmView Realm;
		private Button _clickableBackground;

		public override void Refresh()
		{
			base.Refresh();

			_clickableBackground = Root.Q<Button>("clickableBackground");
			// _clickableBackground.clickable.clicked += ToggleOnSelected;
			_clickableBackground.clickable.clicked += () => OnSelected?.Invoke(Realm);

			Label realmNameLabel = Root.Q<Label>("realmName");
			realmNameLabel.text = Realm.ProjectName;

			Label pidLabel = Root.Q<Label>("pID");
			pidLabel.text = Realm.Pid;

			VisualElement realmCard = Root.Q<VisualElement>("card");
			//realmCard.Select() +=() => OnSelected?.Invoke(Realm);
		}

		// private void ToggleOnSelected()
		// {
		//     if (_selected)
		//     {
		//
		//     }
		//     else
		//     {
		//         _clickableBackground.AddToClassList("borderSelected");
		//     }
		// }
	}
}
