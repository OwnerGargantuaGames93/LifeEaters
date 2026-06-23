using System.Collections;
using System.Linq;
using Boundary.Player;
using Control.GameData;
using Control.Player;
using Data.Entities.Effects;
using Infra.EventBus;
using Infra.TilemapController;
using UnityEngine;
using Utils;

namespace Control.Effects
{
    public class EffectCalculator
    {

        private readonly IEventBus _eventBus;
        private readonly IPlayerModel _player;
        
        public EffectCalculator(IEventBus eventBus, IPlayerModel player) {
            _eventBus = eventBus;
            _player = player;
        }
        
        public void ApplyEffect(EffectData effect)
        {
            switch (effect.Type)
            {
                case EffectType.Heal:
                {
                    Heal((int) effect.Value, effect.ValueType);
                    break;
                }
                case EffectType.FocusGain:
                {
                    ChangeFocus(effect.Value, effect.ValueType);
                    break;
                }
                case EffectType.BuffDropRate:
                {
                    ChangeDropRateAttribute(effect.Value, effect.ValueType, effect.Duration);
                    break;
                }
                case EffectType.BuffFrostResistance:
                {
                    ChangeFrostResistanceAttribute(effect.Value, effect.ValueType, effect.Duration);
                    break;
                }
                case EffectType.PointsCollected:
                {
                    PointsCollected((int) effect.Value);
                    break;                    
                }
                case EffectType.CoinsCollected:
                {
                    CoinsCollected((int) effect.Value);
                    break;
                }
                case EffectType.LifeGain:
                {
                    LifeGain((int)effect.Value, effect.Probability);
                    break;
                }
                case EffectType.DefenseBuff:
                {
                    ChangeDefenseAttribute(effect.Value, effect.ValueType, effect.Duration, effect.UntilDeath);
                    break;
                }
                case EffectType.ReturnToStatue:
                {
                    ReturnToStatue();
                    break;
                }
                case EffectType.UnveilStandardHiddenWalls:
                {
                    UnveilStandardHiddenWalls(effect.Duration);
                    break;
                }
                case EffectType.SaveGame:
                {
                    SaveGameLastSlot();
                    break;
                }
                case EffectType.KillPlayer:
                {
                    KillPlayer();
                    break;
                }
                case EffectType.Poisoned:
                {
                    ApplyPoisoned(effect.Value, effect.Duration);
                    break;
                }
                case EffectType.Burned:
                {
                    ApplyBurned(effect.Value, effect.Duration);
                    break;
                }
                case EffectType.Frostbitten:
                {
                    ApplyFrostbitten(effect.Value, effect.Duration);
                    break;
                }
                case EffectType.EnergyConsumption:
                {
                    ChangeEnergy(-effect.Value, effect.ValueType);
                    break;
                }
                case EffectType.EnergyRecovery:
                {
                    ChangeEnergy(effect.Value, effect.ValueType);
                    break;
                }
                case EffectType.PhysicalDamage:
                case EffectType.FireDamage:
                case EffectType.FrostDamage:
                case EffectType.PoisonDamage:
                case EffectType.StunDamage:
                default:
                    Debug.LogWarning("Effect type not handled: " + effect.Type);
                    break;
            }
        }

        private void Heal(int value, EffectValueType valueType)
        {
            var maxHealth = _player.Attributes.MaxHealth;
            var healAmount = valueType == EffectValueType.Percentage
                ? (int) (maxHealth * (value / 100f))
                : value;

            _player.Status.currentHealth += healAmount;
            
            _eventBus.Publish(new EHpRestored(healAmount));
        }
        
        private void ChangeFocus(float value, EffectValueType valueType)
        {
            var maxFocus = _player.Attributes.MaxEnergy;
            var focusAmount = valueType == EffectValueType.Percentage
                ? (int) (maxFocus * (value / 100f))
                : value;
            
            _player.Status.currentEnergy += focusAmount;
            
            // Send this only when focus is changed to avoid unnecessary events (e.g., when energy is already full)
            _eventBus.Publish(new EEnergyChanged(focusAmount));
        }
        
        private void ChangeDropRateAttribute(float value, EffectValueType valueType, float duration) 
        {
            var currentValue = _player.Attributes.DropRate;
            var valueToAdd = valueType == EffectValueType.Percentage
                ? (currentValue * (value / 100f))
                : value;
            
            if (duration == 0)
            {
                _player.AdditionalAttributes.DropRate += valueToAdd;
            }
            else
            {
                CoroutineRunner.Instance.StartCoroutine(TemporaryBuff(
                    () =>
                    {
                        _player.AdditionalAttributes.DropRate += valueToAdd;
                    },
                    () =>
                    {
                        var effect = new EffectData
                        {
                            Type = EffectType.BuffDropRate,
                            Value = -valueToAdd,
                            ValueType = EffectValueType.Static
                        };

                        var effects = new[] { effect };
                        _eventBus.Publish(new EEffectsApplied(effects.ToList()));
                    },
                    duration));
            }
        }
        
        private void ChangeFrostResistanceAttribute(float value, EffectValueType valueType, float duration)
        {
            var currentValue = _player.Attributes.FrostResistance;
            var valueToAdd = valueType == EffectValueType.Percentage
                ? (currentValue * (value / 100f))
                : value;
            
            if (duration == 0)
            {
                _player.AdditionalAttributes.FrostResistance += (int)valueToAdd;
            }
            else
            {
                CoroutineRunner.Instance.StartCoroutine(TemporaryBuff(
                    () =>
                    {
                        _player.AdditionalAttributes.FrostResistance += (int)valueToAdd;
                    },
                    () =>
                    {
                        var effect = new EffectData
                        {
                            Type = EffectType.BuffFrostResistance,
                            Value = -valueToAdd,
                            ValueType = EffectValueType.Static
                        };

                        var effects = new[] { effect };
                        _eventBus.Publish(new EEffectsApplied(effects.ToList()));
                    },
                    duration));
            }
        }
        
        private void PointsCollected(int value)
        {
            _player.Status.points += value;
            
            _eventBus.Publish(new EPointsCollected(value));
        }
        
        private void CoinsCollected(int value)
        {
            _player.Status.coins += value;
        }

        private void LifeGain(int value, float probability)
        {
            if (Random.value > probability)
            {
                return;
            }
            
            _player.Status.currentLifes += value;
            
            _eventBus.Publish(new ELifeGained(value));
        }

        private void ChangeDefenseAttribute(float value, EffectValueType valueType, float duration, bool untilDeath)
        {
            var currentValue = _player.Attributes.Defense;
            var valueToAdd = valueType == EffectValueType.Percentage
                ? currentValue * (value / 100f)
                : value;
            
            if (duration == 0)
            {
                _player.AdditionalAttributes.Defense += valueToAdd;
            }
            else
            {
                CoroutineRunner.Instance.StartCoroutine(TemporaryBuff(
                    () =>
                    {
                        _player.AdditionalAttributes.Defense += valueToAdd;
                    },
                    () =>
                    {
                        var effect = new EffectData
                        {
                            Type = EffectType.DefenseBuff,
                            Value = -valueToAdd,
                            ValueType = EffectValueType.Static
                        };

                        var effects = new[] { effect };
                        _eventBus.Publish(new EEffectsApplied(effects.ToList()));
                    },
                    duration));
            }

            if (untilDeath)
            {
                if (duration == 0)
                {
                    _player.EffectsOnDeath.Add(new EffectData
                    {
                        Type = EffectType.DefenseBuff,
                        Value = -valueToAdd,
                        ValueType = valueType
                    });
                }
                else
                {
                    Debug.Log("Warning: Defense buff with duration cannot be removed on death.");
                }
            }
        }

        private void ReturnToStatue()
        {
            _eventBus.Publish(new ETeleportToLastStatue());
        }

        private void UnveilStandardHiddenWalls(float duration)
        {
            CoroutineRunner.Instance.StartCoroutine(TemporaryBuff(
                () =>
                {
                    _eventBus.Publish(new EHideStandardHiddenWalls());
                },
                () =>
                {
                    _eventBus.Publish(new EShowStandardHiddenWalls());
                }, duration));
        }

        private void SaveGameLastSlot()
        {
            _eventBus.Publish(new ESaveGameOnLastSlot());
        }

        private void KillPlayer()
        {
            // Force health to 0 — CalculateStatus in PlayerModel will handle life loss / game over
            var damage = new Data.Damage.DamageOutput(
                healthDamage: (int)_player.Attributes.MaxHealth,
                burntBuildUp: 0, frostBuildUp: 0, poisonBuildUp: 0, directLifeSteal: 0
            );
            _eventBus.Publish(new EPlayerReceiveDamage(damage));
        }

        /// <summary>
        /// Directly applies poison build-up to the player.
        /// Value = amount of poison build-up to add.
        /// Duration = if > 0, the build-up is removed after duration seconds (temporary exposure).
        /// </summary>
        private void ApplyPoisoned(float value, float duration)
        {
            var buildUp = (int)value;
            if (duration <= 0f)
            {
                var damage = new Data.Damage.DamageOutput(0, 0, 0, buildUp, 0);
                _eventBus.Publish(new EPlayerReceiveDamage(damage));
            }
            else
            {
                CoroutineRunner.Instance.StartCoroutine(TemporaryBuff(
                    () =>
                    {
                        var damage = new Data.Damage.DamageOutput(0, 0, 0, buildUp, 0);
                        _eventBus.Publish(new EPlayerReceiveDamage(damage));
                    },
                    () =>
                    {
                        // Reduce poison build-up by the same amount after duration
                        var removeEffect = new EffectData { Type = EffectType.Poisoned, Value = -value, ValueType = EffectValueType.Static };
                        _eventBus.Publish(new EEffectsApplied(new System.Collections.Generic.List<EffectData> { removeEffect }));
                    },
                    duration));
            }
        }

        /// <summary>
        /// Directly applies burn build-up to the player.
        /// Value = amount of burn build-up to add.
        /// Duration = if > 0, temporary exposure.
        /// </summary>
        private void ApplyBurned(float value, float duration)
        {
            var buildUp = (int)value;
            if (duration <= 0f)
            {
                var damage = new Data.Damage.DamageOutput(0, buildUp, 0, 0, 0);
                _eventBus.Publish(new EPlayerReceiveDamage(damage));
            }
            else
            {
                CoroutineRunner.Instance.StartCoroutine(TemporaryBuff(
                    () =>
                    {
                        var damage = new Data.Damage.DamageOutput(0, buildUp, 0, 0, 0);
                        _eventBus.Publish(new EPlayerReceiveDamage(damage));
                    },
                    () =>
                    {
                        var removeEffect = new EffectData { Type = EffectType.Burned, Value = -value, ValueType = EffectValueType.Static };
                        _eventBus.Publish(new EEffectsApplied(new System.Collections.Generic.List<EffectData> { removeEffect }));
                    },
                    duration));
            }
        }

        /// <summary>
        /// Directly applies frost build-up to the player.
        /// Value = amount of frost build-up to add.
        /// Duration = if > 0, temporary exposure.
        /// </summary>
        private void ApplyFrostbitten(float value, float duration)
        {
            var buildUp = (int)value;
            if (duration <= 0f)
            {
                var damage = new Data.Damage.DamageOutput(0, 0, buildUp, 0, 0);
                _eventBus.Publish(new EPlayerReceiveDamage(damage));
            }
            else
            {
                CoroutineRunner.Instance.StartCoroutine(TemporaryBuff(
                    () =>
                    {
                        var damage = new Data.Damage.DamageOutput(0, 0, buildUp, 0, 0);
                        _eventBus.Publish(new EPlayerReceiveDamage(damage));
                    },
                    () =>
                    {
                        var removeEffect = new EffectData { Type = EffectType.Frostbitten, Value = -value, ValueType = EffectValueType.Static };
                        _eventBus.Publish(new EEffectsApplied(new System.Collections.Generic.List<EffectData> { removeEffect }));
                    },
                    duration));
            }
        }

        /// <summary>
        /// Changes player energy. Positive = recovery, negative = consumption.
        /// Reuses the same logic as ChangeFocus but can accept negative values.
        /// </summary>
        private void ChangeEnergy(float value, EffectValueType valueType)
        {
            ChangeFocus(value, valueType);
        }

        #region Utilities
        private static IEnumerator TemporaryBuff(System.Action apply, System.Action remove, float duration)
        {
            apply();
            yield return new WaitForSeconds(duration);
            remove();
        }
        
        private static IEnumerator RepeatedBuff(System.Action apply, System.Action remove, float duration, float interval = 1f)
        {
            var elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                apply();
                yield return new WaitForSeconds(interval);
                elapsedTime += interval;
            }
            
            remove();
        }
        #endregion
    }
}