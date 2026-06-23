using System.Collections.Generic;
using Control.Inventory;
using Data.Entities.Item;

namespace Control.Pit
{
    public interface IPitModel
    {
        public float GetEnergyCost();
        public PitObjectData GetDesiredObject();
        public List<InventoryPitObjectItem> GetCurrentObjectsGeneration();
    }
}