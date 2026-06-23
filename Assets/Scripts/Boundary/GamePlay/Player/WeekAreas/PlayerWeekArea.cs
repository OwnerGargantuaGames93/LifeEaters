using Boundary.GamePlay.Enemy.Base;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.Player.WeekAreas
{
    public class PlayerWeekArea: MonoBehaviour
    {
        private IEventBus _eventBus;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.EnemyAttackAreaTag))
            {
                var enemy = other.GetComponentInParent<BaseEnemy>();

                if (!enemy)
                {
                    Debug.LogWarning("[PlayerWeekArea] EnemyAttackAreaTag found but no BaseEnemy component in parent.");
                    return;
                }
                
                enemy.DealContactDamage();
            }
        }
    }
}