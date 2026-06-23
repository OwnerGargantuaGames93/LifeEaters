using Boundary.Commands;
using Boundary.Utils;
using Control.GameData;
using Data.Entities.Loot;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Loot
{
    public class Chest : IdentifiableMonoBehaviour
    {
        private IEventBus _eventBus;
        private IGameDataModel _gameData;

        [SerializeField] public LootData data;
        [SerializeField] private Sprite openChestSprite;

        private bool _playerNear;
        private bool _isOpen;

        private void Awake()
        {
            CommonAwake();
            
            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;
        }

        private void Start()
        {
            if (_gameData.IsObjectCollected(Id))
            {
                _isOpen = true;
                GetComponent<SpriteRenderer>().sprite = openChestSprite;
            }
            
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(Constants.PlayerTag))
            {
                _playerNear = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(Constants.PlayerTag))
            {
                _playerNear = false;
            }
        }

        private void Update()
        {
            if (_playerNear && !_isOpen && UserInput.instance.LootIsPressed())
            {
                OpenChest();
            }
        }

        private void OpenChest()
        {
            if (data.Consumables.Count > 0)
            {
                _eventBus.Publish(new EConsumableLooted(Id, data.Consumables));
            }

            if (data.Equipment.Count > 0)
            {
                _eventBus.Publish(new EEquipmentLooted(Id, data.Equipment));
            }

            if (data.Essences.Count > 0)
            {
                _eventBus.Publish(new EEssenceLooted(Id, data.Essences));
            }

            if (data.Keys.Count > 0)
            {
                _eventBus.Publish(new EKeyLooted(Id, data.Keys));
            }
            
            if (data.Lifes.Count > 0)
            {
                _eventBus.Publish(new ELifeLooted(Id, data.Lifes));
            }

            _isOpen = true;
            GetComponent<SpriteRenderer>().sprite = openChestSprite;
        }
    }
}