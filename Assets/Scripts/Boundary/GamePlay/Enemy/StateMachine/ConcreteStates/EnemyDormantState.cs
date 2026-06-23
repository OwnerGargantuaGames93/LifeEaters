using Boundary.Enemy.StateMachine;
using Boundary.GamePlay.Enemy.Base;

namespace Boundary.GamePlay.Enemy.StateMachine.ConcreteStates
{
    public class EnemyDormantState: EnemyState
    {
        public EnemyDormantState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void EnterState()
        {
            base.EnterState();

            Enemy.EnemyDormantBaseInstance.DoEnterLogic();
        }

        public override void ExitState()
        {
            base.ExitState();

            Enemy.EnemyDormantBaseInstance.DoExitLogic();
        }

        public override void FrameUpdate()
        {
            base.FrameUpdate();

            Enemy.EnemyDormantBaseInstance.DoFrameUpdateLogic();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            Enemy.EnemyDormantBaseInstance.DoPhysicsLogic();
        }
        
        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();

            Enemy.EnemyDormantBaseInstance.OnReceiveDamage();
        }
    }
}