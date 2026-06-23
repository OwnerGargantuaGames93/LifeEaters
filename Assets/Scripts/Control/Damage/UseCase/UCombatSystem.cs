using System;
using Control.Inventory;
using Control.Player;
using Data.Damage;
using Data.Entities.Enemy;
using Data.Entities.Item;
using UnityEngine;

namespace Control.Damage.UseCase
{
    public class UCombatSystem
    {
        private readonly IPlayerModel _playerModel;
        private readonly IInventoryModel _inventoryModel;
        
        public UCombatSystem(IPlayerModel playerModel, IInventoryModel inventoryModel)
        {
            _playerModel = playerModel;
            _inventoryModel = inventoryModel;
        }

        public DamageOutput CalculateJumpAttackDamage(EnemyModel enemyData)
        {
            var playerAttributes = _playerModel.Attributes;
            var damage = (int)MathF.Max(0, playerAttributes.JumpAttackBaseDamage - enemyData.JumpAttackDefense);
            var poisonBuildUp = (int)playerAttributes.JumpAttackPoisonBuildUp;
            var burnBuildUp = (int)playerAttributes.JumpAttackBurnBuildUp;
            var frostBuildUp = (int)playerAttributes.JumpAttackFrostBuildUp;

            enemyData.CurrentHealth -= damage;

            if (enemyData.CurrentHealth <= 0)
            {
                enemyData.CurrentStatus = EnemyStatus.Dead;
            }
            else
            {
                ApplyStatusBuildUps(enemyData, poisonBuildUp, burnBuildUp, frostBuildUp);
            }

            CheckPhase2Transition(enemyData);

            // TODO: Calculate critical hits

            return new DamageOutput(
                healthDamage: damage,
                burntBuildUp: burnBuildUp,
                frostBuildUp: frostBuildUp,
                poisonBuildUp: poisonBuildUp,
                directLifeSteal: 0
            );
        }

        public DamageOutput CalculateThrowAttackDamage(EnemyModel enemyData, PitObjectData data)
        {
            var playerAttributes = _playerModel.Attributes;
            var throwDamage = playerAttributes.ThrowAttackBaseDamage;
            var objectDamage = CalculatePitObjetDamage(data);

            var totalDamage = throwDamage + objectDamage;
            var damage = (int) MathF.Max(0, totalDamage - enemyData.ThrowAttackDefense);
            var poisonBuildUp = data.damage.poisonBuildUp;
            var burnBuildUp = data.damage.burntBuildUp;
            var frostBuildUp = data.damage.frostBuildUp;

            enemyData.CurrentHealth -= damage;

            if (enemyData.CurrentHealth <= 0)
            {
                enemyData.CurrentStatus = EnemyStatus.Dead;
            }
            else
            {
                ApplyStatusBuildUps(enemyData, poisonBuildUp, burnBuildUp, frostBuildUp);
            }

            CheckPhase2Transition(enemyData);

            // TODO: Calculate critical hits

            return new DamageOutput(
                healthDamage: damage,
                burntBuildUp: burnBuildUp,
                frostBuildUp: frostBuildUp,
                poisonBuildUp: poisonBuildUp,
                directLifeSteal: 0
            );
        }
        
        private static void ApplyStatusBuildUps(EnemyModel enemyData, int poisonBuildUp, int burnBuildUp, int frostBuildUp)
        {
            if (poisonBuildUp > 0 && enemyData.CurrentStatus != EnemyStatus.Poisoned)
            {
                enemyData.CurrentPoisonAmount += poisonBuildUp;
                if (enemyData.CurrentPoisonAmount >= enemyData.PoisonResistance)
                    enemyData.CurrentStatus = EnemyStatus.Poisoned;
            }

            if (burnBuildUp > 0)
                enemyData.CurrentBurnAmount += burnBuildUp;

            if (frostBuildUp > 0)
                enemyData.CurrentFrostAmount += frostBuildUp;
        }

        private static void CheckPhase2Transition(EnemyModel data)
        {
            if (data.Phase2HealthThreshold > 0 && data.CurrentPhase == EnemyPhase.Phase1)
            {
                if (data.CurrentHealth <= data.Phase2HealthThreshold)
                {
                    data.CurrentPhase = EnemyPhase.Phase2;
                }
            }
        }

        private int CalculatePitObjetDamage(PitObjectData data)
        {
            var objectDamage = data.damage.healthDamage;
            
            // Add special additional damange
            if (data.id == PitObjectId.ClayBlock && _inventoryModel.IsItemEquipped(EquipmentId.MeteorPendant))
            {
                objectDamage += (int) (data.damage.healthDamage * 0.5f);
            }

            return objectDamage;
        }

    }
}