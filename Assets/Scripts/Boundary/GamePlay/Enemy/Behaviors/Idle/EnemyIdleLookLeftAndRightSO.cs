using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Idle
{
    [CreateAssetMenu(fileName = "Idle Look Left And Right", menuName = "Enemies/Behaviors/Idle/Enemy Idle Look Left And Right")]
    public class EnemyIdleLookLeftAndRightSO : EnemyIdleSOBase
    {
        [SerializeField] private float turnInterval = 2f;

        private float _turnTimer = 0f;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            
            if (Enemy.Animator)
            {
                Enemy.Animator.Play("idle");
            }
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();

            _turnTimer += Time.deltaTime;

            if (!(_turnTimer >= turnInterval))
            {
                return;
            }
            
            Enemy.Turn();
            _turnTimer = 0f;
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();
        }
    }
}