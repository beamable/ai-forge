using System.Collections;
using System.Collections.Generic;
using Beamable;
using Beamable.Player;
using Beamable.Server.Clients;
using Game.Helpers;
using Game.Utils;
using UnityEngine;
using UnityWeld.Binding;

namespace Game.ViewModel
{
    [Binding]
    public class ItemViewModel : BaseViewModel
    {
        [SerializeField] private Sprite defaultSprite;
        private string _name = string.Empty;

        [Binding]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _description = "Put description here";
        private Sprite _sprite;
        private int _index;

        [Binding]
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        [Binding]
        public Sprite Icon
        {
            get => _sprite;
            set
            {
                _sprite = value;
                OnPropertyChanged();
            }
        }

        [Binding]
        public int Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public PlayerItem Model;

        public int Index => _index;
        PlayerItem _playerItem;
        private int _price = 10;
        private BeamContext _beam;

        public void Init(PlayerItem playerItem, int index)
        {
            _beam = BeamContext.InParent(this);
            _playerItem = playerItem;
            _index = index;
            Name = playerItem.Properties["name"];
            Description = playerItem.Properties["description"];
            if (int.TryParse(playerItem.Properties.GetValueOrDefault("price", "10"), out int price))
            {
                Price = price;
            }
            if (playerItem.Properties.ContainsKey("image") &&
                !string.IsNullOrWhiteSpace(playerItem.Properties["image"]))
            {
                StartCoroutine(DownloadHandlerTexture(playerItem.Properties["image"], playerItem.ProxyId.GetOrElse($"{playerItem.ContentId}_{playerItem.ContentId}")));
            }
            else
            {
                Icon = defaultSprite;
            }
        }

        [Binding]
        public void SellItem()
        {
            if(_playerItem.ProxyId.HasValue)
            {
                gameObject.SetActive(false);
                _beam.Microservices().AIMicroservice().SellSword(_playerItem.ProxyId.Value)
                    .Then(_ => _beam.Inventory.Refresh());
            }
        }


        IEnumerator DownloadHandlerTexture(string url, string id)
        {
            var task = ImageDownloader.GetImage(url, id);
            yield return new WaitUntil(() => task.IsCompleted);

            Icon = Sprite.Create(task.Result, new Rect(0, 0, task.Result.width, task.Result.height),
                new Vector2(0.5f, 0.5f));
        }
    }
}