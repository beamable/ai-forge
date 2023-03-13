using Beamable.Avatars;
using Beamable.Common;
using Beamable.Stats;
using Beamable.Theme.Palettes;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace Beamable.AccountManagement
{
	public class AccountPlayerDataMenu : MenuBase
	{
		public StatBehaviour AliasStat;
		public StatBehaviour AvatarStat;

		public AvatarPickerBehaviour AvatarPickerBehaviour;
		public InputReference AliasInput;
		public TextMeshProUGUI Placeholder;

		public InputValidationBehaviour AliasInputValidation;
		public Button SaveButton;

		public StringBinding PlaceholderString, ErrorPlaceholderString;
		public ColorBinding PlaceholderColor, ErrorPlaceholderColor;

		public Promise<Unit> OpenDurationPromise => _openDurationPromise;
		private Promise<Unit> _openDurationPromise;

		public override void OnOpened()
		{
			base.OnOpened();

			AliasStat.Stat = AccountManagementConfiguration.Instance.DisplayNameStat;
			AvatarStat.Stat = AccountManagementConfiguration.Instance.AvatarStat;

			AliasStat.Refresh();
			AvatarStat.Refresh();

			AvatarStat.Read().Then(avatarValue => { AvatarPickerBehaviour.Select(avatarValue); });
			AliasInput.Value = "";

			_openDurationPromise = new Promise<Unit>();
		}

		public override void OnWentBack()
		{
			base.OnWentBack();
			// TODO allow override for this operation...
			Manager.GoBackToPage<AccountMainMenu>();
		}

		private void Update()
		{
			var isSaveValid = AliasInputValidation.gameObject.activeInHierarchy && AliasInputValidation.IsValid;
			SaveButton.interactable = isSaveValid;
		}

		public void ResetPlaceholder()
		{
			AliasInput.Field.placeholder.color = PlaceholderColor.Resolve().Color;
			Placeholder.text = PlaceholderString.Localize();
		}

		public void Save()
		{
			var saveOperations = new List<Promise<Unit>>();
			if (AvatarPickerBehaviour.Selected != null)
			{
				saveOperations.Add(AvatarStat.Write(AvatarPickerBehaviour.Selected.Name));
			}
			else
			{
				saveOperations.Add(AvatarStat.Write(AvatarConfiguration.Instance.Default.Name));
			}

			var alias = string.IsNullOrEmpty(AliasInput.Value) ? AliasStat.Stat.DefaultValue : AliasInput.Value;
			saveOperations.Add(AliasStat.Write(alias).Error(err =>
			{
				AliasInput.Value = "";
				Placeholder.text = ErrorPlaceholderString.Localize();
				Placeholder.color = ErrorPlaceholderColor.Resolve().Color;
			}));

			Promise.Sequence(saveOperations).Then(_ =>
			{
				API.Instance.Then(de =>
			 {
				 AccountManagementConfiguration.Instance.Overrides.HandleAnonymousUserDataUpdated(Manager, de.User);
			 });
			});
		}
	}
}
