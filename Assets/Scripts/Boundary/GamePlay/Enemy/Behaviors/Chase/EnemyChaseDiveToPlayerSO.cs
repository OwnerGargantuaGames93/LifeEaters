using System;
using Boundary.GamePlay.Enemy.Behaviors.MeleeAttack;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Chase
{
    [CreateAssetMenu(fileName = "Melee Attack Dive To Player", menuName = "Enemies/Behaviors/Melee Attack/Enemy Dive To Player")]
    public class EnemyChaseDiveToPlayerSO: EnemyChaseSOBase
    {
        private const string StartStopChaseAnimationName = "start_stop_chase";
        private const string DiveAnimationName = "chase";
        private const string IdleAnimationName = "idle";
        
        private enum DiveState
        {
            Preparing,
            Diving,
            WaitingAfterDive,
            Finished
        }
        
        [SerializeField] private float diveSpeed = 10f;
        [SerializeField] private float diveUpwardForce = 15f;
        [SerializeField] private float divePreparationTime = 0.5f;
        [SerializeField] private float waitAfterDive = 4f;

        private DiveState _diveState;
        private float _stateTimer;
        private Vector2 _diveVelocity;
        private bool _hasAppliedImpulse;

        private const float GroundCheckDelay = 0.2f;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();

            _diveState = DiveState.Preparing;
            _stateTimer = divePreparationTime;
            
            Enemy.Animator.Play(StartStopChaseAnimationName);
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
            ResetValues();
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();
            
            if (_diveState == DiveState.Finished)
            {
                Enemy.StateMachine.ChangeState(Enemy.IdleState);
                return;
            }
            
            if (Enemy.isKnockedBack)
            {
                return;
            }
            
            _stateTimer -= Time.deltaTime;

            switch (_diveState)
            {
                case DiveState.Preparing:
                {
                    if (_stateTimer > 0f)
                    {
                        return;
                    }
                    
                    var directionToPlayer = (Enemy.Player.transform.position - Transform.position).normalized;
                    _diveVelocity = new Vector2(directionToPlayer.x * diveSpeed, diveUpwardForce);
                    _diveState = DiveState.Diving;
                    _hasAppliedImpulse = true;
                    _stateTimer = GroundCheckDelay;
                    
                    Enemy.Animator.Play(DiveAnimationName);
                    
                    break;
                }
                case DiveState.Diving:
                {
                    if (Enemy.TouchingDirections.IsGrounded && _stateTimer <= 0f)
                    {
                        _diveState = DiveState.WaitingAfterDive;
                        _stateTimer = waitAfterDive;
                        Enemy.Rb.linearVelocity = Vector2.zero;
                        Enemy.Animator.Play(IdleAnimationName);
                    }
                    break;
                }
                case DiveState.WaitingAfterDive:
                {
                    if (_stateTimer <= 0f)
                    {
                        _diveState = DiveState.Finished;
                    }
                    break;
                }
                case DiveState.Finished:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();
            
            if (Enemy.isKnockedBack) return;

            if (_diveState == DiveState.Diving && _hasAppliedImpulse)
            {
                Enemy.Rb.AddForce(_diveVelocity, ForceMode2D.Impulse);
                _hasAppliedImpulse = false;
            }
        }
        
        private void ResetValues()
        {
            _diveState = DiveState.Preparing;
            _stateTimer = 0f;
            _diveVelocity = Vector2.zero;
            _hasAppliedImpulse = false;
        }
    }
    
    
}