using System;
using System.Collections.Generic;
using Data.Entities.Item;

namespace Data.Entities.Shops
{
    [Serializable]
    public class Shop
    {
        public string shopName;
        
        public string shopDisplayName;
        
        public List<ConsumableShopItem> consumables = new();
        
        public List<KeyShopItem> keys = new();
        
        public List<EquipmentShopItem> equipments = new();
        
        public List<LifeShopItem> lifes = new();
        
        public bool IsEmpty()
        {
            return (consumables == null || consumables.Count == 0) &&
                   (equipments == null || equipments.Count == 0) &&
                   (keys == null || keys.Count == 0) &&
                   (lifes == null || lifes.Count == 0);
        }
    }
    
    [Serializable]
    public class ConsumableShopItem
    {
        public ConsumableId id;
        public int price;
        public int quantity;
    }

    [Serializable]
    public class EquipmentShopItem
    {
        public EquipmentId id;
        public int price;
        public int quantity;
    }

    [Serializable]
    public class KeyShopItem
    {
        public KeyId id;
        public int price;
        public int quantity;
    }

    [Serializable]
    public class LifeShopItem
    {
        public LifeId id;
        public int price;
        public int quantity;
    }
}