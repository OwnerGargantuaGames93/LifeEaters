using Boundary.GamePlay.Enemy.Base;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Chase
{
    public class EnemyChaseSOBase : ScriptableObject
    {
        protected BaseEnemy Enemy;
        protected Transform Transform;
        protected GameObject GameObject;
        
        [SerializeField] private bool disableRangedAttackRangeCheck;
        [SerializeField] private bool controlContactAttackAreaActivation;

        public void Initialize(GameObject gameObject, BaseEnemy enemy) {
            Enemy = enemy;
            Transform = gameObject.transform;
            GameObject = gameObject;
        }

        public virtual void DoEnterLogic() {
            if (controlContactAttackAreaActivation && Enemy.ContactAttackArea != null)
            {
                Enemy.ContactAttackArea.SetActive(true);
            }
        }

        public virtual void DoExitLogic() {
            ResetValues();
            
            // Stop immediately the enemy when exiting the chase, preventing it from sliding into the next state
            if (!Enemy.isKnockedBack)
            {
                Enemy.Rb.linearVelocity = new Vector2(0f, Enemy.Rb.linearVelocity.y);
            }

            if (controlContactAttackAreaActivation && Enemy.ContactAttackArea != null)
            {
                Enemy.ContactAttackArea.SetActive(false);
            }
        }

        public virtual void DoFrameUpdateLogic()
        {
            if (Enemy.InRangedAttackRange && !disableRangedAttackRangeCheck)
            {
                Enemy.StateMachine.ChangeState(Enemy.RangedAttackState);
            }
        }

        public virtual void DoPhysicsLogic() {
        }
        

        protected virtual void ResetValues() {
        }
        
        public virtual void OnReceiveDamage()
        {
        }
    }
}
