using Boundary.GamePlay.Enemy.Base;

namespace Boundary.Enemy.StateMachine.ConcreteStates
{
    public class EnemyCooldownState : EnemyState
    {

        public EnemyCooldownState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void EnterState()
        {
            base.EnterState();

            Enemy.EnemyCooldownBaseInstance.DoEnterLogic();
        }

        public override void ExitState()
        {
            base.ExitState();

            Enemy.EnemyCooldownBaseInstance.DoExitLogic();
        }

        public override void FrameUpdate()
        {
            base.FrameUpdate();

            Enemy.EnemyCooldownBaseInstance.DoFrameUpdateLogic();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            Enemy.EnemyCooldownBaseInstance.DoPhysicsLogic();
        }
        
        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();

            Enemy.EnemyCooldownBaseInstance.OnReceiveDamage();
        }
    }
}
