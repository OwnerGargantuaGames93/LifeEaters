using Boundary.Enemy.StateMachine;
using Boundary.GamePlay.Enemy.Base;

namespace Boundary.GamePlay.Enemy.StateMachine.ConcreteStates
{
    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {}

        public override void EnterState()
        {
            base.EnterState();

            Enemy.EnemyChaseBaseInstance.DoEnterLogic();
        }

        public override void ExitState()
        {
            base.ExitState();

            Enemy.EnemyChaseBaseInstance.DoExitLogic();
        }

        public override void FrameUpdate()
        {
            base.FrameUpdate();

            Enemy.EnemyChaseBaseInstance.DoFrameUpdateLogic();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            Enemy.EnemyChaseBaseInstance.DoPhysicsLogic();
        }

        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();

            Enemy.EnemyChaseBaseInstance.OnReceiveDamage();
        }

    }
}
