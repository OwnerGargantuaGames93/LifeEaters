using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Boundary.Camera;
using Boundary.Collectibles;
using Boundary.GamePlay.Enemy.Base;
using Boundary.Interactable;
using Boundary.Loot;
using Boundary.UI.GameMenu.LoadGame;
using Boundary.UI.StatueMenu.SaveMenu;
using Control.DialogueHandler;
using Control.Inventory;
using Control.Player;
using Control.Quests;
using Control.Shop;
using Data.Entities.Game;
using Infra.EventBus;
using Infra.SceneHandler;
using UnityEngine;

namespace Control.GameData
{
    public class GameDataModel: IGameDataModel
    {
        private const int MaxSaveSlots = 6;
        
        private readonly IEventBus _eventBus;
        private readonly IInventoryModel _inventory;
        private readonly IPlayerModel _playerModel;
        private readonly ISceneHandler _sceneHandler;
        private readonly IDialogueHandler _dialogueHandler;
        private readonly IShopModel _shopModel;
        private readonly IQuestModel _questModel;
        
        private GameSessionData _gameData;

        public List<SaveSlot> SaveSlots { get; } = new();
        public List<SaveSlot> LoadableSaveSlots { get; } = new();
        public int? LastSavedSlot { get; private set; }

        public GameDataModel(
            IEventBus eventBus,
            IInventoryModel inventory,
            IPlayerModel playerModel,
            ISceneHandler sceneHandler,
            IDialogueHandler dialogueHandler,
            IShopModel shopModel,
            IQuestModel questModel
            )
        {
            _eventBus = eventBus;
            _inventory = inventory;
            _playerModel = playerModel;
            _sceneHandler = sceneHandler;
            _dialogueHandler = dialogueHandler;
            _shopModel = shopModel;
            _questModel = questModel;

            SubscribeToEvents();

            PopulateSaveSlots();
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EBonusCollected>(OnBonusCollected);
            _eventBus.Subscribe<ECoinCollected>(OnCoinCollected);
            _eventBus.Subscribe<EPlayerRestOnStatue>(OnPlayerRestOnStatue);
            _eventBus.Subscribe<EPlayerActivateStatue>(OnPlayerActivateStatue);
            _eventBus.Subscribe<EEnemyDied>(OnEnemyDied);
            _eventBus.Subscribe<EEnemyRespawned>(OnEnemyRespawned);
            _eventBus.Subscribe<EConsumableLooted>(OnConsumablesLooted);
            _eventBus.Subscribe<EEquipmentLooted>(OnEquipmentsLooted);
            _eventBus.Subscribe<EEssenceLooted>(OnEssencesLooted);
            _eventBus.Subscribe<EKeyLooted>(OnKeyItemsLooted);
            _eventBus.Subscribe<ELifeLooted>(OnLifeItemsLooted);
            _eventBus.Subscribe<EGameDataToLoadSelected>(OnGameDataToLoadSelected);
            _eventBus.Subscribe<ESaveGameDataUiCommand>(OnSaveGameDataUiCommand);
            _eventBus.Subscribe<ENewGameStartUiCommand>(OnNewGameStartUiCommand);
            _eventBus.Subscribe<ECombatRoomCompleted>(OnCombatRoomCompleted);
            _eventBus.Subscribe<EDoorOpened>(OnDoorOpened);
            _eventBus.Subscribe<EGameplayEventOccurred>(OnGameplayEventOccurred);
            _eventBus.Subscribe<ESaveGameOnLastSlot>(SaveGameIntoLastUsedSlot);
        }

        #region Event Handlers

        private void OnBonusCollected(EBonusCollected e)
        {
            var id = e.BonusData.Id;
            _gameData.collectibles.Add(id);
        }
        
        private void OnCoinCollected(ECoinCollected e)
        {
            var id = e.CoinId;
            _gameData.collectibles.Add(id);
        }

        private void OnPlayerRestOnStatue(EPlayerRestOnStatue e)
        {
            var statueId = e.StatueData.Id;

            if (_gameData.statues.Find(s => s == statueId) == null)
            {
                _gameData.statues.Add(e.StatueData.Id);
            }
        }
        
        private void OnPlayerActivateStatue(EPlayerActivateStatue e)
        {
            _gameData.lastRespawnPosition = e.StatueData.Position;
            _gameData.initialPosition = e.StatueData.Position;
        }
        
        private void OnConsumablesLooted(EConsumableLooted e)
        {
            var id = e.Id;
            _gameData.collectibles.Add(id);
        }
        
        private void OnEquipmentsLooted(EEquipmentLooted e)
        {
            var id = e.Id;
            
            _gameData.collectibles.Add(id);
        }
        
        private void OnEssencesLooted(EEssenceLooted e)
        {
            var id = e.Id;
            
            if (_gameData.collectibles.Exists(collectedId => collectedId == id))
            {
                return;
            }
            
            _gameData.collectibles.Add(id);
        }
        
        private void OnKeyItemsLooted(EKeyLooted e)
        {
            var id = e.Id;
            
            if (_gameData.collectibles.Exists(collectedId => collectedId == id))
            {
                return;
            }
            
            _gameData.collectibles.Add(id);
        }
        
        private void OnLifeItemsLooted(ELifeLooted e)
        {
            var id = e.Id;
            
            if (_gameData.collectibles.Exists(collectedId => collectedId == id))
            {
                return;
            }
            
            _gameData.collectibles.Add(id);
        }
        
        private void OnEnemyDied(EEnemyDied e)
        {
            var enemyId = e.Enemy.Id;
            _gameData.defeatedEnemies.Add(enemyId);
        }

        private void OnEnemyRespawned(EEnemyRespawned e)
        {
            var enemyId = e.Enemy.Id;
            
            if (!_gameData.defeatedEnemies.Exists(id => id == enemyId))
            {
                return;
            }
            
            _gameData.defeatedEnemies.Remove(enemyId);
        }
        
        private void OnCombatRoomCompleted(ECombatRoomCompleted e)
        {
            var roomId = e.RoomId;
            
            if (_gameData.combatRoomsCompleted.Exists(id => id == roomId))
            {
                return;
            }
            
            _gameData.combatRoomsCompleted.Add(roomId);
        }
        
        private void OnDoorOpened(EDoorOpened e)
        {
            var doorId = e.DoorId;

            if (_gameData.doorsOpened.Exists(id => id == doorId))
            {
                return;
            }

            _gameData.doorsOpened.Add(doorId);
        }

        private void OnGameplayEventOccurred(EGameplayEventOccurred e)
        {
            if (_gameData.gameplayEvents.Exists(id => id == e.EventId))
            {
                return;
            }

            _gameData.gameplayEvents.Add(e.EventId);
        }
        
        private void OnGameDataToLoadSelected(EGameDataToLoadSelected e)
        {
            var slot = e.SlotNumber;
            
            _gameData = ExtractGameDataFromSlot(slot);
            
            // Set the start time of the game to now
            _gameData.startTime = DateTime.Now.Ticks;
            
            // Update the current save slot
            LastSavedSlot = slot;
            
            _eventBus.Publish(new EGameDataLoaded(_gameData));
        }
        
        private void OnSaveGameDataUiCommand(ESaveGameDataUiCommand e)
        {
            var slot = e.SlotNumber;
            
            Debug.Log("Saving game data to slot " + slot);
            
            _inventory.Save(_gameData);
            _playerModel.Save(_gameData);
            _sceneHandler.Save(_gameData);
            _dialogueHandler.Save(_gameData);
            _shopModel.Save(_gameData);
            _questModel.Save(_gameData);
            
            Debug.Log("DateTime.Now: " + DateTime.Now);
            
            _gameData.endTime = DateTime.Now.Ticks;
            
            var endTimeDateTime = new DateTime(_gameData.endTime);
            var startTimeDateTime = new DateTime(_gameData.startTime);
            
            _gameData.gameTimeInSeconds += (endTimeDateTime - startTimeDateTime).TotalSeconds;
            
            // Store GameData in a file as JSON
            var filePath = $"{Application.persistentDataPath}/GameDataSlot{slot}.save";
            var json = JsonUtility.ToJson(_gameData, true);
            
            File.WriteAllText(filePath, json);
            
            // Update the save slot list
            PopulateSaveSlots();
            
            Debug.Log("Game data saved to " + filePath);
            
            // Update the current save slot
            LastSavedSlot = slot;
            
            _eventBus.Publish(new EGameDataSaved(slot, _gameData));
        }
        
        private void OnNewGameStartUiCommand(ENewGameStartUiCommand e)
        {
            // Create new game data
            _gameData = new GameSessionData();
            _gameData.CreateForNewGame();
            
            // Set the start time of the game to now
            _gameData.startTime = DateTime.Now.Ticks;
            
            // No last saved slot
            LastSavedSlot = null;
            
            _eventBus.Publish(new EGameDataLoaded(_gameData));
        }
        
        private void SaveGameIntoLastUsedSlot(ESaveGameOnLastSlot e)
        {
            if (LastSavedSlot == null)
            {
                Debug.LogWarning("No last saved slot found. Cannot save game data.");
                return;
            }
            
            var slot = LastSavedSlot.Value;
            
            _inventory.Save(_gameData);
            _playerModel.Save(_gameData);
            _sceneHandler.Save(_gameData);
            _dialogueHandler.Save(_gameData);
            _shopModel.Save(_gameData);
            
            _gameData.initialPosition = _playerModel.GetPlayerPosition();
            
            _gameData.endTime = DateTime.Now.Ticks;
            
            var endTimeDateTime = new DateTime(_gameData.endTime);
            var startTimeDateTime = new DateTime(_gameData.startTime);
            
            _gameData.gameTimeInSeconds += (endTimeDateTime - startTimeDateTime).TotalSeconds;
            
            // Store GameData in a file as JSON
            var filePath = $"{Application.persistentDataPath}/GameDataSlot{slot}.save";
            var json = JsonUtility.ToJson(_gameData, true);
            
            File.WriteAllText(filePath, json);
            
            Debug.Log("Game data saved to " + filePath);
        }

        #endregion

        #region Interface Methods

        public bool IsStatueActivated(string statueId)
        {
            return _gameData.statues.Exists(s => s == statueId);
        }

        public bool IsObjectCollected(string collectedId)
        {
            return _gameData.collectibles.Exists(id => id == collectedId);
        }
        
        public bool IsDefeatedEnemy(string enemyId)
        {
            // Print out all defeated enemies for debugging
            foreach (var e in _gameData.defeatedEnemies)
            {
                Debug.Log(e);
            }
            
            return _gameData.defeatedEnemies.Exists(id => id == enemyId);
        }

        public bool IsCombatRoomCleared(string roomId)
        {
            return _gameData.combatRoomsCompleted.Exists(id => id == roomId);
        }
        
        public bool IsDoorOpened(string doorId)
        {
            return _gameData.doorsOpened.Exists(id => id == doorId);
        }

        public bool HasGameplayEvent(string eventId)
        {
            return _gameData.gameplayEvents.Exists(id => id == eventId);
        }

        #endregion
        
        #region Helper Methods
        
        private static GameSessionData ExtractGameDataFromSlot(int slot)
        {
            // Load from file as JSON the game data object
            // The default path is ~/Library/Application Support/DefaultCompany on MacOS
            var filePath = $"{Application.persistentDataPath}/GameDataSlot{slot}.save";
            
            Debug.Log("filePath: " + filePath);
            
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<GameSessionData>(json);
            }
            
            Debug.Log($"No game data found for slot {slot} at {filePath}");
            return null;
        }
        
        private void PopulateSaveSlots()
        {
            if (SaveSlots.Count > 0)
            {
                SaveSlots.Clear();
            }
            
            for (var i = 0; i < MaxSaveSlots; i++)
            {
                var gameData = ExtractGameDataFromSlot(i);
                SaveSlots.Add(gameData == null ? new SaveSlot(i) : new SaveSlot(gameData, i, SaveSlotStatus.Saved));
            }

            if (LoadableSaveSlots.Count > 0)
            {
                LoadableSaveSlots.Clear();
            }

            // Add only save slot with status Saved
            foreach (var slot in SaveSlots.Where(slot => slot.Status == SaveSlotStatus.Saved))
            {
                LoadableSaveSlots.Add(slot);
            }
        }
        
        #endregion

    }
    
    public class SaveSlot
    {
        public readonly int SlotNumber;
        public DateTime SaveDateTime;
        public readonly double GameTimeInSeconds;
        public readonly int PlayerLevel;
        public readonly SaveSlotStatus Status;
        
        // TODO: public float PercentComplete;
        // TODO: public int numOfHolPrograms;

        public SaveSlot(GameSessionData gameData, int slotNumber, SaveSlotStatus status = SaveSlotStatus.Empty)
        {
            SlotNumber = slotNumber;
            SaveDateTime = new DateTime(gameData.endTime);
            GameTimeInSeconds = gameData.gameTimeInSeconds;
            PlayerLevel = gameData.characteristics.GetLevelPoints();
            Status = status;
        }

        public SaveSlot(int slotNumber)
        {
            Status = SaveSlotStatus.Empty;
            SlotNumber = slotNumber;
        }
    }
    
    public enum SaveSlotStatus
    {
        Empty,
        Saved,
    }

    #region Events

    public struct EGameplayEventOccurred
    {
        public readonly string EventId;

        public EGameplayEventOccurred(string eventId)
        {
            EventId = eventId;
        }
    }

    /// <summary>
    /// Called when the game data has loaded from the save file
    /// </summary>
    public struct EGameDataLoaded
    {
        public readonly GameSessionData GameData;
        
        public EGameDataLoaded(GameSessionData gameData)
        {
            GameData = gameData;
        }
    }
    
    public struct EGameDataSaved
    {
        public readonly int SlotNumber;
        public readonly GameSessionData GameData;
        
        public EGameDataSaved(int slotNumber, GameSessionData gameData)
        {
            SlotNumber = slotNumber;
            GameData = gameData;
        }
    }
    
    // TODO: Move to UI.MainMenu
    /// <summary>
    /// When the new game option is selected from the Main Menu UI
    /// </summary>
    public struct ENewGameStartUiCommand
    {
    }
    
    /// <summary>
    /// When needs to save the game without a statue
    /// </summary>
    public struct ESaveGameOnLastSlot {}

    #endregion
}