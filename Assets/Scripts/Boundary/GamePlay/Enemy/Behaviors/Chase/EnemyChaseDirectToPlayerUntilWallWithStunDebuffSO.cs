using Boundary.GamePlay.Enemy.Behaviors.Chase;
using UnityEngine;

namespace Boundary.Enemy.Behaviors.Chase
{
    [CreateAssetMenu(fileName = "Enemy Chase Direct To Player Until Wall (Stunned Debuff)", menuName = "Enemies/Behaviors/Chase/Enemy Chase Direct To Player Until Wall (Stunned Debuff)")]
    public class EnemyChaseDirectToPlayerUntilWallWithStunDebuffSO : EnemyChaseDirectToPlayerUntilWallSO
    {
        [SerializeField] private int jumpAttackDefenseValueAfterStunned = 1;
        [SerializeField] private int throwAttackDefenseValueAfterStunned = 0;
        
        protected override void OnStunStateEnter()
        {
            base.OnStunStateEnter();
            
            if (jumpAttackDefenseValueAfterStunned > 0)
            {
                Enemy.ApplyJumpAttackDefenseDebuff(jumpAttackDefenseValueAfterStunned);
            }
            
            if (throwAttackDefenseValueAfterStunned > 0)
            {
                Enemy.ApplyThrowAttackDefenseDebuff(throwAttackDefenseValueAfterStunned);
            }
        }
        
        protected override void OnStunStateExited()
        {
            base.OnStunStateExited();
            
            if (jumpAttackDefenseValueAfterStunned > 0)
            {
                Enemy.RemoveJumpAttackDefenseDebuff(jumpAttackDefenseValueAfterStunned);
            }

            if (throwAttackDefenseValueAfterStunned > 0)
            {
                Enemy.RemoveThrowAttackDefenseDebuff(throwAttackDefenseValueAfterStunned);
            }
        }
    }
}