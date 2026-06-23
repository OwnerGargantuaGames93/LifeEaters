using Boundary.GamePlay.Enemy.Behaviors.Idle;
using UnityEngine;

namespace Boundary.Enemy.Behaviors.Idle
{
    [CreateAssetMenu(fileName = "Idle Stay Still", menuName = "Enemies/Behaviors/Idle/Enemy Idle Stay Still")]
    public class EnemyIdleStayStill: EnemyIdleSOBase
    {
        public override void DoEnterLogic()
        {
            base.DoEnterLogic();

            if (Enemy.Animator)
            {
                Enemy.Animator.Play("idle");
            }
        }
    }
}