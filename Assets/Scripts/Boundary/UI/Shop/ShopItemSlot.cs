using Data.Database;
using Data.Entities.Item;
using Data.Entities.Shops;
using UnityEngine;

namespace Boundary.UI.Shop
{
    
    public class ShopItemSlot
    {
        public KeyId KeyId;

        public LifeId LifeId;
        
        public readonly ConsumableId ConsumableId;
        
        public readonly EquipmentId EquipmentId;
        
        public readonly ShopItemSlotType SlotType;

        public readonly string DisplayName;

        public readonly Sprite Icon;

        public readonly string ShopDescription;

        public readonly int Price;
        
        public int Quantity;
        
        public void SetQuantity(int quantity)
        {
            Quantity = quantity;
        }
        
        public ShopItemSlot(string shopName, KeyShopItem shopItem, int shopQuantity)
        {
            var item = DataSource.Instance.GetKeyItem(shopItem.id);
            
            KeyId = item.Id;
            DisplayName = item.Name;
            Icon = item.Icon;
            SlotType = ShopItemSlotType.Key;
            ShopDescription = item.ShopDescriptions.Find(desc => desc.ShopName == shopName)?.Description ?? item.Description;
            Price = shopItem.price;
            Quantity = shopQuantity;
        }

        public ShopItemSlot(string shopName, ConsumableShopItem shopItem, int shopQuantity)
        {
            var item = DataSource.Instance.GetConsumableItem(shopItem.id);
            
            ConsumableId = item.Id;
            DisplayName = item.Name;
            Icon = item.Icon;
            SlotType = ShopItemSlotType.Consumable;
            ShopDescription = item.ShopDescriptions.Find(desc => desc.ShopName == shopName)?.Description ?? item.Description;
            Price = shopItem.price;
            Quantity = shopQuantity;
        }
        
        public ShopItemSlot(string shopName, EquipmentShopItem shopItem, int shopQuantity)
        {
            var item = DataSource.Instance.GetEquipmentItem(shopItem.id);
            
            EquipmentId = item.id;
            DisplayName = item.Name;
            Icon = item.Icon;
            SlotType = ShopItemSlotType.Equipment;
            ShopDescription = item.ShopDescriptions.Find(desc => desc.ShopName == shopName)?.Description ?? item.Description;
            Price = shopItem.price;
            Quantity = shopQuantity;
        }

        public ShopItemSlot(string shopName, LifeShopItem shopItem, int shopQuantity)
        {
            var item = DataSource.Instance.GetLifeItem(shopItem.id);
            
            LifeId = item.id;
            DisplayName = item.Name;
            Icon = item.Icon;
            SlotType = ShopItemSlotType.Life;
            ShopDescription = item.ShopDescriptions.Find(desc => desc.ShopName == shopName)?.Description ?? item.Description;
            Price = shopItem.price;
            Quantity = shopQuantity;
        }
    }
    
    public enum ShopItemSlotType
    {
        Consumable,
        Equipment,
        Key,
        Life
    }
}