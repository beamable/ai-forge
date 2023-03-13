using Beamable.Api;
using Beamable.Avatars;
using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Stats;
using Beamable.Theme;
using Beamable.Theme.Palettes;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Beamable.AccountManagement
{

	public class AccountCustomizationBehaviour : MonoBehaviour
	{

		public AccountDisplayItem DisplayItem;

		public List<GameObject> EditComponents;
		public List<GameObject> RemoveWhileEditComponents;

		public UnityEvent OnOpened, OnClosed;
		public LoadingEvent OnSaving;

		public StatBehaviour AvatarStatBehaviour;
		public StatBehaviour AliasStatBehaviour;
		public InputReference AliasReference;
		public TextMeshProUGUI AliasPlaceholder;

		public GameObject EmailContainer, FacebookContainer, AppleContainer, GoogleContainer;
		public TextMeshProUGUI EmailText;

		public AvatarPickerBehaviour AvatarPickerBehaviour;

		public ColorBinding PlaceholderDefaultColor, PlaceholderErrorColor;
		public string PlaceholderMessage = "Enter Alias...", PlaceholderErrorMessage = "Enter a valid Alias...";

		// Start is called before the first frame update
		void Start()
		{
			Refresh();
			Hide();
		}

		// Update is called once per frame
		void Update()
		{

		}

		public void Refresh()
		{

			AvatarStatBehaviour.Read().Then(statAvatar =>
			{
				AvatarPickerBehaviour.Select(statAvatar);
			});
		}

		public void ResetPlaceholder()
		{
			if (string.Equals(AliasPlaceholder.text, PlaceholderMessage)) return;
			AliasPlaceholder.text = PlaceholderMessage;
			AliasPlaceholder.color =
				ThemeConfiguration.Instance.Style.ColorPalette.Find(PlaceholderDefaultColor).Color;
		}

		public void Save()
		{
			var saveOperations = new List<Promise<Unit>>();

			// save the avatar...
			if (AvatarPickerBehaviour.Selected != null)
			{
				saveOperations.Add(AvatarStatBehaviour.Write(AvatarPickerBehaviour.Selected.Name));
			}

			saveOperations.Add(AliasStatBehaviour.Write(AliasReference.Value).Error(err =>
			{
				// set an error message on the input box...
				AliasReference.Value = "";
				AliasPlaceholder.text = PlaceholderErrorMessage;
				var errorColor = ThemeConfiguration.Instance.Style.GetPaletteStyle(PlaceholderErrorColor);
				AliasPlaceholder.color = errorColor.Color;
			}));

			var loadingArg = Promise.Sequence(saveOperations).Then(_ =>
			{
				Close();
			}).Error(err =>
			{
				if (err is PlatformRequesterException ex)
				{
					Debug.LogError($"Could not save account stats. {ex.Error.status} {ex.Error.error}");
				}
			}).ToLoadingArg("Saving...", false);
			OnSaving?.Invoke(loadingArg);
		}

		public void Cancel()
		{
			AvatarStatBehaviour.Refresh(); // reset the avatar to whatever is was.
			AliasStatBehaviour.Refresh();
			Close();
		}


		public void Open()
		{
			AliasStatBehaviour.SetForUser(DisplayItem.User);
			AvatarStatBehaviour.SetForUser(DisplayItem.User);
			AliasReference.Field.characterLimit = AccountManagementConfiguration.Instance.AliasCharacterLimit;


			AvatarPickerBehaviour.Selected = null;
			AccountManagementConfiguration.Instance.GetAllEnabledThirdPartiesForUser(DisplayItem.User).Then(assocs =>
				{
					var iconEnablement = assocs.Select(assoc => new IconEnableData
					{
						HasThirdParty = assoc.ShouldShowIcon,
						IconGameObject = GetIconForThirdParty(assoc.ThirdParty)
					}).ToList();

					EditComponents.ForEach(g => g?.SetActive(true));
					RemoveWhileEditComponents.ForEach(g => g?.SetActive(false));
					AvatarStatBehaviour.Read().Then(statAvatar =>
					{
						AvatarPickerBehaviour.Select(statAvatar);
					});

					SetUserInfo(iconEnablement);

					OnOpened?.Invoke();
				});


		}

		private void Hide()
		{
			EditComponents.ForEach(g => g?.SetActive(false));
			RemoveWhileEditComponents.ForEach(g => g?.SetActive(true));
		}

		public void Close()
		{
			Hide();
			OnClosed?.Invoke();
		}

		GameObject GetIconForThirdParty(AuthThirdParty thirdParty)
		{
			switch (thirdParty)
			{
				case AuthThirdParty.Apple:
					return AppleContainer;
				case AuthThirdParty.Facebook:
					return FacebookContainer;
				case AuthThirdParty.Google:
					return GoogleContainer;
				default:
					return null;
			}
		}

		void SetUserInfo(List<IconEnableData> data)
		{
			var userHasEmail = DisplayItem.User.HasDBCredentials();
			EmailContainer.SetActive(userHasEmail);
			EmailText.text = DisplayItem.User.email;
			foreach (var iconEnable in data)
			{
				if (iconEnable.IconGameObject != null)
				{
					iconEnable.IconGameObject.SetActive(iconEnable.HasThirdParty);
				}
			}
		}
	}
}
