using System.Collections;
using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Projectile;
using UnityEngine;
using Utils;

namespace Boundary.GamePlay.Enemy.Behaviors.RangedAttack.Patterns
{
    public enum BurstDirection
    {
        TowardPlayer,   // each projectile aims at the player at fire-time
        Fixed,          // direction locked at the start of the burst
        Alternating     // alternates left/right of the initial direction
    }

    /// <summary>
    /// Fires a sequence of projectiles one after another with a configurable delay.
    /// Requires CoroutineRunner to span across multiple frames.
    /// </summary>
    [CreateAssetMenu(fileName = "BurstShot",
        menuName = "Enemies/Behaviors/Ranged Attack Patterns/Burst Shot")]
    public class BurstShotPatternSO : RangedAttackPatternSOBase
    {
        [Tooltip("Total number of projectiles in the burst.")]
        [Min(1)]
        public int ProjectileCount = 5;

        [Tooltip("Seconds between consecutive projectiles.")]
        [Min(0f)]
        public float DelayBetweenShots = 0.12f;

        [Tooltip("How each projectile in the burst is aimed.")]
        public BurstDirection Direction = BurstDirection.TowardPlayer;

        [Tooltip("Alternating offset angle in degrees (only used when Direction = Alternating).")]
        [Range(0f, 90f)]
        public float AlternatingAngle = 15f;

        public override void Execute(BaseEnemy enemy, ProjectilePool pool)
        {
            if (ProjectileData == null || pool == null) return;

            CoroutineRunner.Instance.StartCoroutine(FireBurst(enemy, pool));
        }

        private IEnumerator FireBurst(BaseEnemy enemy, ProjectilePool pool)
        {
            Vector2 lockedDir = DirectionToPlayer(enemy);

            for (int i = 0; i < ProjectileCount; i++)
            {
                if (enemy == null || pool == null) yield break;

                Vector2 dir = Direction switch
                {
                    BurstDirection.TowardPlayer => DirectionToPlayer(enemy),
                    BurstDirection.Alternating  => Rotate(lockedDir, (i % 2 == 0 ? 1f : -1f) * AlternatingAngle),
                    _                           => lockedDir
                };

                pool.Get(ProjectileData, GetSpawnPosition(enemy), dir);

                yield return new WaitForSeconds(DelayBetweenShots);
            }
        }
    }
}

