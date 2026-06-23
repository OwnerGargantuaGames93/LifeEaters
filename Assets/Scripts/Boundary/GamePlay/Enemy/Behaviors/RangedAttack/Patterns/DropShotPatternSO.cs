using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Projectile;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.RangedAttack.Patterns
{
    /// <summary>
    /// Drops a single projectile straight down from the enemy position.
    /// The projectile must have GravityScale > 0 and ProjectileType = Physical in its ProjectileData.
    ///
    /// Use case: enemy that "releases" a heavy object downward, affected by gravity.
    /// </summary>
    [CreateAssetMenu(
        fileName = "DropShotPattern",
        menuName = "Enemies/Behaviors/Ranged Attack Patterns/Drop Shot")]
    public class DropShotPatternSO : RangedAttackPatternSOBase
    {
        [Header("Drop settings")]
        [Tooltip("Optional horizontal nudge applied to the drop direction. " +
                 "Positive = right, Negative = left. 0 = straight down.")]
        [Range(-1f, 1f)]
        public float HorizontalBias = 0f;

        public override void Execute(BaseEnemy enemy, ProjectilePool pool)
        {
            if (ProjectileData == null)
            {
                Debug.LogWarning($"[DropShotPatternSO] ProjectileData is not assigned on {name}.");
                return;
            }

            var direction = new Vector2(HorizontalBias, -1f).normalized;
            pool.Get(ProjectileData, GetSpawnPosition(enemy), direction);
        }
    }
}


