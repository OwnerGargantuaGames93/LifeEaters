using System;
using Data.Damage;
using UnityEngine;

namespace Data.Entities.Item
{
    [Serializable]
    public class PitObjectData : Item
    {
        public PitObjectId id;
        
        public PitObjectPrimaryType primaryType;
        
        public PitObjectSecondaryType secondaryType;
        
        public PitObjectCategory category;

        public DamageOutput damage;

        public GameObject @object;

        public int weight;

        public int difficulty;

        public PitObjectRarity rarity;

        public float energyCost;
    }

    public enum PitObjectCategory
    {
        Insect,
        Explosive,
        Structural,
        Food,
        Magical,
        Plants,
        Dust,
        Miscellaneous,
    }

    public enum PitObjectPrimaryType
    {
        Attack,
        Defense,
        Bonus
    }
    
    public enum PitObjectSecondaryType
    {
        Melee,
        Ranged,
        Shield,
        Contact,
        Tools,
    }
    
    public enum PitObjectId
    {
        ClayBlock,
        Wheel,
        Wool,
    }
    
    public enum PitObjectRarity
    {
        Common, // drop rate value 25-60% - 75% to be in present
        Uncommon, // drop rate value 10-40% - 50% to be in present
        Rare, // drop rate value 0-25% - 30% to be in present
        Epic, // drop rate value 0-10% - 20% to be in present
        Legendary, // drop rate value 0-5% - 10% to be in present
    }
}