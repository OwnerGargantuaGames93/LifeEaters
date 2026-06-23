using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Idle
{
    [CreateAssetMenu(fileName = "Idle Casual Patrolling", menuName = "Enemies/Behaviors/Idle/Enemy Causal Patrolling")]
    public class EnemyIdleCasualPatrollingSO: EnemyIdleSOBase
    {
        private enum PatrolState
        {
            Moving,
            Stopping
        }
        
        [SerializeField] private float movementSpeed = 0.5f;
        [SerializeField] private float minTurnDelay = 0.5f;
        [SerializeField] private float maxTurnDelay = 2f;
        [SerializeField] private float minStopBeforeTurnDelay = 0.5f;
        [SerializeField] private float maxStopBeforeTurnDelay = 2f;
        
        private PatrolState _patrolState;
        private float _stateTimer;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            _patrolState = PatrolState.Moving;
            _stateTimer = Random.Range(minTurnDelay, maxTurnDelay);
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
            _patrolState = PatrolState.Moving;
            Enemy.Rb.linearVelocity = Vector2.zero;
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();
            
            _stateTimer -= Time.deltaTime;

            if (_stateTimer <= 0f)
            {
                switch (_patrolState)
                {
                    case PatrolState.Moving:
                    {
                        _patrolState = PatrolState.Stopping;
                        _stateTimer = Random.Range(minStopBeforeTurnDelay, maxStopBeforeTurnDelay);
                        Enemy.Rb.linearVelocity = Vector2.zero;
                        break;
                    }
                    case PatrolState.Stopping:
                    {
                        Enemy.Turn();
                        _patrolState = PatrolState.Moving;
                        _stateTimer = Random.Range(minTurnDelay, maxTurnDelay);
                        break;
                    }
                }
            }
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();

            if (Enemy.isKnockedBack)
            {
                return;
            }

            if (!Enemy.TouchingDirections.IsGrounded)
            {
                return;
            }

            Enemy.Rb.linearVelocity = _patrolState switch
            {
                PatrolState.Moving => new Vector2(movementSpeed * Enemy.walkDirectionVector.x,
                    Enemy.Rb.linearVelocity.y),
                PatrolState.Stopping => Vector2.zero,
                _ => Enemy.Rb.linearVelocity
            };
        }
        
        private void ChangePatrolState(PatrolState newState)
        {
            if (_patrolState == newState)
            {
                return;
            }
            
            _patrolState = newState;
            
            // Control animation
            switch (_patrolState)
            {
                case PatrolState.Moving:
                    Enemy.Animator.Play("walk");
                    break;
                case PatrolState.Stopping:
                    Enemy.Animator.Play("idle");
                    break;
            }
        }
    }
}