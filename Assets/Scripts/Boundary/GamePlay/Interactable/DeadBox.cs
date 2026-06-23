using Boundary.GamePlay.Enemy.Base;
using Infra.EventBus;
using UnityEngine;
using Utils;

namespace Boundary.GamePlay.Interactable
{
    public class DeadBox : MonoBehaviour
    {
        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(Constants.PlayerTag))
            {
                _eventBus.Publish(new EPlayerFallInDeadBox());
            } else if (other.gameObject.layer == Constants.GrabbableGroundLayerNumber)
            {
                Destroy(other.gameObject);
            } else if (other.gameObject.layer == Constants.EnemyLayerNumber)
            {
                var enemy = other.gameObject.GetComponent<BaseEnemy>();
                if (enemy != null)
                {
                    _eventBus.Publish(new EEnemyFallInDeadBox(enemy.Id));
                }
            }
        }
    }
    
    #region Events

    public struct EPlayerFallInDeadBox
    {
    }
        
    public struct EEnemyFallInDeadBox
    {
        public readonly string EnemyId;
            
        public EEnemyFallInDeadBox(string enemyId)
        {
            EnemyId = enemyId;
        }
    }

    #endregion
}
