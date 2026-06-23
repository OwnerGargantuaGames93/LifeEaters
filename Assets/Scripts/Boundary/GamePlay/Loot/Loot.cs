using System.Collections.Generic;
using Boundary.Commands;
using Boundary.Utils;
using Control.GameData;
using Data.Entities.Loot;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Loot
{
    public class Loot : IdentifiableMonoBehaviour
    {
        private IEventBus _eventBus;
        private IGameDataModel _gameData;
        
        [SerializeField] public LootData data;

        private bool _playerOnLoot;

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
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(Constants.PlayerTag))
            {
                _playerOnLoot = true;
            }
        }
    
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(Constants.PlayerTag))
            {
                _playerOnLoot = false;
            }
        }
    
        private void Update()
        {
            if (_playerOnLoot && UserInput.instance.LootIsPressed())
            {
                LootItem();
            }
        }

        private void LootItem()
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

            _playerOnLoot = false;
        
            // GameDataManager.Instance.AddObject(UniqueId);
        
            Destroy(gameObject);
        }
    }
    
    #region Events

    /// <summary>
    /// When consumable items are looted.
    /// </summary>
    public struct EConsumableLooted
    {
        public readonly string Id;
        public readonly List<ConsumableLootData> Items;

        public EConsumableLooted(string id, List<ConsumableLootData> items)
        {
            Id = id;
            Items = items;
        }
    }

    /// <summary>
    /// When equipment items are looted.
    /// </summary>
    public struct EEquipmentLooted
    {
        public readonly string Id;
        public readonly List<EquipmentLootData> Items;

        public EEquipmentLooted(string id, List<EquipmentLootData> items)
        {
            Id = id;
            Items = items;
        }
    }

    /// <summary>
    /// When essence items are looted.
    /// </summary>
    public struct EEssenceLooted
    {
        
        public readonly string Id;
        public readonly List<EssenceLootData> Items;

        public EEssenceLooted(string id, List<EssenceLootData> items)
        {
            Id = id;
            Items = items;
        }
    }

    /// <summary>
    /// When key items are looted.
    /// </summary>
    public struct EKeyLooted
    {
        public readonly string Id;
        public readonly List<KeyLootData> Items;

        public EKeyLooted(string id, List<KeyLootData> items)
        {
            Id = id;
            Items = items;
        }
    }
    
    /// <summary>
    /// When life items are looted.
    /// </summary>
    public struct ELifeLooted
    {
        public readonly string Id;
        public readonly List<LifeLootData> Items;

        public ELifeLooted(string id, List<LifeLootData> items)
        {
            Id = id;
            Items = items;
        }
    }
    
    #endregion
}
