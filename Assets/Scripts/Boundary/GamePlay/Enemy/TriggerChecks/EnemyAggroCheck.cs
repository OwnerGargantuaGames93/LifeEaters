using System.Collections.Generic;
using Boundary.GamePlay.Enemy.Base;
using UnityEngine;
using Utils;

namespace Boundary.GamePlay.Enemy.TriggerChecks
{
    public class EnemyAggroCheck : MonoBehaviour
    {
        private GameObject PlayerTarget { get; set; }
        private BaseEnemy _enemy;
    
        private bool triggerActive;
        [SerializeField] private bool considerObstacles = true;

        private void Awake()
        {
            _enemy = GetComponentInParent<BaseEnemy>();
        }

        private void Start()
        {
            PlayerTarget = GameObject.FindGameObjectWithTag(Constants.PlayerTag);
        }

        private void Update()
        {
            _enemy.SetAggroStatus(triggerActive && EnemyCheckUtils.EnemyIsWatchingPlayer(_enemy, PlayerTarget, considerObstacles));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(Constants.PlayerTag))
            {
                triggerActive = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag(Constants.PlayerTag))
            {
                triggerActive = false;
            }
        }
    }
}
