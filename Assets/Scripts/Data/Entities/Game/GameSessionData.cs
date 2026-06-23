using System;
using System.Collections.Generic;
using Control.Inventory;
using Data.Entities.Effects;
using Data.Entities.Item;
using Data.Entities.Player;
using Data.Entities.Shops;
using UnityEngine;
using Utils;

namespace Data.Entities.Game
{
    [Serializable]
    public class GameSessionData
    {
        public string sceneName;

        public Vector3 lastRespawnPosition;

        public Vector3 initialPosition;

        public PlayerCharacteristics characteristics;
        
        public PlayerCharacteristics additionalCharacteristics;
        
        public PlayerAttributes attributes;
        
        public PlayerAttributes additionalAttributes;
        
        public List<EffectData> effectsOnDeath;
        
        public PlayerStatus status;

        public List<TalentId> talents;
        
        public List<TalentId> additionalTalents;

        public List<InventoryConsumableItem> consumables;
        
        public List<InventoryLifeItem> lifes;

        public List<InventoryEquipmentItem> equipment;
        
        public InventoryEquipmentItem body;
        
        public InventoryEquipmentItem acc1;
        
        public InventoryEquipmentItem acc2;
        
        public InventoryEquipmentItem acc3;

        public List<InventoryKeyItem> keys;

        public List<InventoryEssenceItem> essences;
        
        public InventoryEssenceItem essenceS1;
        
        public InventoryEssenceItem essenceS2;
        
        public InventoryEssenceItem essenceS3;
        
        public InventoryEssenceItem essenceS4;
        
        public InventoryEssenceItem essenceS5;
        
        public InventoryEssenceItem essenceS6;
        
        public InventoryEssenceItem essenceS7;
        
        public InventoryEssenceItem essenceS8;
        
        public InventoryEssenceItem essenceS9;
        
        public InventoryEssenceItem essenceS10;
        
        public InventoryEssenceItem essenceB1;
        
        public InventoryEssenceItem essenceB2;
        
        public InventoryEssenceItem essenceB3;
        
        public List<string> collectibles;

        public List<string> statues;
        
        public List<string> defeatedEnemies;
        
        public List<string> combatRoomsCompleted;
        
        public string inkDialogueState;
        
        // public List<BossId> defeatedBosses;
        
        public List<string> doorsOpened;

        public List<string> gameplayEvents;

        public double gameTimeInSeconds;

        public long startTime;
        
        public long endTime;
        
        public List<Shop> shopStatus;
        
        public List<NpcQuestStateData> npcQuestStates;
        
        public void CreateForNewGame()
        {
            sceneName = Constants.Level11ParentHouse;
            lastRespawnPosition = Constants.Level11ParentHouseRespawnPosition;
            initialPosition = Constants.Level11ParentHouseRespawnPosition;
            characteristics = new PlayerCharacteristics();
            additionalCharacteristics = PlayerCharacteristics.Identity();
            attributes = PlayerAttributes.InitialValue();
            additionalAttributes = new PlayerAttributes();
            talents = new List<TalentId>
            {
                TalentId.Run,
                TalentId.Jump,
                TalentId.Grab,
                TalentId.Climber,
                TalentId.LifeEater,
                TalentId.Pit,
            };
            additionalTalents = new List<TalentId>();
            effectsOnDeath = new List<EffectData>();
            status = new PlayerStatus(attributes);
            // status.coins = 500;
            consumables = new List<InventoryConsumableItem>();
            lifes = new List<InventoryLifeItem>();
            
            var commonClothesInvItem = new InventoryEquipmentItem(EquipmentId.Pajamas, 1);
            
            equipment = new List<InventoryEquipmentItem>()
            {
                commonClothesInvItem,
            };
            body = commonClothesInvItem;
            acc1 = null;
            acc2 = null;
            acc3 = null;
            keys = new List<InventoryKeyItem>();
            essences = new List<InventoryEssenceItem>();
            
            // ////////////////////////////////////////////////////////
            // Test Code
            // ////////////////////////////////////////////////////////
            /*
            var essence1 = DataSource.Instance.GetEssenceItem(EssenceId.Shepherd);
            var version1 = essence1.CreateEssenceVersion();
            var inventoryItem1 = new InventoryEssenceItem(version1, 1);
            */
            
            var version = new EssenceVersion
            {
                uniqueId = "InitialMinerEssence",
                objects = new List<PitObjectId> { PitObjectId.ClayBlock },
                sourceId = EssenceId.Miner,
            };
            
            var inventoryItem = new InventoryEssenceItem(version, 1);
            
            essences.Add(inventoryItem);
            
            /*
            var essence3 = DataSource.Instance.GetEssenceItem(EssenceId.Roller);
            var version3 = essence3.CreateEssenceVersion();
            var inventoryItem3 = new InventoryEssenceItem(version3, 1);
            
            essences.Add(inventoryItem1);
            essences.Add(inventoryItem2);
            essences.Add(inventoryItem3);

            essenceS1 = inventoryItem1;
            essenceS2 = inventoryItem2;
            */
            // ////////////////////////////////////////////////////////
            
            collectibles = new List<string>();
            statues = new List<string>();
            defeatedEnemies = new List<string>();
            combatRoomsCompleted = new List<string>();
            doorsOpened = new List<string>();
            gameplayEvents = new List<string>();
            shopStatus = new List<Shop>();
            npcQuestStates = new List<NpcQuestStateData>();

            inkDialogueState = "";
            startTime = DateTime.Now.Ticks;
            gameTimeInSeconds = 0;
        }
    }
    
    /// <summary>
    /// Serializable structure to save NPC quest states.
    /// Tracks which phase each NPC is currently at.
    /// </summary>
    [Serializable]
    public struct NpcQuestStateData
    {
        public string npcName;
        public string currentPhaseName;
    }
}