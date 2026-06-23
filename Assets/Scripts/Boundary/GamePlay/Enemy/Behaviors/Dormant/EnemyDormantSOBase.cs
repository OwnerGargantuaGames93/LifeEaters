using Boundary.GamePlay.Enemy.Base;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Dormant
{
    public class EnemyDormantSOBase : ScriptableObject
    {
        protected BaseEnemy Enemy;
        protected Transform Transform;
        protected GameObject GameObject;

        public virtual void Initialize(GameObject gameObject, BaseEnemy enemy) {
            Enemy = enemy;
            Transform = gameObject.transform;
            GameObject = gameObject;
        }

        public virtual void DoEnterLogic() {
            
        }

        public virtual void DoExitLogic() {
            ResetValues();
        }

        public virtual void DoFrameUpdateLogic() {
            if (Enemy.IsAwake) {
                Enemy.StateMachine.ChangeState(Enemy.IdleState);
            }
        }

        public virtual void DoPhysicsLogic() {

        }


        public virtual void OnReceiveDamage()
        {
        }

        private void ResetValues()
        {
            
        }


    }
}
