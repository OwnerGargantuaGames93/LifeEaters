using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Enemy.Behaviors.Chase;
using UnityEngine;

namespace Boundary.Enemy.Behaviors.Chase.Shooting
{
    [CreateAssetMenu(fileName = "Chase Horizontally Tracking", menuName = "Enemies/Behaviors/Chase/Enemy Chase Horizontally Tracking")]
    public class EnemyChaseHorizontalTrackingSO: EnemyChaseSOBase
    {
        [SerializeField] private float horizontalChaseRange = 2f;
        [SerializeField] private float chaseSpeed = 1.5f;
    
        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            
            if (Enemy.Animator)
            {
                Enemy.Animator.Play("walk");
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
            
            TurnToPlayer();
            
            // Check if the player is within the horizontal range of the enemy
            if (Enemy.Rb.position.x < Enemy.Player.transform.position.x - horizontalChaseRange || Enemy.Rb.position.x > Enemy.Player.transform.position.x + horizontalChaseRange)
            {
                return; // Player is out of horizontal range, do not shoot
            }

            DropBullet();
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();
            
            // Cliff Detection
            if (Enemy.TouchingDirections.IsGrounded)
            {
                if (Enemy.cliffDetection.DetectedColliders.Count == 0 && Enemy.cliffDetection.readyForFixedUpdate)
                {
                    return;
                }
            }
            
            var directionToPlayer = Enemy.Player.transform.position - Transform.position;
            var horizontalDirection = new Vector2(directionToPlayer.x, 0).normalized;

            Enemy.Rb.linearVelocity = new Vector2(horizontalDirection.x * chaseSpeed, Enemy.Rb.linearVelocity.y);
        }

        protected override void ResetValues()
        {
            base.ResetValues();
        }

        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();
        }

        private void TurnToPlayer() 
        {
            if (Enemy.Player is not null)
            {
                var directionToPlayer = Enemy.Player.transform.position - Transform.position;

                Enemy.WalkDirection = directionToPlayer.x switch
                {
                    > 0 => BaseEnemy.WalkDirectionEnum.Right,
                    < 0 => BaseEnemy.WalkDirectionEnum.Left,
                    _ => Enemy.WalkDirection
                };
            }
        }
            
        private void DropBullet()
        {
            Enemy.StateMachine.ChangeState(Enemy.RangedAttackState);
        }
    }
}