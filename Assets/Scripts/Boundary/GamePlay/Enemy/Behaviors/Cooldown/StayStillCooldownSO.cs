using Data.Entities.Enemy;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.MeleeAttack
{
    [CreateAssetMenu(fileName = "Stay Still Cooldown", menuName = "Enemies/Behaviors/Cooldown/Stay Still Cooldown")]
    public class StayStillCooldownSO: EnemyCooldownSOBase
    {
        [SerializeField] private float cooldownDuration = 2f;
        [SerializeField] private float cooldownDurationPhase2 = 1.5f;
        [SerializeField] private string animationName;

        private float _cooldownTimer;
        
        public override void DoEnterLogic()
        {
            base.DoEnterLogic();

            if (Enemy.Animator && !string.IsNullOrEmpty(animationName))
            {
                Enemy.Animator.Play(animationName);    
            }
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
            ResetValues();
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();
            
            var duration = Enemy.CurrentPhase() switch
            {
                EnemyPhase.Phase1 => this.cooldownDuration,
                EnemyPhase.Phase2 => cooldownDurationPhase2,
                _ => throw new System.ArgumentOutOfRangeException("Invalid enemy phase for Stay Still Cooldown State: " +
                                                                   Enemy.CurrentPhase())
            };
            
            if (_cooldownTimer >= duration)
            {
                Enemy.StateMachine.ChangeState(Enemy.IdleState);
            }
            else
            {
                _cooldownTimer += Time.deltaTime;
            }
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();
            
            if (Enemy.Rb.linearVelocity != Vector2.zero)
            {
                Enemy.Rb.linearVelocity = Vector2.zero;    
            }
        }


        private void ResetValues()
        {
            _cooldownTimer = 0f;
        }
    }
}