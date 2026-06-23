using System;
using System.Collections.Generic;
using Data.Entities.Item;

namespace Data.Entities.Loot
{
    [Serializable]
    public class LootData
    {
        public List<ConsumableLootData> Consumables;
        
        public List<EquipmentLootData> Equipment;
        
        public List<EssenceLootData> Essences;
        
        public List<KeyLootData> Keys;
        
        public List<LifeLootData> Lifes;
    }
    
    [Serializable]
    public class ConsumableLootData
    {
        public ConsumableId Id;
        
        public int Quantity;
        
        public ConsumableLootData(ConsumableId id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }
    
    [Serializable]
    public class EquipmentLootData
    {
        public EquipmentId Id;
        
        public int Quantity;
        
        public EquipmentLootData(EquipmentId id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }
    
    [Serializable]
    public class EssenceLootData
    {
        public EssenceId Id;
        
        public int Quantity;
        
        public EssenceLootData(EssenceId id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }
    
    [Serializable]
    public class KeyLootData
    {
        public KeyId Id;
        
        public int Quantity;
        
        public KeyLootData(KeyId id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }

    [Serializable]
    public class LifeLootData
    {
        public LifeId Id;
        
        public int Quantity;
        
        public LifeLootData(LifeId id, int quantity)
        {
            Id = id;
            Quantity = quantity;
        }
    }
}