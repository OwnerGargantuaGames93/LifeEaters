using System;
using System.Collections.Generic;
using Data.Entities.Item;

namespace Data.Entities.Enemy
{
    [Serializable]
    public class EnemyModel
    {
        public string Id;

        public int MaxHealth;

        public int CurrentHealth;

        public int PointsDrop;

        public int JumpAttackDefense;

        public int ThrowAttackDefense;

        public EnemyStatus CurrentStatus;

        public List<LifeDrop> PossibleLifeDrops;

        public EnemyPhase CurrentPhase;

        public int Phase2HealthThreshold;

        public int PoisonResistance;

        public int BurnResistance;

        public int FrostResistance;

        public float CurrentPoisonAmount;

        public float CurrentBurnAmount;

        public float CurrentFrostAmount;

        public EnemyModel(int maxHealth, int pointsDrop, int jumpAttackDefense, int throwAttackDefense, string id, List<LifeDrop> possibleLifeDrops = null)
        {
            Id = id;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            JumpAttackDefense = jumpAttackDefense;
            ThrowAttackDefense = throwAttackDefense;
            PointsDrop = pointsDrop;
            CurrentStatus = EnemyStatus.Normal;
            PossibleLifeDrops = possibleLifeDrops ?? new List<LifeDrop>();
            Phase2HealthThreshold = 0;
            CurrentPhase = EnemyPhase.Phase1;
            PoisonResistance = 5;
            BurnResistance = 5;
            FrostResistance = 5;
        }

        public EnemyModel(
            int maxHealth,
            int pointsDrop,
            int jumpAttackDefense,
            int throwAttackDefense,
            string id,
            List<LifeDrop> possibleLifeDrops = null,
            int phase2HealthThreshold = 0,
            int poisonResistance = 5,
            int burnResistance = 5,
            int frostResistance = 5
        )
        {
            Id = id;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            JumpAttackDefense = jumpAttackDefense;
            ThrowAttackDefense = throwAttackDefense;
            PointsDrop = pointsDrop;
            CurrentStatus = EnemyStatus.Normal;
            PossibleLifeDrops = possibleLifeDrops ?? new List<LifeDrop>();
            Phase2HealthThreshold = phase2HealthThreshold;
            CurrentPhase = EnemyPhase.Phase1;
            PoisonResistance = poisonResistance;
            BurnResistance = burnResistance;
            FrostResistance = frostResistance;
        }

    }

    public enum EnemyStatus
    {
        Dead,
        Burned,
        Frozen,
        Poisoned,
        Normal
    }
    
    public enum EnemyPhase
    {
        Phase1,
        Phase2,
        Phase3,
        Phase4
    }

    [Serializable]
    public class LifeDrop
    {
        /// <summary>
        /// Life Consumable Id
        /// </summary>
        public LifeId lifeId;
        
        /// <summary>
        /// Drop chance from 0 to 1 (0% to 100%)
        /// </summary>
        public float dropChance;
        
        public LifeDrop(LifeId lifeId, float dropChance)
        {
            this.lifeId = lifeId;
            this.dropChance = dropChance;
        }
    }
}