using System.Collections.Generic;

namespace Control.GameData
{
    public interface IGameDataModel
    {
        public bool IsStatueActivated(string statueId);
        public bool IsObjectCollected(string collectedId);
        public bool IsCombatRoomCleared(string roomId);
        public bool IsDefeatedEnemy(string enemyId);
        public bool IsDoorOpened(string doorId);
        public bool HasGameplayEvent(string eventId);
        public List<SaveSlot> SaveSlots { get; }
        public List<SaveSlot> LoadableSaveSlots { get; }
        public int? LastSavedSlot { get; }
    }
}