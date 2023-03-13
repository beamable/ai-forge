using System.Collections.Generic;
using System.Linq;
using Beamable.Player;
using Game.Helpers;
using Game.Utils;
using Game.ViewModel;
using Helpers;
using UnityEngine;
using UnityEngine.UI;
using UnityWeld.Binding;

namespace Game.GUI
{
    [RequireComponent(typeof(DefaultPoolSystem))]
    public class ItemsList : MonoBehaviour
    {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private DefaultPoolSystem _poolSystem;
        private List<PlayerItem> _playerItems;
        private List<ItemViewModel> _itemViewModels = new List<ItemViewModel>();

        [Binding]
        public ObservableList<PlayerItem> Items
        {
            set
            {
                if (value == null || value.Count < 1) return;
                _playerItems = value.ToList();
                UpdateItemList();
            } 
        }

        private void OnValidate()
        {
            _poolSystem = GetComponent<DefaultPoolSystem>();
        }

        private void UpdateItemList()
        {
            _playerItems.Sort((a, b) => b.CreatedAt.CompareTo(a.CreatedAt));
            for (int i = _playerItems.Count; i < _itemViewModels.Count; i++)
            {
                _itemViewModels[i].GetComponent<PoolableBehaviour>().Release();
            }
            for (int i = 0; i < _playerItems.Count; i++)
            {
                if (_itemViewModels.Count <= i)
                {
                    _itemViewModels.Add(_poolSystem.Pool.Get().GetComponent<ItemViewModel>());
                }
                var item = _itemViewModels[i];
                item.Init(_playerItems[i], i);
                item.gameObject.SetActive(true);
            }
            _itemViewModels.Sort((a, b) => a.Index.CompareTo(b.Index));
            for (var index = _itemViewModels.Count - 1; index >= 0; --index)
            {
                _itemViewModels[index].transform.SetAsFirstSibling();
            }

            _scrollRect.ScrollToTop();
        }
    }
}