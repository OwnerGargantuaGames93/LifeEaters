using Boundary.GamePlay.Enemy.Base;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.MeleeAttack
{
    public class EnemyCooldownSOBase : ScriptableObject
    {
        protected BaseEnemy Enemy;
        protected Transform Transform;
        protected GameObject GameObject;

        public void Initialize(GameObject gameObject, BaseEnemy enemy) {
            Enemy = enemy;
            Transform = gameObject.transform;
            GameObject = gameObject;
        }

        public virtual void DoEnterLogic()
        {
        }

        public virtual void DoExitLogic()
        {
            ResetValues();
        }

        public virtual void DoFrameUpdateLogic()
        {
        }

        public virtual void DoPhysicsLogic()
        {
        }

        public virtual void OnReceiveDamage()
        {
        }

        private static void ResetValues() {

        }


    }
}
