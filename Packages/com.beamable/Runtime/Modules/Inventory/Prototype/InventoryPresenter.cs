using Beamable.Common.Api.Inventory;
using Beamable.Modules.Generics;
using Beamable.Modules.Inventory.LanguageLocalization;
using Beamable.Modules.Inventory.Prototypes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Modules.Inventory
{
	public class InventoryPresenter : CollectionPresenter<ItemsCollection>
	{
#pragma warning disable CS0649
		[Header("References")]
		[SerializeField] private GenericTextButton _closeButton;
		[SerializeField] private GameObject _loadingIndicator;
		[SerializeField] private GameObject _itemPrefab;
		[SerializeField] private RectTransform _itemsContainer;
		[SerializeField] private ScrollRect _scrollRect;
		[SerializeField] private ContentSizeFitter _contentSizeFitter;

		[Header("Parameters")]
		[SerializeField] private GridLayoutGroup.Axis _direction;
		[SerializeField] private bool _groupItems = true;
		[SerializeField] private int _itemsSpacing = 20;
#pragma warning restore CS0649

		private readonly List<ItemPresenter> _spawnedItems = new List<ItemPresenter>();

		private void Awake()
		{
			SetupScrollView();
			SetupButtons();
		}

		private void OnEnable()
		{
			if (Collection == null)
			{
				Collection = new ItemsCollection(OnCollectionUpdated);
			}
		}

		private void OnDisable()
		{
			Collection?.Unsubscribe();
			Collection = null;
		}

		private void SetupScrollView()
		{
			switch (_direction)
			{
				case GridLayoutGroup.Axis.Horizontal:
					SetupHorizontalScroll();
					break;
				case GridLayoutGroup.Axis.Vertical:
					SetupVerticalScroll();
					break;
				default:
					SetupVerticalScroll();
					break;
			}
		}

		private void SetupHorizontalScroll()
		{
			_scrollRect.vertical = false;
			_scrollRect.horizontal = true;
			_contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			_contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

			GridLayoutGroup layoutGroup = _itemsContainer.gameObject.AddComponent<GridLayoutGroup>();
			layoutGroup.startAxis = _direction;
			layoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
			layoutGroup.constraintCount = 1;
			// TODO: maybe we could expose this to customer in future, now we are constraining this just to make items look "properly"
			layoutGroup.cellSize = new Vector2(300.0f, 600.0f);
			layoutGroup.spacing = new Vector2(_itemsSpacing, _itemsSpacing);
		}

		private void SetupVerticalScroll()
		{
			_scrollRect.vertical = true;
			_scrollRect.horizontal = false;
			_contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
			_contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			GridLayoutGroup layoutGroup = _itemsContainer.gameObject.AddComponent<GridLayoutGroup>();
			layoutGroup.startAxis = _direction;
			layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			layoutGroup.constraintCount = 1;
			// TODO: maybe we could expose this to customer in future, now we are constraining this just to make items look "properly" 
			layoutGroup.cellSize = new Vector2(700.0f, 320.0f);
			layoutGroup.spacing = new Vector2(_itemsSpacing, _itemsSpacing);
		}

		private void SetupButtons()
		{
			_closeButton.Setup(() =>
			{
				gameObject.SetActive(false);
			});
		}

		private void OnCollectionUpdated()
		{
			_loadingIndicator.SetActive(Collection.Count == 0);

			ClearItems();

			if (_groupItems)
			{
				SpawnGroupedItems(Collection);
			}
			else
			{
				SpawnIndividualItems(Collection);
			}
		}

		private void ClearItems()
		{
			foreach (ItemPresenter itemPresenter in _spawnedItems)
			{
				Destroy(itemPresenter.gameObject);
			}

			_spawnedItems.Clear();
		}

		private void SpawnGroupedItems(ItemsCollection collection)
		{
			foreach (ItemGroupData data in collection)
			{
				ItemPresenter newPresenter = Instantiate(_itemPrefab, _itemsContainer.transform, false)
					.GetComponent<ItemPresenter>();

				newPresenter.Setup(data.Content.icon, LocalizationHelper.GetItemName(data.Content.Id), data.Items.Count);
				_spawnedItems.Add(newPresenter);
			}
		}

		private void SpawnIndividualItems(ItemsCollection collection)
		{
			foreach (ItemGroupData data in collection)
			{
				foreach (ItemView itemData in data.Items)
				{
					ItemPresenter newPresenter = Instantiate(_itemPrefab, _itemsContainer.transform, false)
						.GetComponent<ItemPresenter>();

					newPresenter.Setup(data.Content.icon, LocalizationHelper.GetItemName(data.Content.Id), 0);
					_spawnedItems.Add(newPresenter);
				}
			}
		}
	}
}
