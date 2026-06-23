using Boundary.GamePlay.Enemy.Base;

namespace Boundary.Enemy.StateMachine
{
    public class EnemyState
    {
        protected readonly BaseEnemy Enemy;
        protected EnemyStateMachine StateMachine;

        protected EnemyState(BaseEnemy enemy, EnemyStateMachine stateMachine)
        {
            Enemy = enemy;
            StateMachine = stateMachine;
        }

        public virtual void EnterState()
        {
        }

        public virtual void ExitState()
        {
        }

        public virtual void FrameUpdate()
        {
        }

        public virtual void PhysicsUpdate()
        {
        }
        
        public virtual void OnReceiveDamage()
        {
        }
    }
}