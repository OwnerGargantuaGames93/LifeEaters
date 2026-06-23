using Control.Player;
using Data.Damage;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Control.Damage
{
    public class PlayerConditionHandler
    {
        // Service dependencies
        private readonly IEventBus _eventBus;
        
        public PlayerConditionHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;

            SubscribeToEvents();
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EPlayerPoisoned>(OnPlayerPoisoned);
        }
        
        #region Event Handlers   
        private void OnPlayerPoisoned(EPlayerPoisoned e)
        {
            CoroutineRunner.Instance.StartCoroutine(ApplyPoisonDamageOverTime(e.Duration, e.DamagePerTick, e.TickInterval));
        }
        
        private void OnPlayerBurnt(EPlayerBurnt e)
        {
            CoroutineRunner.Instance.StartCoroutine(ApplyBurnt(e.Duration));
        }
        #endregion
        
        #region Coroutines
        private System.Collections.IEnumerator ApplyPoisonDamageOverTime(float duration, int damagePerTick, float tickInterval)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                // Apply damage to player
                var damage = new DamageOutput(
                    healthDamage: damagePerTick,
                    burntBuildUp: 0,
                    frostBuildUp: 0,
                    poisonBuildUp: 0,
                    directLifeSteal: 0
                );
                
                _eventBus.Publish(new EPlayerReceiveDamage(damage));
                
                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }
            
            _eventBus.Publish(new EPoisonEffectFinished());
        }
        
        private System.Collections.IEnumerator ApplyBurnt(float duration)
        {
            yield return new WaitForSeconds(duration);
            
            // Revert fire resistance or remove visual effect
            _eventBus.Publish(new EBurnEffectFinished());
        }
        #endregion
    }
}