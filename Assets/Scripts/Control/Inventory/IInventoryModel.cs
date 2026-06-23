using System.Collections.Generic;
using Boundary.UI.StatueMenu.EquipmentMenu;
using Data.Entities.Game;
using Data.Entities.Item;
using UnityEngine;

namespace Control.Inventory
{
    public interface IInventoryModel
    {
        public List<InventoryItemSlot> GetItemSlots();
        public bool HasKeyObject(KeyId keyId);
        public bool IsItemEquipped(EquipmentId equipmentId);
        public List<InventoryEquipmentItem> GetEquipmentItems(EquipmentType type);
        public List<InventoryEquipmentItem> GetCurrentEquipment();
        public Sprite GetEquipmentImage(EquipmentSlot selectedEquipmentSlot);
        public string GetEquipmentInfo(EquipmentSlot selectedEquipmentSlot);
        public string GetEquipmentChangeString(InventoryItemSlot equipment, EquipmentSlot selectedEquipmentSlot);
        public List<InventoryEssenceItem> GetEquippedEssences();
        public List<InventoryEssenceItem> EquippedStandardEssences();
        public List<InventoryEssenceItem> EquippedBehaviouralEssences();
        public List<InventoryEssenceItem> GetAvailableEssences(EssenceType type);
        public List<PitObjectData> GetAllPitObjects();
        
        public List<InventoryConsumableItem> Consumables { get; }
        public List<InventoryEquipmentItem> Equipments { get; }
        public List<InventoryKeyItem> KeyItems { get; }
        public List<InventoryEssenceItem> Essences { get; }
        
        public InventoryEquipmentItem Body { get; }
        public InventoryEquipmentItem Acc1 { get; }
        public InventoryEquipmentItem Acc2 { get; }
        public InventoryEquipmentItem Acc3 { get; }
        
        public InventoryEssenceItem EssenceS1 { get; }
        public InventoryEssenceItem EssenceS2 { get; }
        public InventoryEssenceItem EssenceB1 { get; }
        public InventoryEssenceItem EssenceS3 { get; }
        public InventoryEssenceItem EssenceB2 { get; }
        public InventoryEssenceItem EssenceS4 { get; }
        public InventoryEssenceItem EssenceB3 { get; }
        public InventoryEssenceItem EssenceS5 { get; }

        public EssenceType GetEssenceTypeByIndex(int index);
        
        public void Save(GameSessionData gameData);
    }
}