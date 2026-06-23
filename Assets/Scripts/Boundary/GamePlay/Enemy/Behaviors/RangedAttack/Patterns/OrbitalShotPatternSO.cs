using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Projectile;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.RangedAttack.Patterns
{
    /// <summary>
    /// Fires N projectiles evenly distributed in a full 360° ring.
    /// Useful for boss "burst ring" attacks.
    /// </summary>
    [CreateAssetMenu(fileName = "OrbitalShot",
        menuName = "Enemies/Behaviors/Ranged Attack Patterns/Orbital Shot")]
    public class OrbitalShotPatternSO : RangedAttackPatternSOBase
    {
        [Tooltip("Number of projectiles distributed evenly around 360°.")]
        [Min(2)]
        public int ProjectileCount = 8;

        [Tooltip("Degrees to rotate the entire ring.  0 = first projectile fires right.")]
        [Range(0f, 360f)]
        public float RotationOffset = 0f;

        public override void Execute(BaseEnemy enemy, ProjectilePool pool)
        {
            if (ProjectileData == null || pool == null) return;

            Vector2 spawnPos = GetSpawnPosition(enemy);
            float   step     = 360f / ProjectileCount;

            for (int i = 0; i < ProjectileCount; i++)
            {
                float   angle = RotationOffset + step * i;
                float   rad   = angle * Mathf.Deg2Rad;
                Vector2 dir   = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                pool.Get(ProjectileData, spawnPos, dir);
            }
        }
    }
}

