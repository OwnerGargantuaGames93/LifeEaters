 using System;
using System.Collections.Generic;
using System.Linq;
using Boundary.UI.Shop;
using Control.GameData;
using Control.Player;
using Data.Database;
using Data.Entities.Game;
using Infra.EventBus;
using UnityEngine;
using ShopData = Data.Entities.Shops;

namespace Control.Shop
{
    public class ShopModel: IShopModel
    {
        private readonly IEventBus _eventBus;
        private readonly IPlayerModel _playerModel;
        
        private readonly List<ShopData.Shop> _shopStatus = new();
        private bool _shopIsOpen = false;

        public ShopModel(IEventBus eventBus, IPlayerModel playerModel)
        {
            _eventBus = GameContext.Instance.EventBus;
            _playerModel = GameContext.Instance.Player;
            
            SubscribeToEvents();
        }
        
        #region Events Handling
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EOpenShop>(OnOpenShop);
            _eventBus.Subscribe<ECloseShop>(OnCloseShop);
            _eventBus.Subscribe<EGameDataLoaded>(OnGameDataLoaded);
            _eventBus.Subscribe<EShopItemSelected>(OnShopItemSelected);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EOpenShop>(OnOpenShop);
            _eventBus.Unsubscribe<ECloseShop>(OnCloseShop);
            _eventBus.Subscribe<EGameDataLoaded>(OnGameDataLoaded);
            _eventBus.Unsubscribe<EShopItemSelected>(OnShopItemSelected);
        }

        private void OnOpenShop(EOpenShop e)
        {
            var shopName = e.ShopName;
            
            Debug.Log($"ShopModel: Opening shop '{shopName}'");

            
            // Check the existence of the shop
            var shop = DataSource.Instance.GetShopByName(shopName);

            if (shop == null)
            {
                Debug.LogError($"ShopModel: Shop '{shopName}' not found in the database.");
                throw new Exception();
            }
        
            // Check if there's some items
            if (shop.IsEmpty())
            {
                Debug.Log($"ShopModel: Shop '{shopName}' has no items to sell.");
                throw new Exception();
            }
            
            var buyableItems = GetBuyableItems(shop);
            
            // Publish an event to open the shop UI with the buyable items
            _eventBus.Publish(new EDisplayShopItemsList(shopName, shop.shopDisplayName, buyableItems));

            _shopIsOpen = true;
        }
        
        private void OnCloseShop(ECloseShop e)
        {
            Debug.Log("ShopModel: Closing shop");
            _shopIsOpen = false;
        }

        private void OnShopItemSelected(EShopItemSelected e)
        {
            var selectedItem = e.SelectedItem;
            var shopName = e.ShopName;

            Debug.Log($"ShopModel: Item '{selectedItem.DisplayName}' selected in shop '{shopName}'");
            
            // Checks if the player has enough money to purchase the item
            if (!PlayerCanBuyItem(selectedItem))
            {
                Debug.Log($"ShopModel: Player cannot afford item '{selectedItem.DisplayName}'");
                return;
            }
            
            // Add the item shopped to the current shop status
            var currentShopStatus = _shopStatus.Find(s => s.shopName == shopName);
            if (currentShopStatus == null)
            {
                currentShopStatus = new ShopData.Shop
                {
                    shopName = shopName
                };
                
                _shopStatus.Add(currentShopStatus);
            }
            
            // Force to buy only one quantity of the item, even if the shop has more than one quantity available, to avoid complexity in the UI and in the player inventory management
            selectedItem.SetQuantity(1);
            AddShopItemToCurrentShopStatus(selectedItem, currentShopStatus);

            // Send event to notify the item has been purchased
            _eventBus.Publish(new EItemPurchased(selectedItem));
            
            // Close the shop after purchase
            _eventBus.Publish(new ECloseShop());
        }
        
        private void OnGameDataLoaded(EGameDataLoaded e)
        {
            var gameData = e.GameData;

            if (gameData.shopStatus == null)
            {
                return;
            }

            _shopStatus.Clear();
            _shopStatus.AddRange(gameData.shopStatus);
        }
        
        #endregion

        private List<ShopItemSlot> GetBuyableItems(ShopData.Shop shop)
        {
                var shopName = shop.shopName;
                var currentShopStatus = _shopStatus.Find(s => s.shopName == shopName);
                
                // Get the list of the already buyed items (if any) and remove them from the buyable items
                var buyableItems = new List<ShopItemSlot>();
                foreach (var keyShopItem in shop.keys)
                {
                    var alreadyBought = currentShopStatus?.keys.Find(k => k.id == keyShopItem.id);
                    
                    // If already bought all the shop quantity, skip it
                    if (alreadyBought != null && alreadyBought.quantity >= keyShopItem.quantity)
                    {
                        continue;
                    }
                    
                    // Calculate the remaining shop quantity
                    var shopQuantity = keyShopItem.quantity;
                    if (alreadyBought != null)
                    {
                        shopQuantity -= alreadyBought.quantity;
                    }
                    
                    var itemSlot = new ShopItemSlot(shopName, keyShopItem, shopQuantity);
                    buyableItems.Add(itemSlot);
                }
                
                foreach (var consumableShopItem in shop.consumables)
                {
                    var alreadyBought = currentShopStatus?.consumables.Find(k => k.id == consumableShopItem.id);
                    
                    // If already bought all the shop quantity, skip it
                    if (alreadyBought != null && alreadyBought.quantity >= consumableShopItem.quantity)
                    {
                        continue;
                    }
                    
                    // Calculate the remaining shop quantity
                    var shopQuantity = consumableShopItem.quantity;
                    if (alreadyBought != null)
                    {
                        shopQuantity -= alreadyBought.quantity;
                    }
                    
                    var itemSlot = new ShopItemSlot(shopName, consumableShopItem, shopQuantity);
                    buyableItems.Add(itemSlot);
                }
                
                foreach (var equipmentShopItem in shop.equipments)
                {
                    var alreadyBought = currentShopStatus?.equipments.Find(k => k.id == equipmentShopItem.id);
                    // If already bought all the shop quantity, skip it
                    if (alreadyBought != null && alreadyBought.quantity >= equipmentShopItem.quantity)
                    {
                        continue;
                    }
                    
                    // Calculate the remaining shop quantity
                    var shopQuantity = equipmentShopItem.quantity;
                    if (alreadyBought != null)
                    {
                        shopQuantity -= alreadyBought.quantity;
                    }
                    
                    var itemSlot = new ShopItemSlot(shopName, equipmentShopItem, shopQuantity);
                    buyableItems.Add(itemSlot);
                }
                
                foreach (var lifeShopItem in shop.lifes)
                {
                    
                    var alreadyBought = currentShopStatus?.lifes.Find(k => k.id == lifeShopItem.id);
                    // If already bought all the shop quantity, skip it
                    if (alreadyBought != null && alreadyBought.quantity >= lifeShopItem.quantity)
                    {
                        continue;
                    }
                    
                    // Calculate the remaining shop quantity
                    var shopQuantity = lifeShopItem.quantity;
                    if (alreadyBought != null)
                    {
                        shopQuantity -= alreadyBought.quantity;
                    }
                    
                    var itemSlot = new ShopItemSlot(shopName, lifeShopItem, shopQuantity);
                    buyableItems.Add(itemSlot);
                }
                
                return buyableItems;
        }

        private bool PlayerCanBuyItem(ShopItemSlot item)
        {
            var totalPlayerMoney = _playerModel.Status.coins;
            var totalPrice = item.Price;
            
            return totalPlayerMoney >= totalPrice;
        }
        
        private void AddShopItemToCurrentShopStatus(ShopItemSlot selectedItem, ShopData.Shop currentShopStatus)
        {
            switch (selectedItem.SlotType)
            {
                case ShopItemSlotType.Key:
                    var existingKey = currentShopStatus.keys.Find(i => i.id == selectedItem.KeyId);
                    if (existingKey != null)
                    {
                        existingKey.quantity += selectedItem.Quantity;
                    }
                    else
                    {
                        var newKey = new ShopData.KeyShopItem
                        {
                            id = selectedItem.KeyId,
                            quantity = selectedItem.Quantity
                        };

                        currentShopStatus.keys.Add(newKey);
                    }
                    break;
                case ShopItemSlotType.Consumable:
                    var existingConsumable = currentShopStatus.consumables.Find(i => i.id == selectedItem.ConsumableId);
                    if (existingConsumable != null)
                    {
                        existingConsumable.quantity += selectedItem.Quantity;
                    }
                    else
                    {
                        var newConsumable = new ShopData.ConsumableShopItem()
                        {
                            id = selectedItem.ConsumableId,
                            quantity = selectedItem.Quantity
                        };
                        currentShopStatus.consumables.Add(newConsumable);
                    }
                    break;
                case ShopItemSlotType.Equipment:
                    var existingEquipment = currentShopStatus.equipments.Find(i => i.id == selectedItem.EquipmentId);
                    if (existingEquipment != null)
                    {
                        existingEquipment.quantity += selectedItem.Quantity;
                    }
                    else
                    {
                        var newEquipment = new ShopData.EquipmentShopItem
                        {
                            id = selectedItem.EquipmentId,
                            quantity = selectedItem.Quantity
                        };
                        currentShopStatus.equipments.Add(newEquipment);
                    }
                    break;
                case ShopItemSlotType.Life:
                    var existingLife = currentShopStatus.lifes.Find(i => i.id == selectedItem.LifeId);
                    if (existingLife != null)
                    {
                        existingLife.quantity += selectedItem.Quantity;
                    }
                    else
                    {
                        var newLife = new ShopData.LifeShopItem()
                        {
                            id = selectedItem.LifeId,
                            quantity = selectedItem.Quantity
                        };
                        currentShopStatus.lifes.Add(newLife);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
        
        public bool IsShopOpen()
        {
            return _shopIsOpen;
        }

        public void Save(GameSessionData gameData)
        {
            gameData.shopStatus = _shopStatus;
        }
    }

    #region Events

    public struct EOpenShop
    {
        public readonly string ShopName;

        public EOpenShop(string shopName)
        {
            ShopName = shopName;
        }
    }
    
    public struct ECloseShop
    {
    }
    
    public struct EDisplayShopItemsList
    {
        public readonly string ShopName;
        
        public readonly string ShopDisplayName;
        
        public readonly List<ShopItemSlot> BuyableItems;

        public EDisplayShopItemsList(string shopName, string shopDisplayName, List<ShopItemSlot> buyableItems)
        {
            ShopName = shopName;
            ShopDisplayName = shopDisplayName;
            BuyableItems = buyableItems;
        }
    }
    
    public struct EShopItemSelected
    {
        public readonly ShopItemSlot SelectedItem;
        public readonly string ShopName;

        public EShopItemSelected(ShopItemSlot selectedItem, string shopName)
        {
            SelectedItem = selectedItem;
            ShopName = shopName;
        }
    }
    
    public struct EItemPurchased
    {
        public readonly ShopItemSlot PurchasedItem;

        public EItemPurchased(ShopItemSlot purchasedItem)
        {
            PurchasedItem = purchasedItem;
        }
    }
    
    public struct EUpdateShopItemSelectedIndex
    {
        public readonly int SelectedIndex;

        public EUpdateShopItemSelectedIndex(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }
    
    public struct EShopConfirmCommandExecuted { }
    
    public struct EShopCancelCommandExecuted { }

    #endregion
}