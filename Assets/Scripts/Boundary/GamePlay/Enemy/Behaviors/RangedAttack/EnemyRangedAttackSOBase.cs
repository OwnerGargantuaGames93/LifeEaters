using Boundary.GamePlay.Enemy.Base;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.Chase
{
    public class EnemyRangedAttackSOBase : ScriptableObject
    {
        protected BaseEnemy Enemy;
        protected Transform Transform;
        protected GameObject GameObject;

        public void Initialize(GameObject gameObject, BaseEnemy enemy) {
            Enemy = enemy;
            Transform = gameObject.transform;
            GameObject = gameObject;
        }

        public virtual void DoEnterLogic() {

        }

        public virtual void DoExitLogic() {
            ResetValues();
        }

        public virtual void DoFrameUpdateLogic()
        {
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
