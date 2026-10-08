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
        
        [SerializeField] private string walkAnimationName = "walk";
        [SerializeField] private string idleAnimationName = "idle";
        
        [SerializeField] private float movementSpeed = 0.5f;
        [SerializeField] private float minTurnDelay = 0.5f;
        [SerializeField] private float maxTurnDelay = 2f;
        [SerializeField] private float minStopBeforeTurnDelay = 0.5f;
        [SerializeField] private float maxStopBeforeTurnDelay = 2f;

        [Header("Hop Before Chase")]
        [Tooltip("If enabled, when the player enters aggro the enemy does a small hop before switching to chase")]
        [SerializeField] private bool hopBeforeChase;
        [Tooltip("Upward velocity applied for the hop")]
        [SerializeField, Min(0f)] private float hopIntensity = 4f;
        [SerializeField] private string hopAnimationName = "";
        [Tooltip("Safety timeout: switches to chase even if the enemy hasn't landed yet")]
        [SerializeField, Min(0f)] private float maxHopDuration = 1f;
        
        private PatrolState _patrolState;
        private float _stateTimer;
        private bool _isHopping;
        private bool _hasLeftGround;
        private float _hopTimer;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            _patrolState = PatrolState.Moving;
            _stateTimer = Random.Range(minTurnDelay, maxTurnDelay);
            ResetHop();
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
            _patrolState = PatrolState.Moving;
            Enemy.Rb.linearVelocity = Vector2.zero;
            ResetHop();
        }

        public override void DoFrameUpdateLogic()
        {
            // Saltino in corso: aspetta di atterrare (o il timeout), poi parte il chase
            if (_isHopping)
            {
                UpdateHop();
                return;
            }

            if (hopBeforeChase && !DisableAggroCheck && Enemy.IsAggroed && Enemy.TouchingDirections.IsGrounded)
            {
                StartHop();
                return;
            }

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
                        UpdateAnimation();
                        break;
                    }
                    case PatrolState.Stopping:
                    {
                        Enemy.Turn();
                        _patrolState = PatrolState.Moving;
                        _stateTimer = Random.Range(minTurnDelay, maxTurnDelay);
                        UpdateAnimation();
                        break;
                    }
                }
            }
        }

        private void UpdateAnimation()
        {
            switch (_patrolState)
            {
                case PatrolState.Moving:
                    Enemy.Animator.Play(walkAnimationName);
                    break;
                case PatrolState.Stopping:
                    Enemy.Animator.Play(idleAnimationName);
                    break;
            }
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();

            if (Enemy.isKnockedBack)
            {
                return;
            }

            if (_isHopping)
            {
                Enemy.Rb.linearVelocity = new Vector2(0f, Enemy.Rb.linearVelocity.y);
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
        
        private void StartHop()
        {
            _isHopping = true;
            _hasLeftGround = false;
            _hopTimer = 0f;

            Enemy.Rb.linearVelocity = new Vector2(0f, hopIntensity);

            if (!string.IsNullOrEmpty(hopAnimationName))
            {
                Enemy.Animator.Play(hopAnimationName);
            }
        }

        private void UpdateHop()
        {
            _hopTimer += Time.deltaTime;

            var isGrounded = Enemy.TouchingDirections.IsGrounded;
            if (!isGrounded)
            {
                _hasLeftGround = true;
            }

            var hasLanded = _hasLeftGround && isGrounded;
            if (hasLanded || _hopTimer >= maxHopDuration)
            {
                Enemy.StateMachine.ChangeState(Enemy.ChasingState);
            }
        }

        private void ResetHop()
        {
            _isHopping = false;
            _hasLeftGround = false;
            _hopTimer = 0f;
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