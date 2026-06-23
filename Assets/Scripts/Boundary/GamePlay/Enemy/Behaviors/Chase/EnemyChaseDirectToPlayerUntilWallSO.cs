using Data.Entities.Enemy;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Chase
{
    [CreateAssetMenu(fileName = "Enemy Chase Direct To Player Until Wall", menuName = "Enemies/Behaviors/Chase/Enemy Chase Direct To Player Until Wall")]
    public class EnemyChaseDirectToPlayerUntilWallSO : EnemyChaseSOBase
    {
        [SerializeField] private float phase1ChaseSpeed = 16f;
        [SerializeField] private float phase2ChaseSpeed = 20f;
        [SerializeField] private float stunnedDuration = 1.0f;
        [SerializeField] private bool stunOnAttack = true;
        
        private float _stunnedTimer;
        
        protected ChaseStatus _chaseStatus = ChaseStatus.ReadyToChase;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            
            _chaseStatus = ChaseStatus.Chasing;

            _stunnedTimer = stunnedDuration;
            
            if (Enemy.Animator)
            {
                Enemy.Animator.Play("walk");
            }
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
            
            _chaseStatus = ChaseStatus.ReadyToChase;
            OnStunStateExited();

            _stunnedTimer = stunnedDuration;
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();
            
            switch (_chaseStatus)
            {
                case ChaseStatus.Chasing:
                    // Check for wall collision
                    if (Enemy.TouchingDirections.IsOnWall)
                    {
                        _chaseStatus = ChaseStatus.Stunned;
                        OnStunStateEnter();
                        if (Enemy.Animator)
                        {
                            // Add stunned animation
                            Enemy.Animator.Play("idle");
                        }
                    }
                    break;
                case ChaseStatus.Stunned:
                    _stunnedTimer -= Time.deltaTime;
                    if (_stunnedTimer <= 0f)
                    {
                        Enemy.StateMachine.ChangeState(Enemy.IdleState);
                    }
                    return;
                case ChaseStatus.ReadyToChase:
                case ChaseStatus.LostChase:
                    break;
                default:
                    Debug.LogError("[EnemyChaseDirectToPlayerUntilWallSO] Invalid chase status");
                    break;
            }
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();

            // Se non è in fase di knockback
            switch (_chaseStatus)
            {
                case ChaseStatus.Stunned:
                    Enemy.Rb.linearVelocity = Vector2.zero;
                    return;
                case ChaseStatus.Chasing:
                    var chaseSpeed = Enemy.CurrentPhase() == EnemyPhase.Phase1 ? phase1ChaseSpeed : phase2ChaseSpeed;
                    
                    Enemy.Rb.linearVelocity = new Vector2(chaseSpeed * Enemy.walkDirectionVector.x, Enemy.Rb.linearVelocity.y);
                    break;
                case ChaseStatus.ReadyToChase:
                case ChaseStatus.LostChase:
                    break;
                default:
                    Debug.LogError("[EnemyChaseDirectToPlayerUntilWallSO] Invalid chase status");
                    return;
            }
        }

        protected override void ResetValues()
        {
            base.ResetValues();
        }

        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();

            if (stunOnAttack)
            {
                _chaseStatus = ChaseStatus.Stunned;
            }
        }
        
        protected virtual void OnStunStateEnter()
        {
        }
        
        protected virtual void OnStunStateExited()
        {
        }
    }

    public enum ChaseStatus
    {
        Chasing,
        LostChase,
        Stunned,
        ReadyToChase
    }
}
