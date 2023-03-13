using Beamable.Api;
using Beamable.Api.Payments;
using Beamable.Common;
using Beamable.Common.Shop;
using Beamable.Shop.Defaults;
using Beamable.UI.Layouts;
using Beamable.UI.Scripts;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Beamable.Common.Constants.URLs;
using Button = UnityEngine.UI.Button;

namespace Beamable.Shop
{
	[HelpURL(Documentations.URL_DOC_STORE_FLOW)]
	public class ShopFlow : MonoBehaviour
	{
		public MenuManagementBehaviour MenuManager;
		public Button StoreButton;
		public ReparenterBehaviour StoreButtonLayout;

		private Dictionary<StoreRef, PlayerStoreView> _storeData = new Dictionary<StoreRef, PlayerStoreView>();
		//private Dictionary<StoreRef, Promise<Func<PlayerStoreView>>> _storeRefToDataDelegate = new Dictionary<StoreRef, Promise<Func<PlayerStoreView>>>();
		private Promise<List<PlatformSubscription<PlayerStoreView>>> _subscriptionsPromise = new Promise<List<PlatformSubscription<PlayerStoreView>>>();
		private bool _hasCreatedSubscriptions = false;


		private Dictionary<StoreRef, Promise<Unit>> _isStoreAvailable = new Dictionary<StoreRef, Promise<Unit>>();

		void Start()
		{
			SetupStoreSubscriptions();
		}

		async void SetupStoreSubscriptions()
		{
			if (_hasCreatedSubscriptions) return;
			_hasCreatedSubscriptions = true;
			var configuration = ShopConfiguration.Instance;
			var de = await API.Instance;

			var allSubscriptions = new List<PlatformSubscription<PlayerStoreView>>();

			for (var i = 0; i < StoreButtonLayout.CurrentParent.childCount; i++)
			{
				Destroy(StoreButtonLayout.CurrentParent.GetChild(i).gameObject);
			}

			for (var i = 0; i < configuration.Stores.Count; i++)
			{
				var store = configuration.Stores[i];

				var btn = Instantiate(StoreButton, StoreButtonLayout.CurrentParent);
				_isStoreAvailable.Add(store, new Promise<Unit>());

				btn.onClick.AddListener(() => RenderShop(store));

				var subscription = de.CommerceService.Subscribe(store.Id, (data) =>
				{
					_storeData[store] = data;
					_isStoreAvailable[store].CompleteSuccess(PromiseBase.Unit);

					btn.gameObject.SetActive(true);
					btn.GetComponentInChildren<TextMeshProUGUI>().text = data.title;
				});

				if (i == 0)
				{
					RenderShop(store);
				}
				allSubscriptions.Add(subscription);
			}

			_subscriptionsPromise.CompleteSuccess(allSubscriptions);
		}

		void RenderShop(StoreRef store)
		{
			if (!_subscriptionsPromise.IsCompleted)
			{
				MenuManager.Show<ShopLoadingMenu>();
				_subscriptionsPromise.Then(_ => RenderShop(store));
				return; // simply wait for all the subscriptions to be created, before doing anything...
			}

			if (_storeData.TryGetValue(store, out var storeData))
			{
				// go ahead and render
				if (MenuManager.CurrentMenu is BasicStoreRenderer storeMenu && storeMenu.Store == storeData)
				{
					return; // don't show if menu is already loaded with correct data.
				}
				MenuManager.Show<BasicStoreRenderer>(menu => { menu.Store = storeData; });

			}
			else
			{
				// show loading screen, and wait....
				MenuManager.Show<ShopLoadingMenu>();
				_isStoreAvailable[store].Then(_ => RenderShop(store));
			}
		}

		public void OnToggleShop(bool shouldShow)
		{
			if (!shouldShow && MenuManager.IsOpen)
			{
				MenuManager.CloseAll();
			}
			else if (shouldShow && !MenuManager.IsOpen)
			{
				var firstStoreRef = ShopConfiguration.Instance.Stores[0];
				RenderShop(firstStoreRef);
			}
		}

		public void RenderReward(PlayerListingView data)
		{
			MenuManager.Show<ShopRewardRenderer>(menu => { menu.Listing = data; });
		}

	}
}
