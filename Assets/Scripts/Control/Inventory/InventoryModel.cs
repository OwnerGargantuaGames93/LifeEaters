using System;
using System.Collections.Generic;
using System.Linq;
using Boundary.Interactable;
using Boundary.Loot;
using Boundary.UI.Shop;
using Boundary.UI.StatueMenu.EquipmentMenu;
using Control.GameData;
using Control.Player;
using Control.Shop;
using Data.Database;
using Data.Entities.Item;
using Data.Entities.Player;
using Infra.EventBus;
using UnityEngine;
// ReSharper disable All

namespace Control.Inventory
{
    /// <summary>
    /// Inventory data model. Contains inventory-related data such as items, equipment,
    /// essences and the logic to manipulate them.
    /// </summary>
    public class InventoryModel : IInventoryModel
    {
        private readonly IEventBus _eventBus;

        public List<InventoryConsumableItem> Consumables { get; private set; } = new();
        public List<InventoryEquipmentItem> Equipments { get; private set; } = new();
        public List<InventoryKeyItem> KeyItems { get; private set; } = new();
        public List<InventoryEssenceItem> Essences { get; private set; } = new();
        public List<InventoryLifeItem> Lifes { get; private set; } = new();

        public InventoryEquipmentItem Body { get; private set; }
        public InventoryEquipmentItem Acc1 { get; private set; }
        public InventoryEquipmentItem Acc2 { get; private set; }
        public InventoryEquipmentItem Acc3 { get; private set; }

        public InventoryEssenceItem EssenceS1 { get; private set; }
        public InventoryEssenceItem EssenceS2 { get; private set; }
        public InventoryEssenceItem EssenceB1 { get; private set; }
        public InventoryEssenceItem EssenceS3 { get; private set; }
        public InventoryEssenceItem EssenceB2 { get; private set; }
        public InventoryEssenceItem EssenceS4 { get; private set; }
        public InventoryEssenceItem EssenceB3 { get; private set; }
        public InventoryEssenceItem EssenceS5 { get; private set; }

        public InventoryModel(IEventBus eventBus)
        {
            _eventBus = eventBus;

            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EEquipmentSelected>(OnEquipmentSelected);
            _eventBus.Subscribe<EEquipmentRemoved>(OnEquipmentRemoved);
            _eventBus.Subscribe<EConsumableLooted>(OnConsumablesLooted);
            _eventBus.Subscribe<EEquipmentLooted>(OnEquipmentsLooted);
            _eventBus.Subscribe<EEssenceLooted>(OnEssencesLooted);
            _eventBus.Subscribe<EKeyLooted>(OnKeyItemsLooted);
            _eventBus.Subscribe<ELifeLooted>(OnLifeItemsLooted);
            _eventBus.Subscribe<EItemPurchased>(OnItemPuchased);
            _eventBus.Subscribe<EItemUsed>(OnItemUsed);
            _eventBus.Subscribe<EEssenceSelected>(OnEssencesSelected);
            _eventBus.Subscribe<EGameDataLoaded>(OnGameLoaded);
            _eventBus.Subscribe<ELifeCollected>(OnLifeCollected);
        }

        #region Event Handlers

        private void OnEquipmentSelected(EEquipmentSelected e)
        {
            var selectedEquipment = e.SelectedEquipment;
            var slot = e.Slot;

            if (selectedEquipment == null)
            {
                Debug.LogWarning("Selected equipment is null");
                return;
            }

            if (selectedEquipment.SlotType != InventoryItemSlotType.EQUIPMENT)
            {
                Debug.LogWarning("You can't equip item that is not equipment");
                return;
            }

            var equipmentItem = Equipments.Find(i => i.item == selectedEquipment.EquipmentId);

            if (equipmentItem == null)
            {
                return;
            }

            switch (slot)
            {
                case EquipmentSlot.Body:
                    Body = equipmentItem;
                    break;
                case EquipmentSlot.Acc1:
                    Acc1 = equipmentItem;
                    break;
                case EquipmentSlot.Acc2:
                    Acc2 = equipmentItem;
                    break;
                case EquipmentSlot.Acc3:
                    Acc3 = equipmentItem;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
            }

            _eventBus.Publish(new EItemsEquipped(new List<InventoryEquipmentItem> { equipmentItem }));
        }

        private void OnEquipmentRemoved(EEquipmentRemoved e)
        {
            var selectedEquipment = e.SelectedEquipment;
            var slot = e.Slot;

            var equipmentToRemove = slot switch
            {
                EquipmentSlot.Body => Body,
                EquipmentSlot.Acc1 => Acc1,
                EquipmentSlot.Acc2 => Acc2,
                EquipmentSlot.Acc3 => Acc3,
                _ => null
            };

            if (equipmentToRemove != null)
            {
                switch (slot)
                {
                    case EquipmentSlot.Body:
                        Body = null;
                        break;
                    case EquipmentSlot.Acc1:
                        Acc1 = null;
                        break;
                    case EquipmentSlot.Acc2:
                        Acc2 = null;
                        break;
                    case EquipmentSlot.Acc3:
                        Acc3 = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
                }
            }

            var equipmentItem = Equipments.Find(i => i.item == selectedEquipment.EquipmentId);

            if (equipmentItem == null)
            {
                return;
            }

            _eventBus.Publish(new EItemsUnequipped(new List<InventoryEquipmentItem> { equipmentItem }));
        }

        private void OnConsumablesLooted(EConsumableLooted e)
        {
            var items = e.Items;

            foreach (var item in items)
            {
                var existingItem = Consumables.Find(i => i.item == item.Id);
                if (existingItem != null)
                {
                    existingItem.quantity += item.Quantity;
                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
                }
                else
                {
                    var newItem = new InventoryConsumableItem(item.Id, item.Quantity);
                    Consumables.Add(newItem);

                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
                }
            }
        }

        private void OnEquipmentsLooted(EEquipmentLooted e)
        {
            var items = e.Items;

            foreach (var item in items)
            {
                var existingItem = Equipments.Find(i => i.item == item.Id);
                if (existingItem != null)
                {
                    existingItem.quantity += item.Quantity;

                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
                }
                else
                {
                    var newItem = new InventoryEquipmentItem(item.Id, item.Quantity);
                    Equipments.Add(newItem);

                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
                }
            }
        }

        private void OnEssencesLooted(EEssenceLooted e)
        {
            var items = e.Items;

            foreach (var item in items)
            {
                var existingItem = Essences.Find(i => i.item.sourceId == item.Id);
                if (existingItem != null)
                {
                    existingItem.quantity += item.Quantity;

                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
                }
                else
                {
                    var essenceItem = DataSource.Instance.GetEssenceItem(item.Id);
                    var essenceVersion = essenceItem.CreateEssenceVersion();

                    // Allow essences with 0 objects only if they are Behavioural type
                    if (essenceVersion.objects.Count == 0 && essenceItem.type != EssenceType.Behavioural)
                    {
                        // TODO: Reproduce some kind of feedback
                        Debug.LogWarning(
                            $"Looted essence {item.Id} has no objects associated, cannot create inventory item");
                        continue;
                    }

                    var newItem = new InventoryEssenceItem(essenceItem.CreateEssenceVersion(), item.Quantity);

                    Essences.Add(newItem);

                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
                }
            }
        }

        private void OnKeyItemsLooted(EKeyLooted e)
        {
            var items = e.Items;

            foreach (var item in items)
            {
                var existingItem = KeyItems.Find(i => i.item == item.Id);
                if (existingItem != null)
                {
                    existingItem.quantity += item.Quantity;

                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
                }
                else
                {
                    var newItem = new InventoryKeyItem(item.Id, item.Quantity);
                    KeyItems.Add(newItem);

                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
                }
            }
        }

        private void OnLifeItemsLooted(ELifeLooted e)
        {
            var items = e.Items;

            foreach (var item in items)
            {
                var existingItem = Lifes.Find(i => i.item == item.Id);
                if (existingItem != null)
                {
                    existingItem.quantity += item.Quantity;

                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
                }
                else
                {
                    var newItem = new InventoryLifeItem(item.Id, item.Quantity);
                    Lifes.Add(newItem);

                    _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
                }
            }
        }

        private void OnLifeCollected(ELifeCollected e)
        {
            var id = e.Id;

            var existingItem = Lifes.Find(i => i.item == id);
            if (existingItem != null)
            {
                existingItem.quantity += 1;

                _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
            }
            else
            {
                var newItem = new InventoryLifeItem(id, 1);
                Lifes.Add(newItem);

                _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
            }
        }

        private void OnItemUsed(EItemUsed e)
        {
            var item = e.Item;

            if (item.SlotType != InventoryItemSlotType.CONSUMABLE && item.SlotType != InventoryItemSlotType.LIFE)
            {
                Debug.LogWarning("You can't use item that is not a consumable or life");
                return;
            }

            switch (item.SlotType)
            {
                case InventoryItemSlotType.CONSUMABLE:
                {
                    if (!ItemUsageController.Instance.CanUseItem(item.ConsumableId))
                    {
                        Debug.LogWarning(ItemUsageController.Instance.GetItemUsageRequirementText(item.ConsumableId));
                        return;
                    }

                    var consumableItem = Consumables.Find(i => i.item == item.ConsumableId);
                    if (consumableItem == null)
                    {
                        return;
                    }

                    // TODO: support using multiple consumables at once
                    consumableItem.quantity -= 1;
                    consumableItem.LastUsedTime = DateTime.UtcNow;

                    if (consumableItem.quantity <= 0)
                    {
                        Consumables.Remove(consumableItem);
                    }

                    var consumable = DataSource.Instance.GetConsumableItem(consumableItem.item);

                    _eventBus.Publish(new EEffectsApplied(consumable.Effects));
                    break;
                }
                case InventoryItemSlotType.LIFE:
                {
                    var lifeItem = Lifes.Find(i => i.item == item.LifeId);
                    if (lifeItem == null)
                    {
                        return;
                    }

                    lifeItem.quantity -= 1;
                    lifeItem.LastUsedTime = DateTime.UtcNow;

                    if (lifeItem.quantity <= 0)
                    {
                        Lifes.Remove(lifeItem);
                    }

                    // Create the essence, if available
                    var lifeItemData = DataSource.Instance.GetLifeItem(lifeItem.item);
                    {

                        var lifeEssence = lifeItemData.associatedEssence;
                        if (lifeEssence != EssenceId.Empty)
                        {
                            var essence = DataSource.Instance.GetEssenceItem(lifeEssence);
                            var essenceVersion = essence.CreateEssenceVersion();

                            // An essence version might be null if it doesn't produce any objects
                            // Exception: Behavioural essences are allowed even with 0 objects
                            if (essenceVersion != null)
                            {
                                var inventoryItem = new InventoryEssenceItem(essenceVersion, 1);
                                Essences.Add(inventoryItem);

                                _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(inventoryItem)));
                            }
                            else
                            {
                                Debug.LogWarning("Life essence has no objects associated, cannot create version");
                            }
                        }
                    }

                    _eventBus.Publish(new EEffectsApplied(lifeItemData.effects));
                    break;
                }
                case InventoryItemSlotType.EQUIPMENT:
                case InventoryItemSlotType.ESSENCE:
                case InventoryItemSlotType.TALENT:
                case InventoryItemSlotType.KEY:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnEssencesSelected(EEssenceSelected e)
        {
            var essenceToEquip = e.SelectedEssence;
            var slot = e.Slot;

            switch (slot)
            {
                case 0:
                    EssenceS1 = essenceToEquip;
                    break;
                case 1:
                    EssenceS2 = essenceToEquip;
                    break;
                case 2:
                    EssenceB1 = essenceToEquip;
                    break;
                case 3:
                    EssenceS3 = essenceToEquip;
                    break;
                case 4:
                    EssenceB2 = essenceToEquip;
                    break;
                case 5:
                    EssenceS4 = essenceToEquip;
                    break;
                case 6:
                    EssenceB3 = essenceToEquip;
                    break;
                case 7:
                    EssenceS5 = essenceToEquip;
                    break;
                default:
                    Debug.LogWarning("Invalid slot essence");
                    break;
            }

            _eventBus.Publish(new EEssenceEquipped(essenceToEquip));
        }

        private void OnGameLoaded(EGameDataLoaded e)
        {
            var gameData = e.GameData;

            Consumables = gameData.consumables;
            Lifes = gameData.lifes;
            Equipments = gameData.equipment;
            Essences = gameData.essences;
            KeyItems = gameData.keys;
            Body = gameData.body != null && gameData.body.item != EquipmentId.Empty ? gameData.body : null;
            Acc1 = gameData.acc1 != null && gameData.acc1.item != EquipmentId.Empty ? gameData.acc1 : null;
            Acc2 = gameData.acc2 != null && gameData.acc2.item != EquipmentId.Empty ? gameData.acc2 : null;
            Acc3 = gameData.acc3 != null && gameData.acc3.item != EquipmentId.Empty ? gameData.acc3 : null;
            EssenceS1 = gameData.essenceS1 != null && gameData.essenceS1.item.sourceId != EssenceId.Empty
                ? gameData.essenceS1
                : null;
            EssenceS2 = gameData.essenceS2 != null && gameData.essenceS2.item.sourceId != EssenceId.Empty
                ? gameData.essenceS2
                : null;
            EssenceB1 = gameData.essenceB1 != null && gameData.essenceB1.item.sourceId != EssenceId.Empty
                ? gameData.essenceB1
                : null;
            EssenceS3 = gameData.essenceS3 != null && gameData.essenceS3.item.sourceId != EssenceId.Empty
                ? gameData.essenceS3
                : null;
            EssenceB2 = gameData.essenceB2 != null && gameData.essenceB2.item.sourceId != EssenceId.Empty
                ? gameData.essenceB2
                : null;
            EssenceS4 = gameData.essenceS4 != null && gameData.essenceS4.item.sourceId != EssenceId.Empty
                ? gameData.essenceS4
                : null;
            EssenceB3 = gameData.essenceB3 != null && gameData.essenceB3.item.sourceId != EssenceId.Empty
                ? gameData.essenceB3
                : null;
            EssenceS5 = gameData.essenceS5 != null && gameData.essenceS5.item.sourceId != EssenceId.Empty
                ? gameData.essenceS5
                : null;
            // Old slots S6-S10 are ignored (backwards compatibility with old saves)
        }

        private void OnItemPuchased(EItemPurchased e)
        {
            var item = e.PurchasedItem;

            switch (item.SlotType)
            {
                case ShopItemSlotType.Key:
                {
                    var key = item.KeyId;
                    var existingItem = KeyItems.Find(i => i.item == key);
                    if (existingItem != null)
                    {
                        existingItem.quantity += 1;

                        _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
                    }
                    else
                    {
                        var newItem = new InventoryKeyItem(key, 1);
                        KeyItems.Add(newItem);

                        _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
                    }

                    break;
                }
                case ShopItemSlotType.Consumable:
                {
                    var consumable = item.ConsumableId;
                    var existingItem = Consumables.Find(i => i.item == consumable);
                    if (existingItem != null)
                    {
                        existingItem.quantity += 1;

                        _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
                    }
                    else
                    {
                        var newItem = new InventoryConsumableItem(consumable, 1);
                        Consumables.Add(newItem);

                        _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
                    }

                    break;
                }
                case ShopItemSlotType.Equipment:
                {
                    var equipment = item.EquipmentId;
                    var existingItem = Equipments.Find(i => i.item == equipment);
                    if (existingItem != null)
                    {
                        existingItem.quantity += 1;

                        _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
                    }
                    else
                    {
                        var newItem = new InventoryEquipmentItem(equipment, 1);
                        Equipments.Add(newItem);

                        _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
                    }

                    break;
                }
                case ShopItemSlotType.Life:
                {
                    var lifeId = item.LifeId;
                    var existingItem = Lifes.Find(i => i.item == lifeId);
                    if (existingItem != null)
                    {
                        existingItem.quantity += 1;

                        _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(existingItem)));
                    }
                    else
                    {
                        var newItem = new InventoryLifeItem(lifeId, 1);
                        Lifes.Add(newItem);

                        _eventBus.Publish(new EItemAddedToInventory(new InventoryItemSlot(newItem)));
                    }

                    break;
                }
                default:
                {
                    Debug.LogWarning("Purchased item has invalid slot type");
                    break;
                }
            }
        }

        #endregion

        #region Get Interface Implementation

        public bool HasKeyObject(KeyId keyId)
        {
            var keyItem = KeyItems.Find(i => i.item == keyId);
            return keyItem is { quantity: > 0 };
        }

        public bool IsItemEquipped(EquipmentId equipmentId)
        {
            var equippedItems = GetCurrentEquipment();
            return equippedItems.Exists(i => i.item == equipmentId);
        }

        // Torna tutti gli oggetti equipaggiabili per un certo tipo di equipaggiamento (es. body, acc, etc.)
        public List<InventoryEquipmentItem> GetEquipmentItems(EquipmentType type)
        {
            var equippedItems = GetCurrentEquipment();
            var availableItems = new List<InventoryEquipmentItem>();
            foreach (var equipment in Equipments)
            {
                var data = DataSource.Instance.GetEquipmentItem(equipment.item);

                if (data.type == type && !equippedItems.Exists(i => i.item == equipment.item))
                {
                    availableItems.Add(equipment);
                }
            }

            return availableItems;
        }

        public List<InventoryEquipmentItem> GetCurrentEquipment()
        {
            var equippedItems = new List<InventoryEquipmentItem>();
            if (Body != null)
            {
                equippedItems.Add(Body);
            }

            if (Acc1 != null)
            {
                equippedItems.Add(Acc1);
            }

            if (Acc2 != null)
            {
                equippedItems.Add(Acc2);
            }

            if (Acc3 != null)
            {
                equippedItems.Add(Acc3);
            }

            return equippedItems;
        }

        public List<InventoryItemSlot> GetItemSlots()
        {
            var itemSlots = Consumables.Select(item => new InventoryItemSlot(item)).ToList();
            itemSlots.AddRange(Equipments.Select(item => new InventoryItemSlot(item)));
            itemSlots.AddRange(KeyItems.Select(item => new InventoryItemSlot(item)));
            itemSlots.AddRange(Lifes.Select(item => new InventoryItemSlot(item)));
            itemSlots.AddRange(Essences.Select(item => new InventoryItemSlot(item)));

            // Sort the item slots by the time they were picked up
            itemSlots.Sort((x, y) => x.PickedUpAt.CompareTo(y.PickedUpAt));
            return itemSlots;
        }

        public Sprite GetEquipmentImage(EquipmentSlot selectedEquipmentSlot)
        {
            var equipment = selectedEquipmentSlot switch
            {
                EquipmentSlot.Body => Body,
                EquipmentSlot.Acc1 => Acc1,
                EquipmentSlot.Acc2 => Acc2,
                EquipmentSlot.Acc3 => Acc3,
                _ => null
            };

            if (equipment == null)
            {
                return null;
            }

            var equipmentData = DataSource.Instance.GetEquipmentItem(equipment.item);
            return equipmentData.Icon;
        }

        public string GetEquipmentInfo(EquipmentSlot selectedEquipmentSlot)
        {
            var equipment = selectedEquipmentSlot switch
            {
                EquipmentSlot.Body => Body,
                EquipmentSlot.Acc1 => Acc1,
                EquipmentSlot.Acc2 => Acc2,
                EquipmentSlot.Acc3 => Acc3,
                _ => null
            };

            if (equipment == null)
            {
                return "Empty Slot";
            }

            var equipmentData = DataSource.Instance.GetEquipmentItem(equipment.item);
            return equipmentData.Description;
        }

        public string GetEquipmentChangeString(InventoryItemSlot equipment, EquipmentSlot selectedEquipmentSlot)
        {
            // Equipment that we want to equip
            var equipmentItem = Equipments.Find(i => i.item == equipment.EquipmentId);
            if (equipmentItem == null)
            {
                return "";
            }

            var equipmentData = DataSource.Instance.GetEquipmentItem(equipmentItem.item);

            var equipmentAttributes = equipmentData.attributesModifier;
            var equipmentCharacteristics = equipmentData.characteristicsModifier;

            // Currently equipped item
            var currentEquipment = selectedEquipmentSlot switch
            {
                EquipmentSlot.Body => Body,
                EquipmentSlot.Acc1 => Acc1,
                EquipmentSlot.Acc2 => Acc2,
                EquipmentSlot.Acc3 => Acc3,
                _ => null
            };

            EquipmentItem data;
            if (currentEquipment == null)
            {
                data = null;
            }
            else
            {
                data = DataSource.Instance.GetEquipmentItem(currentEquipment.item);
            }

            var currentEquipmentAttributes = data != null
                ? data.attributesModifier
                : PlayerAttributes.Identity();
            var currentEquipmentCharacteristics = data != null
                ? data.characteristicsModifier
                : PlayerCharacteristics.Identity();

            // Calculate the change in attributes and characteristics
            var characteristicsChange = equipmentCharacteristics - currentEquipmentCharacteristics;
            var attributesChange = equipmentAttributes - currentEquipmentAttributes;
            var change = "" + characteristicsChange + attributesChange;

            return change;
        }

        public List<InventoryEssenceItem> EquippedStandardEssences()
        {
            var equippedEssences = new List<InventoryEssenceItem>
            {
                EssenceS1, EssenceS2, EssenceS3, EssenceS4, EssenceS5
            };

            return equippedEssences.Where(e => e != null && e.item.sourceId != EssenceId.Empty).ToList();
        }

        public List<InventoryEssenceItem> EquippedBehaviouralEssences()
        {
            var equippedEssences = new List<InventoryEssenceItem>
            {
                EssenceB1, EssenceB2, EssenceB3
            };

            return equippedEssences.Where(e => e != null).ToList();
        }

        public List<InventoryEssenceItem> GetEquippedEssences()
        {
            var equippedEssences = new List<InventoryEssenceItem>
            {
                EssenceS1, EssenceS2, EssenceB1, EssenceS3, EssenceB2, EssenceS4, EssenceB3, EssenceS5
            };

            return equippedEssences.ToList();
        }

        public List<InventoryEssenceItem> GetAvailableEssences(EssenceType type)
        {
            var availableEssences = new List<InventoryEssenceItem>();
            
            foreach (var essence in Essences)
            {
                var essenceData = DataSource.Instance.GetEssenceItem(essence.item.sourceId);
                var isEquipped = IsEssenceEquipped(essence.item.uniqueId);
                
                if (essenceData.type == type && !isEquipped)
                {
                    availableEssences.Add(essence);
                }
            }
            
            return availableEssences;
        }

        public EssenceType GetEssenceTypeByIndex(int index)
        {
            return index switch
            {
                0 or 1 => EssenceType.Standard,      // S1, S2
                2 => EssenceType.Behavioural,         // B1
                3 => EssenceType.Standard,            // S3
                4 => EssenceType.Behavioural,         // B2
                5 => EssenceType.Standard,            // S4
                6 => EssenceType.Behavioural,         // B3
                7 => EssenceType.Standard,            // S5
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Invalid essence slot index (max 7)")
            };
        }

        public void Save(Data.Entities.Game.GameSessionData gameData)
        {
            gameData.consumables = Consumables;
            gameData.equipment = Equipments;
            gameData.essences = Essences;
            gameData.keys = KeyItems;
            gameData.lifes = Lifes;

            gameData.acc1 = Acc1;
            gameData.acc2 = Acc2;
            gameData.acc3 = Acc3;
            gameData.body = Body;

            gameData.essenceS1 = EssenceS1;
            gameData.essenceS2 = EssenceS2;
            gameData.essenceB1 = EssenceB1;
            gameData.essenceS3 = EssenceS3;
            gameData.essenceB2 = EssenceB2;
            gameData.essenceS4 = EssenceS4;
            gameData.essenceB3 = EssenceB3;
            gameData.essenceS5 = EssenceS5;
            // Old slots S6-S10 are not written anymore (backwards compatibility - they remain null in save file)
            gameData.essenceB3 = EssenceB3;
        }

        public List<PitObjectData> GetAllPitObjects()
        {
            var pitObjectIds = new HashSet<PitObjectId>();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var essence in EquippedStandardEssences())
            {
                var objects = essence.item.objects;
                foreach (var obj in objects)
                {
                    pitObjectIds.Add(obj);
                }
            }

            var pitObjects = new List<PitObjectData>();
            foreach (var pitObjectId in pitObjectIds)
            {
                var pitObject = DataSource.Instance.GetPitObject(pitObjectId);
                if (pitObject != null)
                {
                    pitObjects.Add(pitObject);
                }
                else
                {
                    Debug.LogError($"Pit object with id {pitObjectId} not found in data source");
                }
            }

            return pitObjects;
        }

        #endregion

        #region Utility

        private bool IsEssenceEquipped(string essenceId)
        {
            var equippedEssences = new List<InventoryEssenceItem>
            {
                EssenceS1, EssenceS2, EssenceB1, EssenceS3, EssenceB2, EssenceS4, EssenceB3, EssenceS5
            }.Where(e => e != null).ToList();

            return equippedEssences
                .Exists(e => e != null && e.item.uniqueId == essenceId);
        }

        #endregion
    }

    #region Helper Classes

    [Serializable]
    public class InventoryEssenceItem
    {
        public EssenceVersion item;

        public int quantity;

        public DateTime PickedUpAt;

        public InventoryEssenceItem(EssenceVersion item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
            PickedUpAt = DateTime.UtcNow;
        }
    }

    [Serializable]
    public class InventoryPitObjectItem
    {
        public PitObjectId item;

        public InventoryPitObjectItem(PitObjectId item)
        {
            this.item = item;
        }
    }

    [Serializable]
    public class InventoryEquipmentItem
    {
        public EquipmentId item;

        public int quantity;

        public DateTime PickedUpAt;

        public DateTime LastUsedTime;

        public InventoryEquipmentItem(EquipmentId item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
            PickedUpAt = DateTime.UtcNow;
        }
    }

    [Serializable]
    public class InventoryKeyItem
    {
        public KeyId item;

        public int quantity;

        public DateTime PickedUpAt;

        public DateTime LastUsedTime;

        public InventoryKeyItem(KeyId item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
            PickedUpAt = DateTime.UtcNow;
        }
    }

    [Serializable]
    public class InventoryConsumableItem
    {
        public ConsumableId item;

        public int quantity;

        public DateTime PickedUpAt;

        public DateTime LastUsedTime;

        public InventoryConsumableItem(ConsumableId item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
            PickedUpAt = DateTime.UtcNow;
        }
    }

    [Serializable]
    public class InventoryLifeItem
    {
        public LifeId item;
        public int quantity;
        public DateTime PickedUpAt;
        public DateTime LastUsedTime;

        public InventoryLifeItem(LifeId item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
            PickedUpAt = DateTime.UtcNow;
        }
    }

    public class InventoryItemSlot : Item
    {

        public KeyId KeyId;

        public LifeId LifeId;

        public readonly ConsumableId ConsumableId;

        public readonly EquipmentId EquipmentId;

        public EssenceId EssenceId;

        public TalentId TalentId;

        public readonly InventoryItemSlotType SlotType;

        public readonly int Quantity;

        public DateTime PickedUpAt;

        public InventoryItemSlot(InventoryKeyItem item)
        {
            var itemData = DataSource.Instance.GetKeyItem(item.item);

            Name = itemData.Name;
            Description = itemData.Description;
            ShortDescription = itemData.ShortDescription;
            GamePlayDescription = itemData.GamePlayDescription;
            SmallIcon = itemData.SmallIcon;
            Icon = itemData.Icon;

            SlotType = InventoryItemSlotType.KEY;
            KeyId = item.item;
            Quantity = item.quantity;
            PickedUpAt = item.PickedUpAt;
        }

        public InventoryItemSlot(InventoryConsumableItem item)
        {
            var itemData = DataSource.Instance.GetConsumableItem(item.item);

            Name = itemData.Name;
            Description = itemData.Description;
            ShortDescription = itemData.ShortDescription;
            GamePlayDescription = itemData.GamePlayDescription;
            SmallIcon = itemData.SmallIcon;
            Icon = itemData.Icon;

            SlotType = InventoryItemSlotType.CONSUMABLE;
            ConsumableId = item.item;
            Quantity = item.quantity;
            PickedUpAt = item.PickedUpAt;
        }

        public InventoryItemSlot(InventoryEquipmentItem item)
        {
            var itemData = DataSource.Instance.GetEquipmentItem(item.item);

            Name = itemData.Name;
            Description = itemData.Description;
            ShortDescription = itemData.ShortDescription;
            GamePlayDescription = itemData.GamePlayDescription;
            SmallIcon = itemData.SmallIcon;
            Icon = itemData.Icon;

            SlotType = InventoryItemSlotType.EQUIPMENT;
            EquipmentId = item.item;
            Quantity = item.quantity;
            PickedUpAt = item.PickedUpAt;
        }

        public InventoryItemSlot(InventoryEssenceItem item)
        {
            var itemData = DataSource.Instance.GetEssenceItem(item.item.sourceId);

            Name = itemData.Name;
            Description = itemData.Description;
            ShortDescription = itemData.ShortDescription;
            GamePlayDescription = itemData.GamePlayDescription;
            SmallIcon = itemData.SmallIcon;
            Icon = itemData.Icon;

            SlotType = InventoryItemSlotType.ESSENCE;
            EssenceId = item.item.sourceId;
            Quantity = item.quantity;
            PickedUpAt = item.PickedUpAt;
        }

        public InventoryItemSlot(InventoryLifeItem item)
        {
            var itemData = DataSource.Instance.GetLifeItem(item.item);

            Name = itemData.Name;
            Description = itemData.Description;
            ShortDescription = itemData.ShortDescription;
            GamePlayDescription = itemData.GamePlayDescription;
            SmallIcon = itemData.SmallIcon;
            Icon = itemData.Icon;
            SlotType = InventoryItemSlotType.LIFE;
            LifeId = item.item;
            Quantity = item.quantity;
            PickedUpAt = item.PickedUpAt;
        }

        public InventoryItemSlot(TalentItem item)
        {
            Name = item.Name;
            Description = item.Description;
            ShortDescription = item.ShortDescription;
            GamePlayDescription = item.GamePlayDescription;
            SmallIcon = item.Icon;
            Icon = item.Icon;

            SlotType = InventoryItemSlotType.TALENT;
        }
    }

    public enum InventoryItemSlotType
    {
        CONSUMABLE,
        EQUIPMENT,
        ESSENCE,
        TALENT,
        KEY,
        LIFE
    }

    #endregion

    #region Events

    // TODO: Move in UI
    /// <summary>
    /// When the user selects an essence to equip in the equipment menu.
    /// </summary>
    public struct EEssenceSelected
    {
        public readonly InventoryEssenceItem SelectedEssence;
        public readonly int Slot;

        public EEssenceSelected(InventoryEssenceItem selectedEssence, int slot)
        {
            SelectedEssence = selectedEssence;
            Slot = slot;
        }
    }

    /// <summary>
    /// When items are equipped.
    /// </summary>
    public struct EItemsEquipped
    {
        public readonly List<InventoryEquipmentItem> Items;

        public EItemsEquipped(List<InventoryEquipmentItem> items)
        {
            Items = items;
        }
    }

    /// <summary>
    /// When items are unequipped.
    /// </summary>
    public struct EItemsUnequipped
    {
        public readonly List<InventoryEquipmentItem> Items;

        public EItemsUnequipped(List<InventoryEquipmentItem> items)
        {
            Items = items;
        }
    }

    /// <summary>
    /// When an item is used from the inventory or quick access.
    /// </summary>
    public struct EItemUsed
    {
        public readonly InventoryItemSlot Item;

        public readonly int Quantity;

        public EItemUsed(InventoryItemSlot item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }
    }

    /// <summary>
    /// When an essence is equipped.
    /// </summary>
    public struct EEssenceEquipped
    {
        public readonly InventoryEssenceItem Essence;

        public EEssenceEquipped(InventoryEssenceItem essence)
        {
            Essence = essence;
        }
    }

    /// <summary>
    /// When an item is added to the inventory.
    /// </summary>
    public struct EItemAddedToInventory
    {
        public readonly InventoryItemSlot Item;

        public EItemAddedToInventory(InventoryItemSlot item)
        {
            Item = item;
        }
    }


    #endregion

    public sealed class ItemUsageController
    {
        private static ItemUsageController _instance;
        public static ItemUsageController Instance => _instance ?? throw new InvalidOperationException("ItemUsageController not initialized. Call Initialize() first.");
        private readonly IPlayerModel _player;
        private readonly IGameDataModel _gameData;

        private ItemUsageController(IPlayerModel player, IGameDataModel gameData)
        {
            _player = player;
            _gameData = gameData;
        }

        public static void Initialize(IPlayerModel player, IGameDataModel gameData)
        {
            if (_instance != null)
                throw new InvalidOperationException("ItemUsageController is already initialized.");
            _instance = new ItemUsageController(player, gameData);
        }

        public bool CanUseItem(ConsumableId id)
        {
            switch (id)
            {
                case ConsumableId.MagicFormula:
                    return !_player.InCombat();
                default:
                    Debug.Log("Item usage check not implemented for this item type, allowing usage by default");
                    return true;
            }
        }

        public string GetItemUsageRequirementText(ConsumableId id)
        {
            switch (id)
            {
                case ConsumableId.MagicFormula:
                    return "Can only be used outside of combat.";
                default:
                    return "";
            }
        }
    }
}