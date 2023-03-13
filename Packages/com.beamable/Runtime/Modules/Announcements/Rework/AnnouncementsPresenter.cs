using Beamable.Announcements;
using Beamable.Modules.Generics;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Beamable.Modules.Content
{
	public class AnnouncementsPresenter : CollectionPresenter<AnnouncementsCollection>
	{
#pragma warning disable CS0649
		[SerializeField] private GameObject _announcementRowPrefab;
		[SerializeField] private TextMeshProUGUI _noItemsText;
		[SerializeField] private RectTransform _listRoot;
		[SerializeField] private Button _closeButton;
#pragma warning restore CS0649

		private void Awake()
		{
			_closeButton.onClick.AddListener(() => gameObject.SetActive(false));
		}

		private void OnEnable()
		{
			Collection = new AnnouncementsCollection(OnCollectionUpdated);
		}

		private void OnDisable()
		{
			Collection?.Unsubscribe();
			Collection = null;
		}

		private void OnCollectionUpdated()
		{
			_noItemsText.gameObject.SetActive(Collection.Count == 0);

			ClearItems();

			foreach (var announcement in Collection)
			{
				AnnouncementSummary row = Instantiate(_announcementRowPrefab, _listRoot).GetComponent<AnnouncementSummary>();
				Assert.IsNotNull(row, $"Instantiation of {nameof(AnnouncementSummary)} failed");
				row.Setup(announcement.title, announcement.body);
			}
		}

		private void ClearItems()
		{
			foreach (Transform child in _listRoot)
			{
				Destroy(child.gameObject);
			}
		}

		public async void OnClaimAll()
		{
			List<string> ids = new List<string>();
			foreach (var announcement in Collection)
			{
				ids.Add(announcement.id);
			}

			var api = await API.Instance;
			await api.AnnouncementService.Claim(ids);
		}
	}
}
