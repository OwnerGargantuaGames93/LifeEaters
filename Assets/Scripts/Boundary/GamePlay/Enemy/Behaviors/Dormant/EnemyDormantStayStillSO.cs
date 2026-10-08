using Boundary.GamePlay.Enemy.Behaviors.Dormant;
using UnityEngine;

namespace Boundary.Enemy.Behaviors.Dormant
{
    [CreateAssetMenu(fileName = "Dormant Stay Still", menuName = "Enemies/Behaviors/Dormant/Enemy Dormant Stay Still")]
    public class EnemyDormantStayStillSO: EnemyDormantSOBase
    {
        [SerializeField] private string dormantAnimationName = "idle";
        
        public override void DoEnterLogic()
        {
            base.DoEnterLogic();

            if (Enemy.Animator && !string.IsNullOrEmpty(dormantAnimationName))
            {
                Enemy.Animator.Play(dormantAnimationName);
            }
        }
        
    }
}