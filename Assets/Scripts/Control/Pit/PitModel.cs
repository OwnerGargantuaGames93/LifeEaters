using System.Collections.Generic;
using System.Linq;
using Control.GameData;
using Control.Inventory;
using Data.Database;
using Data.Entities.Item;
using Infra.EventBus;

namespace Control.Pit
{
    public class PitModel: IPitModel
    {
        private readonly IInventoryModel _inventory;
        private readonly IEventBus _eventBus;

        private PitObjectId? _desiredObjectId;
        
        public PitModel(IEventBus eventBus, IInventoryModel inventory)
        {
            _inventory = inventory;
            _eventBus = eventBus;
            
            SubscribeToEvents();
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EGameDataLoaded>(OnGameDataLoaded);
            _eventBus.Subscribe<EEssenceEquipped>(OnEssenceEquipped);
            _eventBus.Subscribe<ENextPitObjectCommandExecuted>(OnNextPitObjectSelected);
            _eventBus.Subscribe<EPreviousPitObjectCommandExecuted>(OnPreviousPitObjectSelected);
        }

        #region Event Handlers

        private void OnEssenceEquipped(EEssenceEquipped e)
        {
            // When an essence is equipped, we might want to update the desired object with the first object
            var objects = _inventory.GetAllPitObjects();
            if (objects.Count > 0)
            {
                _desiredObjectId = objects.First().id;
            }
            else
            {
                _desiredObjectId = null;
            }
            
            _eventBus.Publish(new EDesiredPitObjectChanged(_desiredObjectId));
        }

        private void OnNextPitObjectSelected(ENextPitObjectCommandExecuted e)
        {
            var objects = _inventory.GetAllPitObjects();
            if (objects.Count == 0)
            {
                _desiredObjectId = null;
                return;
            }

            if (_desiredObjectId == null)
            {
                _desiredObjectId = objects.First().id;
                return;
            }

            var pitObjectId = _desiredObjectId.Value;
            var currentIndex = -1;
            for (var i = 0; i < objects.Count; i++)
            {
                if (objects[i].id != pitObjectId)
                {
                    continue;
                }

                currentIndex = i;
                break;
            }
            
            var nextIndex = (currentIndex + 1) % objects.Count;
            _desiredObjectId = objects[nextIndex].id;
            
            _eventBus.Publish(new EDesiredPitObjectChanged(_desiredObjectId));
        }
        
        private void OnPreviousPitObjectSelected(EPreviousPitObjectCommandExecuted e)
        {
            var objects = _inventory.GetAllPitObjects();
            if (objects.Count == 0)
            {
                _desiredObjectId = null;
                return;
            }

            if (_desiredObjectId == null)
            {
                _desiredObjectId = objects.First().id;
                return;
            }
            
            var currentIndex = -1;
            for (var i = 0; i < objects.Count; i++)
            {
                if (objects[i].id != _desiredObjectId.Value)
                {
                    continue;
                }

                currentIndex = i;
                break;
            }
            
            var previousIndex = (currentIndex - 1 + objects.Count) % objects.Count;
            _desiredObjectId = objects[previousIndex].id;
            
            _eventBus.Publish(new EDesiredPitObjectChanged(_desiredObjectId.Value));
        }
        
        private void OnGameDataLoaded(EGameDataLoaded e)
        {
            // When game data is loaded, we might want to set the desired object to the first available object
            var objects = _inventory.GetAllPitObjects();
            if (objects.Count > 0)
            {
                _desiredObjectId = objects.First().id;
            }
            else
            {
                _desiredObjectId = null;
            }
            
            _eventBus.Publish(new EDesiredPitObjectChanged(_desiredObjectId));
        }

        #endregion
        
        /// <summary>
        /// The energy cost for generating pit is: cost of all standard essences equipped * number of pits to be generated
        /// </summary>
        /// <returns></returns>
        public float GetEnergyCost()
        {
            var standardEquippedEssences = _inventory.EquippedStandardEssences();

            var energyCost = 0f;
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var essence in standardEquippedEssences)
            {
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (var obj in essence.item.objects)
                {
                    energyCost += DataSource.Instance.GetPitObject(obj).energyCost;
                }
            }

            return energyCost;
        }

        public PitObjectData GetDesiredObject()
        {
            return _desiredObjectId == null ? null : DataSource.Instance.GetPitObject(_desiredObjectId.Value);
        }
        
        public int GetNumberOfPitsCanBeGenerated() {
            // TODO: Define logic to determine the number of pits that can be generated
            // The number of pit generated should depend some player abilities and/or malus
            // So this "2" is relative to a some other stuff
            // Also, which points will be selected is also relative to some player abilities
            return 2;
        }
        
        public List<InventoryPitObjectItem> GetCurrentObjectsGeneration()
        {
            var equippedEssences = _inventory.EquippedStandardEssences();
            
            var generatedObjects = new List<InventoryPitObjectItem>();
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var essence in equippedEssences)
            {
                var objIds = essence.item.objects;
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (var objId in objIds)
                {
                    generatedObjects.Add(new InventoryPitObjectItem(objId));
                }
            }
            
            return generatedObjects;
        }
    }
}

#region Events

public struct ENextPitObjectCommandExecuted
{
}

public struct EPreviousPitObjectCommandExecuted
{
}

public struct EDesiredPitObjectChanged
{
    public PitObjectId? DesiredObjectId;

    public EDesiredPitObjectChanged(PitObjectId? id)
    {
        DesiredObjectId = id;
    }
}

#endregion