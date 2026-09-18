using Boundary.GamePlay.Enemy.Base;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Idle
{
    public class EnemyIdleSOBase : ScriptableObject
    {
        protected BaseEnemy Enemy;
        protected Transform Transform;
        protected GameObject GameObject;
        
        [SerializeField] private bool disableAggroCheck;
        [SerializeField] private bool disableRangedAttackRangeCheck;

        public void Initialize(GameObject gameObject, BaseEnemy enemy) {
            Enemy = enemy;
            Transform = gameObject.transform;
            GameObject = gameObject;
        }

        public virtual void DoEnterLogic() {
            if (Enemy.IdleAnimationName != null)
            {
                Enemy.Animator.Play(Enemy.IdleAnimationName);    
            }
        }

        public virtual void DoExitLogic() {
            ResetValues();
        }

        public virtual void DoFrameUpdateLogic() {
            if (Enemy.IsAggroed && !disableAggroCheck){
                Enemy.StateMachine.ChangeState(Enemy.ChasingState);
            }
            
            if (Enemy.InRangedAttackRange && !disableRangedAttackRangeCheck)
            {
                Enemy.StateMachine.ChangeState(Enemy.RangedAttackState);
            }
        }

        public virtual void DoPhysicsLogic() {

        }

        public virtual void OnReceiveDamage()
        {
            
        }

        private static void ResetValues() {

        }


    }
}
