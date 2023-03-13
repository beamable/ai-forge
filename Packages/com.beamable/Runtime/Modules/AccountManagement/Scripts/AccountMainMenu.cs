using Beamable.Common;
using Beamable.Common.Api.Auth;
using Beamable.Coroutines;
using Beamable.UI.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Beamable.AccountManagement
{
	public class AccountMainMenu : MenuBase
	{
		public AccountDisplayItem _accountObj;
		public GameObject _promoObj;
		public GameObject _promoPagination;
		public GameObject _switchAccountObj;
		public GameObject _developerLogo;
		public Button emailButton, facebookButton, appleButton, googleButton;
		public TextMeshProUGUI emailText, facebookText, appleText;
		public Transform otherAccountsContainer;
		public AccountDisplayItem AccountDisplayPrefab;
		public LoadingIndicator LoadingIndicator;
		public UnityEvent OnInitialize;
		public AccountManagementBehaviour AccountBehaviour;
		public List<TextMeshProUGUI> PromoTextElements;
		public MediaSourceBehaviour MediaSource;
		public RectTransform OtherAccountsScroller;
		public LayoutElement ExpandAccountButton;
		public RectTransform ExpandAccountVisual;
		public AccountCustomizationBehaviour CustomizationBehaviour;
		public bool ShowMoreAccounts;
		private bool _wereOtherAccountsHidden;
		private bool _isAccountEditorOpen;
		private User _user;
		private Promise<User> _mainAccountPromise;
		private Promise<DeviceUserArg> _otherAccountsPromise;
		private Promise<Unit> _allUsersPromise;

		private void Start()
		{
			_accountObj.gameObject.SetActive(false);
			appleButton.gameObject.SetActive(false);
			facebookButton.gameObject.SetActive(false);
			googleButton.gameObject.SetActive(false);
			_switchAccountObj.gameObject.SetActive(false);
			emailButton.gameObject.SetActive(false);
		}

		private void Update()
		{
			var areOtherAccountsHidden =
			   OtherAccountsScroller.rect.height < (otherAccountsContainer as RectTransform).rect.height;
			if (areOtherAccountsHidden != _wereOtherAccountsHidden)
			{
				_wereOtherAccountsHidden = areOtherAccountsHidden;
				SetComponentsForMoreAccounts();
			}
		}

		public IEnumerator SchedulePositionPagination()
		{
			yield return Yielders.EndOfFrame;
			PositionPagination();
		}

		public void PositionPagination()
		{
			if (!_promoObj.gameObject.activeInHierarchy) return;
			var promoPaginationTransform = _promoPagination.transform;
			var promoPadding = 30;

			// find largest text element
			var existingParent = promoPaginationTransform.parent;
			RectTransform textTransform = null;
			foreach (var promoTextTransform in PromoTextElements.Select(e => e.transform as RectTransform))
			{
				if (textTransform == null || promoTextTransform.rect.height > textTransform.rect.height)
				{
					textTransform = promoTextTransform;
				}
			}

			// parent to the largest text element.
			promoPaginationTransform.SetParent(textTransform);

			// reset position
			promoPaginationTransform.localPosition = new Vector3(promoPaginationTransform.localPosition.x,
			   -textTransform.rect.height - promoPadding, 0);

			// unparent, but keep position
			promoPaginationTransform.SetParent(existingParent);
			_promoPagination.gameObject.SetActive(true);
		}

		public void ToggleShowMoreAccounts()
		{
			ShowMoreAccounts = !ShowMoreAccounts;
			if (!ShowMoreAccounts)
			{
				CustomizationBehaviour.Cancel();
			}

			SetComponentsForMoreAccounts();
		}

		public void ShowThirdPartyScreen(ThirdPartyLoginPromise arg)
		{
			var menu = Manager.Show<AccountThirdPartyWaitingMenu>();
			menu.SetFor(arg.ThirdParty);
			arg.Then(response =>
			{
				if (response.Cancelled)
				{
					Manager.GoBack();
				}
			}).Error(err =>
			{
				var errorMenu = Manager.Show<AccountGeneralErrorBehaviour>();
				errorMenu.ErrorText.Value = err.Message;
			});
		}

		void SetComponentsForMoreAccounts()
		{
			var isLandscape = MediaSource.Calculate();

			// in landscape mode, the drawer button should always be visible.
			// in landscape mode, an open drawer, OR an open editor should hide the dev logo
			// in landscape mode, the device accounts should never be hidden.

			// in portrait mode, the device accounts should hide when the editor is open
			// in portrait mode, the dev logo only goes away when the drawer is open
			// in portrait mode, the drawer button is only visible when it is out of space.
			if (isLandscape)
			{
				var hideDevLogo = (ShowMoreAccounts || _isAccountEditorOpen);
				ExpandAccountButton.gameObject.SetActive(_wereOtherAccountsHidden || ShowMoreAccounts);
				OtherAccountsScroller.gameObject.SetActive(true);
				_developerLogo.SetActive(!hideDevLogo);
			}
			else
			{
				ExpandAccountButton.gameObject.SetActive(false);
				_developerLogo.gameObject.SetActive(true);
				OtherAccountsScroller.gameObject.SetActive(!_isAccountEditorOpen);
			}

			ExpandAccountVisual.eulerAngles = new Vector3(0, 0, 90 * (ShowMoreAccounts ? 1 : -1));
		}

		public void OnAccountEditOpened()
		{
			_isAccountEditorOpen = true;
			var isLandscape = MediaSource.Calculate();
			ShowMoreAccounts |= isLandscape;
			SetComponentsForMoreAccounts();
			DisablePromotion();
		}

		public void OnAccountEditClosed()
		{
			_isAccountEditorOpen = false;
			ShowMoreAccounts = false;
			SetComponentsForMoreAccounts();
			TogglePromotion();
		}

		public void GoToSelectMenu(AccountDisplayItem item)
		{
			AccountManagementSignals.SetPending(item.TokenReference.Bundle.User, item.TokenReference.Bundle.Token);
			var menu = Manager.Show<AccountExistsSelect>();
			menu.SetForUserImmediate(item);
		}

		protected void OnEnable()
		{
			_accountObj.gameObject.SetActive(false);
			appleButton.gameObject.SetActive(false);
			facebookButton.gameObject.SetActive(false);
			googleButton.gameObject.SetActive(false);
			_switchAccountObj.gameObject.SetActive(false);
			emailButton.gameObject.SetActive(false);

			// empty children, and re-populate.
			for (var i = 0; i < otherAccountsContainer.childCount; i++)
			{
				Destroy(otherAccountsContainer.GetChild(i).gameObject);
			}

			DisablePromotion();
			_mainAccountPromise = new Promise<User>();
			_otherAccountsPromise = new Promise<DeviceUserArg>();
			OnInitialize?.Invoke();
			_allUsersPromise = Promise
			   .Sequence(new List<Promise<Unit>> { _mainAccountPromise.ToUnit(), _otherAccountsPromise.ToUnit() }).ToUnit();
			var entireLoadingSession = new Promise<Unit>();
			LoadingIndicator.Show(entireLoadingSession.ToLoadingArg());
			_allUsersPromise.Then(_ =>
			{
				var user = _mainAccountPromise.GetResult();
				var data = _otherAccountsPromise.GetResult();
				var mainAccountLoading = _accountObj.StartLoading(user, true, null);
				var allAccountPromises = new List<Promise<AccountDisplayItem>> { mainAccountLoading };
				foreach (var otherAccount in data.OtherUsers)
				{
					var accountDisplay = Instantiate(AccountDisplayPrefab, otherAccountsContainer);
					accountDisplay.gameObject.SetActive(false);
					accountDisplay.AccountButton.onClick.AddListener(() => GoToSelectMenu(accountDisplay));
					var promise = accountDisplay.StartLoading(otherAccount.User, false, otherAccount.Token);
					allAccountPromises.Add(promise);
				}

				Promise.Sequence(allAccountPromises).Then(set =>
			 {
				 SetButtonsForUser(_user);
				 TogglePromotion();
				 foreach (var accountPromise in set)
				 {
					 accountPromise.Apply();
				 }

				 // LOADING IS FINALLY TOTALLY DONE.
				 entireLoadingSession.CompleteSuccess(PromiseBase.Unit);
			 });
			});
		}

		public void OnUserAvailable(User user)
		{
			_user = user;
			_mainAccountPromise.CompleteSuccess(user);
		}

		public void OnAnonymousUser(User user)
		{
			_user = user;
			_mainAccountPromise.CompleteSuccess(user);
		}

		public void OnOtherAccountsAvailable(DeviceUserArg data)
		{
			_otherAccountsPromise.CompleteSuccess(data);
		}

		void TogglePromotion()
		{
			var areOtherAccountsAvailable = otherAccountsContainer.childCount > 0;
			var isCurrentAccountAnonymous = _user == null || !_user.HasAnyCredentials();
			var isCurrentAccountVisible = _accountObj.gameObject.activeInHierarchy;
			if (areOtherAccountsAvailable || !isCurrentAccountAnonymous || !isCurrentAccountVisible)
			{
				DisablePromotion();
			}
			else
			{
				EnablePromotion();
			}
		}

		void EnablePromotion()
		{
			if (AccountManagementConfiguration.Instance.ShowPromotionalSlider)
			{
				_promoObj.gameObject.SetActive(true);
				OtherAccountsScroller.gameObject.SetActive(false);
				StartCoroutine(SchedulePositionPagination());
			}
		}

		void DisablePromotion()
		{
			_promoObj.gameObject.SetActive(false);
			_promoPagination.gameObject.SetActive(false);
			OtherAccountsScroller.gameObject.SetActive(true);
		}

		void SetButtonsForUser(User user)
		{
			_accountObj.ThirdPartyAssociationPromise.Then(assocs =>
			{
				emailButton.gameObject.SetActive(!user.HasDBCredentials());
				_switchAccountObj.SetActive(assocs.Count > 0 || user.HasDBCredentials());
				foreach (var assoc in assocs)
				{
					switch (assoc.ThirdParty)
					{
						case AuthThirdParty.Apple:
							appleButton.gameObject.SetActive(assoc.ShouldShowButton);
							break;
						case AuthThirdParty.Facebook:
							facebookButton.gameObject.SetActive(assoc.ShouldShowButton);
							break;
						case AuthThirdParty.Google:
							googleButton.gameObject.SetActive(assoc.ShouldShowButton);
							break;
					}
				}
			});
		}

		public void OnEmailPressed()
		{
			Manager.Show<AccountEmailLogin>();
		}

		public void OnSwitchAccountPressed()
		{
			Manager.Show<AccountSwitch>();
		}
	}
}
