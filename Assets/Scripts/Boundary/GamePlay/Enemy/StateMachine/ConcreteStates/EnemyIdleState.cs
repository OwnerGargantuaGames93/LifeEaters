using Boundary.GamePlay.Enemy.Base;

namespace Boundary.Enemy.StateMachine.ConcreteStates
{
    public class EnemyIdleState : EnemyState
    {

        public EnemyIdleState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void EnterState()
        {
            base.EnterState();

            Enemy.EnemyIdleBaseInstance.DoEnterLogic();
        }

        public override void ExitState()
        {
            base.ExitState();

            Enemy.EnemyIdleBaseInstance.DoExitLogic();
        }

        public override void FrameUpdate()
        {
            base.FrameUpdate();

            Enemy.EnemyIdleBaseInstance.DoFrameUpdateLogic();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            Enemy.EnemyIdleBaseInstance.DoPhysicsLogic();
        }
        
        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();

            Enemy.EnemyIdleBaseInstance.OnReceiveDamage();
        }
    }
}
