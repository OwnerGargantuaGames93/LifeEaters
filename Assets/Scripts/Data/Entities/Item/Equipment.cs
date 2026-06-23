using System;
using System.Collections.Generic;
using Data.Entities.Player;

namespace Data.Entities.Item
{
    [Serializable]
    public class EquipmentItem : Item
    {
        public EquipmentId id;
        
        public EquipmentType type;

        public PlayerAttributes attributesModifier;

        public PlayerCharacteristics characteristicsModifier;

        public List<TalentId> talentsModifier;
    }
    
    public enum EquipmentType {
        Set,
        Accessory
    }
    
    public enum EquipmentId
    {
        Empty,
        Pajamas,
        RingOfFlesh,
        MeteorPendant,
        MithridatesRing,
        MinerSuit,
        MuddyBoots
    }
}