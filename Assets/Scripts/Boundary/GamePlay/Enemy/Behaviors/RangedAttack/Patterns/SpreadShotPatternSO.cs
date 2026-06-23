using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Projectile;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.RangedAttack.Patterns
{
    /// <summary>
    /// Fires multiple projectiles simultaneously in a fan/spread centred on the player direction.
    /// Example: count=3, spreadAngle=60° → fires at -30°, 0°, +30° relative to player direction.
    /// </summary>
    [CreateAssetMenu(fileName = "SpreadShot",
        menuName = "Enemies/Behaviors/Ranged Attack Patterns/Spread Shot")]
    public class SpreadShotPatternSO : RangedAttackPatternSOBase
    {
        [Tooltip("Total number of projectiles fired simultaneously.")]
        [Min(1)]
        public int ProjectileCount = 3;

        [Tooltip("Total arc angle in degrees spread across all projectiles.")]
        [Range(0f, 360f)]
        public float SpreadAngle = 60f;

        public override void Execute(BaseEnemy enemy, ProjectilePool pool)
        {
            if (ProjectileData == null || pool == null) return;

            Vector2 baseDir   = DirectionToPlayer(enemy);
            Vector2 spawnPos  = GetSpawnPosition(enemy);

            if (ProjectileCount == 1)
            {
                pool.Get(ProjectileData, spawnPos, baseDir);
                return;
            }

            float step      = SpreadAngle / (ProjectileCount - 1);
            float startAngle = -SpreadAngle / 2f;

            for (int i = 0; i < ProjectileCount; i++)
            {
                float  angle = startAngle + step * i;
                Vector2 dir  = Rotate(baseDir, angle);
                pool.Get(ProjectileData, spawnPos, dir);
            }
        }
    }
}

