using Boundary.GamePlay.Enemy.Behaviors.Dormant;
using UnityEngine;

namespace Boundary.Enemy.Behaviors.Dormant
{
    [CreateAssetMenu(fileName = "Dormant Stay Still", menuName = "Enemies/Behaviors/Dormant/Enemy Dormant Stay Still")]
    public class EnemyDormantStayStillSO: EnemyDormantSOBase
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