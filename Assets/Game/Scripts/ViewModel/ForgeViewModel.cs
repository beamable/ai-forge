using System.Collections.Generic;
using Beamable;
using Beamable.Common.Inventory;
using Beamable.Player;
using Game.Data;
using UnityEngine;
using UnityWeld.Binding;

namespace Game.ViewModel
{
    [Binding]
    public class ForgeViewModel : BaseViewModel
    {
        [Binding]
        public bool CanForgeItem
        {
            get => _canForgeItem;
            set
            {
                if (value == _canForgeItem) return;
                _canForgeItem = value;
                OnPropertyChanged();
            }
        }
        [Binding]
        public ObservableList<PlayerItem> PlayerItems
        {
            get => _playerItems;
            set
            {
                _playerItems = value;
                OnPropertyChanged();
            }
        }
        
        public ItemRef contentRef;
        private BeamContext _beam;
        private AiItemContent _aiItemContent;
        private ObservableList<PlayerItem> _playerItems;
        private bool _canForgeItem;
        private PlayerItemGroup items;

        //Add callback to update UI when we finish creating the item
        private void Awake()
        {
            EventBetter.Listen(this, (CreatingItemFinished e) => HandleFinishedItem(e));
        }

        private async void Start()
        {
            _beam = BeamContext.InParent(this);
            await _beam.OnReady;
            items = _beam.Inventory.GetItems(contentRef);

            await items.Refresh();
            items.OnElementsAdded += HandleItemsAdded;
            items.OnElementRemoved += HandleItemRemoved;
            PlayerItems = new ObservableList<PlayerItem>(items);
            CanForgeItem = true;

            var content = new AiItemContentRef { Id = contentRef.Id };
            _aiItemContent = await content.Resolve();
            Debug.Log(_aiItemContent.ToJson());
        }

        private void HandleItemRemoved(IEnumerable<PlayerItem> obj)
        {
            items.Refresh().Then(_ =>
            {
                PlayerItems = new ObservableList<PlayerItem>(items);
            });
        }

        private void HandleFinishedItem(CreatingItemFinished creatingItemFinished)
        {
            if (creatingItemFinished.Successful)
            {
                items.Refresh().Then(_ =>
                {
                    PlayerItems = new ObservableList<PlayerItem>(items);
                });
            }
        }

        private void HandleItemsAdded(IEnumerable<PlayerItem> addedItems)
        {
            foreach (var item in addedItems)
            {
                EventBetter.Raise(new CreatingItemFinished { ContentID = item.ContentId });
                PlayerItems.Add(item);
            }

            CanForgeItem = true;
        }

        [Binding]
        public void ForgeItem()
        {
            if(!CanForgeItem)
            {
                Debug.LogError("It should not happened!");
                return;
            }
            CanForgeItem = false;
            EventBetter.Raise(new CreatingItemStarted { ContentID = contentRef.Id });
            EventBetter.Raise(new ButtonPressed());
            Debug.Log("Forge item!");
            _beam.Inventory.Update(builder =>
            {
                builder.AddItem(contentRef.Id);
            }).Error(_ => EventBetter.Raise(new CreatingItemFinished
                { ContentID = contentRef.Id, Successful = false }));
        }
    }
}