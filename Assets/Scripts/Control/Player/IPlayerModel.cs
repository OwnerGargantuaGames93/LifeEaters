using System.Collections.Generic;
using Control.Inventory;
using Data.Entities.Effects;
using Data.Entities.Game;
using Data.Entities.Player;
using UnityEngine;

namespace Control.Player
{
    public interface IPlayerModel
    {
        public bool CanRun { get; }
        public bool CanThrow { get; }
        public bool CanJump { get; }
        public bool CanDash { get; }
        public bool CanCreatePits { get; }
        public bool CanSwim { get; }
        public bool CanTeleport { get; }
        public bool CanPillEating { get; }
        public bool CanFlyingDash { get; }
        public bool CanSmash { get; }
        public bool CanDoubleJump { get; }
        public bool CanWallJump { get; }
        public bool CanHeadbutt { get; }
        public bool CanClimb { get; }
        public bool IsInvincible { get; }
        public bool CanGrab(int weights, int difficulty);
        public bool PickUpWeightCheck(int weight);
        public bool PickUpDifficultyCheck(int difficulty);
        public bool CanLevelUp(PlayerCharacteristics upgrade);
        public List<InventoryItemSlot> GetTalentsSlots();
        public bool CanGeneratePit();
        public int GetNumberOfPitsCanBeGenerated();
        public List<string> GetAttributeLabels();
        public List<string> GetOtherLabels();
        public bool InCombat();
        public Vector3 GetPlayerPosition();
        void SetPlayerPosition(Vector3 position);
        
        public PlayerAttributes Attributes { get; }
        public PlayerAttributes AdditionalAttributes { get; }
        public PlayerCharacteristics Characteristics { get; }
        public PlayerCharacteristics AdditionalCharacteristics { get; }
        public PlayerStatus Status { get; }
        public List<EffectData> EffectsOnDeath { get; }

        public void Save(GameSessionData gameData);

    }
}