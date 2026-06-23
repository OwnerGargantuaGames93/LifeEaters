using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Enemy.Behaviors.Idle;
using UnityEngine;

namespace Boundary.Enemy.Behaviors.Idle
{
    [CreateAssetMenu(fileName = "Popjue Bear Idle State", menuName = "Enemies/Behaviors/Idle/Popjue Bear Idle State")]
    public class PopjueBearIdleStateSO : EnemyIdleLookLeftAndRightSO
    {
        [SerializeField] private float goToSleepAfterSeconds = 10f;
        private float _sleepTimer;

        public override void DoEnterLogic()
        {
            base.DoEnterLogic();
            _sleepTimer = 0f;
        }

        public override void DoExitLogic()
        {
            base.DoExitLogic();
            _sleepTimer = 0f;
        }

        public override void DoFrameUpdateLogic()
        {
            base.DoFrameUpdateLogic();
            
            _sleepTimer += Time.deltaTime;

            if (!(_sleepTimer >= goToSleepAfterSeconds))
            {
                return;
            }
            
            Enemy.StateMachine.ChangeState(Enemy.DormantState);
        }

        public override void DoPhysicsLogic()
        {
            base.DoPhysicsLogic();
        }
    }
}