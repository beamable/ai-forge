using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Stats;
using Beamable.Theme;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.AccountManagement
{
	public class AccountDisplayItem : MonoBehaviour
	{

		public TextMeshProUGUI _emailTxt, _progressTxt;
		public Image _statusImg;
		public StyleBehaviour StatusSelectedBehaviour, StatusDeleteBehaviour;
		public GameObject EmailIcon, FacebookIcon, AppleIcon, GoogleIcon;
		public List<GameObject> RemoveAccountElements;
		public Button AccountButton;
		public TextReference AttachedEmailTextReference;

		private bool _isActiveAccount = false;

		public TokenReference TokenReference;

		public string SubTextLabel;
		public StatBehaviour DisplayNameStat;
		public StatBehaviour SubTextStat;

		public StatBehaviour AvatarStat;
		public AccountAvatarBehaviour AvatarBehaviour;
		public User User { get; private set; }
		public bool IsCurrent { get; private set; }

		public Promise<List<AccountManagementConfiguration.UserThirdPartyAssociation>> ThirdPartyAssociationPromise { get; private set; }
		public Promise<UserExtensions.StatCollection> StatPromise { get; private set; }

		public Promise<AccountDisplayItem> LoadingPromise { get; private set; }

		private bool _initializedStatCallbacks = false;

		void Awake()
		{
			InitializeStatCallbacks();
		}

		void InitializeStatCallbacks()
		{
			if (_initializedStatCallbacks) return;
			_initializedStatCallbacks = true;
			DisplayNameStat.OnStatReceived.AddListener(OnAliasAvailable);
			SubTextStat.OnStatReceived.AddListener(OnSubtextAvailable);
		}

		public void Apply()
		{

			// first, await the entire operation.
			LoadingPromise.Then(_ =>
			{

				// apply the third party icons...
				ThirdPartyAssociationPromise.Then(assocs =>
				{
					EmailIcon.SetActive(User.HasDBCredentials());
					if (AttachedEmailTextReference != null)
					{
						AttachedEmailTextReference.Value = User.email;
					}

					foreach (var assoc in assocs)
					{
						switch (assoc.ThirdParty)
						{
							case AuthThirdParty.Apple:
								AppleIcon.SetActive(assoc.ShouldShowIcon);
								break;
							case AuthThirdParty.Facebook:
								FacebookIcon.SetActive(assoc.ShouldShowIcon);
								break;
							case AuthThirdParty.Google:
								GoogleIcon.SetActive(assoc.ShouldShowIcon);
								break;
						}
					}

					LayoutCredentialIcons();
				});

				// apply the stat data
				StatPromise.Then(stats =>
				{
					InitializeStatCallbacks();
					var config = AccountManagementConfiguration.Instance;
					DisplayNameStat.SetStat(config.DisplayNameStat);
					SubTextStat.SetStat(config.SubtextStat);
					AvatarStat.SetStat(config.AvatarStat);
					SubTextLabel = config.SubtextLabel;

					DisplayNameStat.SetForUser(User);
					SubTextStat.SetForUser(User);
					AvatarStat.SetForUser(User);

					var alias = stats.Get(DisplayNameStat.Stat);
					var subtext = stats.Get(SubTextStat.Stat);
					var avatar = stats.Get(AvatarStat.Stat);

					var aliasValue = alias ?? (User.email ?? "Anonymous");
					var subtextValue = subtext ?? (User.id.ToString());
					SubTextStat.DefaultValueOverride = subtextValue;
					AvatarStat.DefaultValueOverride = avatar;
					DisplayNameStat.SetCurrentValue(aliasValue);
					SubTextStat.SetCurrentValue(subtextValue);
					AvatarStat.SetCurrentValue(avatar);
					DisplayNameStat.RefreshOnStart = true;
					SubTextStat.RefreshOnStart = true;
					AvatarStat.RefreshOnStart = true;
				});

				SetActiveAccount(IsCurrent);
				SetRemoveAccountElements(false);
				gameObject.SetActive(true);
			});

		}

		void LayoutCredentialIcons()
		{
			var parent = EmailIcon.transform.parent as RectTransform;

			/*
		  * we want to layout icons right-justified, based on the gameobject's enabled state
		  */

			var iconWidth = 20;
			var activeCount = 0;
			for (var i = 0; i < parent.childCount; i++)
			{
				var child = parent.GetChild(i) as RectTransform;
				if (child.gameObject.activeInHierarchy)
				{
					child.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, activeCount * iconWidth, iconWidth);
					activeCount += 1;
				}
			}
		}

		public void SetRemoveAccountElements(bool active)
		{
			if (AccountButton == null) return;

			AccountButton.interactable = !active;
			foreach (var element in RemoveAccountElements)
			{
				element?.SetActive(active);
			}
		}

		public void SetActiveAccount(bool active)
		{
			if (StatusSelectedBehaviour != null)
			{
				StatusSelectedBehaviour.enabled = active;
			}

			if (StatusDeleteBehaviour != null)
			{
				StatusDeleteBehaviour.enabled = !active;
			}
		}

		public void OnSubtextAvailable(string subText)
		{
			_progressTxt.text = $"{SubTextLabel}{subText}";
		}

		public void OnAliasAvailable(string alias)
		{
			_emailTxt.text = alias;
		}

		public void OnStatusClicked()
		{
			if (!_isActiveAccount)
			{
				SetRemoveAccountElements(true);
			}
		}

		public void Destroy()
		{
			GameObject.Destroy(gameObject);
		}

		public Promise<AccountDisplayItem> StartLoading(AccountDisplayItem clone)
		{
			User = clone.User;
			IsCurrent = clone.IsCurrent;
			if (TokenReference != null)
			{
				TokenReference.Bundle = clone.TokenReference.Bundle;
			}

			StatPromise = clone.StatPromise;
			ThirdPartyAssociationPromise = clone.ThirdPartyAssociationPromise;
			LoadingPromise = clone.LoadingPromise;

			return LoadingPromise;
		}

		public Promise<AccountDisplayItem> StartLoading(User user, bool isCurrent, TokenResponse token)
		{
			User = user;
			IsCurrent = isCurrent;
			if (TokenReference != null && token != null)
			{
				TokenReference.Bundle = new UserBundle
				{
					User = user,
					Token = token
				};
			}

			StatPromise = user.GetStats(
			   AccountManagementConfiguration.Instance.DisplayNameStat,
			   AccountManagementConfiguration.Instance.SubtextStat,
			   AccountManagementConfiguration.Instance.AvatarStat);

			ThirdPartyAssociationPromise = AccountManagementConfiguration.Instance.GetAllEnabledThirdPartiesForUser(User);

			LoadingPromise = Promise.Sequence(StatPromise.ToUnit(), ThirdPartyAssociationPromise.ToUnit()).Map(_ => this);

			return LoadingPromise;
		}

	}

	struct IconEnableData
	{
		public bool HasThirdParty;
		public GameObject IconGameObject;
	}
}
