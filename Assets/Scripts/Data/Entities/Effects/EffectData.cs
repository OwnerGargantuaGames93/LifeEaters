using System;

namespace Data.Entities.Effects
{
    [Serializable]
    public class EffectData
    {
        public EffectValueType ValueType;
        public float Value;
        public float Duration; // 0 for instant effects
        public EffectType Type;
        public bool UntilDeath = false; // Effect lasts until death if true
        public float Probability = 1f; // Chance for the effect to occur (0-1)
        
        public static EffectData CreatePointsCollectedEffect(int value)
        {
            return new EffectData
            {
                ValueType = EffectValueType.Static,
                Value = value,
                Duration = 0,
                Type = EffectType.PointsCollected
            };
        }
        
        public static EffectData CreateCoinsCollectedEffect(int value)
        {
            return new EffectData
            {
                ValueType = EffectValueType.Static,
                Value = value,
                Duration = 0,
                Type = EffectType.CoinsCollected
            };
        }

        public static EffectData CreateKillPlayerEffect()
        {
            return new EffectData
            {
                ValueType = EffectValueType.Static,
                Value = 0,
                Duration = 0,
                Type = EffectType.KillPlayer
            };
        }

        public static EffectData CreateBurnEffect()
        {
            return new EffectData
            {
                ValueType = EffectValueType.Percentage,
                Value = 50,
                Duration = 30,
                Type = EffectType.Burned
            };
        }
        
        public static EffectData CreatePoisonEffect()
        {
            return new EffectData
            {
                ValueType = EffectValueType.Static,
                Value = 1,
                Duration = 60,
                Type = EffectType.Poisoned
            };
        }

        public static EffectData CreateFrostbittenEffect()
        {
            return new EffectData
            {
                ValueType = EffectValueType.Static,
                Value = 1,
                Duration = 30,
                Type = EffectType.Frostbitten
            };
        }
        
        public static EffectData CreateEnergyCollectedEffect(float value)
        {
            return new EffectData
            {
                ValueType = EffectValueType.Static,
                Value = value,
                Duration = 0,
                Type = EffectType.EnergyRecovery
            };
        }
        
        public static EffectData CreateEnergyConsumptionEffect(float value)
        {
            return new EffectData
            {
                ValueType = EffectValueType.Static,
                Value = value,
                Duration = 0,
                Type = EffectType.EnergyConsumption
            };
        }

        public static EffectData CreateLifeGain(int lifes)
        {
            return new EffectData
            {
                ValueType = EffectValueType.Static,
                Value = lifes,
                Duration = 0,
                Type = EffectType.LifeGain
            };
        }
    }
    
    public enum EffectType
    {
        Heal,
        PointsCollected,
        CoinsCollected,
        PhysicalDamage,
        KillPlayer,
        FireDamage,
        FrostDamage,
        PoisonDamage,
        StunDamage,
        BuffFrostResistance,
        BuffDropRate,
        Burned,
        Poisoned,
        Frostbitten,
        EnergyConsumption,
        EnergyRecovery,
        LifeGain,
        DefenseBuff,
        FocusGain,
        ReturnToStatue,
        UnveilStandardHiddenWalls,
        SaveGame
    }

    public enum EffectValueType
    {
        Static,
        Percentage
    }
}