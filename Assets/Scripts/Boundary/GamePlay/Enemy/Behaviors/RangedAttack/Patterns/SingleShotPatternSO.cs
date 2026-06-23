using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Projectile;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.RangedAttack.Patterns
{
    /// <summary>
    /// Fires a single projectile aimed at the player.
    /// </summary>
    [CreateAssetMenu(fileName = "SingleShot",
        menuName = "Enemies/Behaviors/Ranged Attack Patterns/Single Shot")]
    public class SingleShotPatternSO : RangedAttackPatternSOBase
    {
        public override void Execute(BaseEnemy enemy, ProjectilePool pool)
        {
            if (ProjectileData == null || pool == null) return;

            pool.Get(ProjectileData, GetSpawnPosition(enemy), DirectionToPlayer(enemy));
        }
    }
}

