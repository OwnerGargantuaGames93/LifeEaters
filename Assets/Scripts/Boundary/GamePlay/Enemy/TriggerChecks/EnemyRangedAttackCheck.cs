using Boundary.GamePlay.Enemy.Base;
using UnityEngine;
using Utils;

namespace Boundary.GamePlay.Enemy.TriggerChecks
{
    public class EnemyRangedAttackCheck: MonoBehaviour
    {
        [SerializeField] private bool triggerActive;
        [SerializeField] private bool considerObstacles = false;
        
        private GameObject _player;
        private BaseEnemy _enemy;
        
        private void Awake()
        {
            _enemy = GetComponentInParent<BaseEnemy>();
            _player = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
        }
        
        private void Update()
        {
            _enemy.SetInRangedAttackRange(triggerActive && EnemyCheckUtils.EnemyIsWatchingPlayer(_enemy, _player, considerObstacles));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                triggerActive = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag(Constants.PlayerTag))
            {
                triggerActive = false;
            }
        }
    }
}