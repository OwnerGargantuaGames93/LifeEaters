using Boundary.GamePlay.Enemy.Base;

namespace Boundary.Enemy.StateMachine.ConcreteStates
{
    public class EnemyRangedAttackState : EnemyState
    {

        public EnemyRangedAttackState(BaseEnemy enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
        {
        }

        public override void EnterState()
        {
            base.EnterState();

            Enemy.EnemyRangedAttackBaseInstance.DoEnterLogic();
        }

        public override void ExitState()
        {
            base.ExitState();

            Enemy.EnemyRangedAttackBaseInstance.DoExitLogic();
        }

        public override void FrameUpdate()
        {
            base.FrameUpdate();

            Enemy.EnemyRangedAttackBaseInstance.DoFrameUpdateLogic();
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();

            Enemy.EnemyRangedAttackBaseInstance.DoPhysicsLogic();
        }
        
        public override void OnReceiveDamage()
        {
            base.OnReceiveDamage();

            Enemy.EnemyRangedAttackBaseInstance.OnReceiveDamage();
        }
    }
}
