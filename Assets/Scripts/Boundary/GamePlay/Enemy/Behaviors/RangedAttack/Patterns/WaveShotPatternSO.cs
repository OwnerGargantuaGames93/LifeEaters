using System.Collections;
using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Projectile;
using UnityEngine;
using Utils;

namespace Boundary.GamePlay.Enemy.Behaviors.RangedAttack.Patterns
{
    /// <summary>
    /// Fires multiple waves; each wave is a spread of projectiles aimed at the player.
    /// Useful for swarm-like attacks and bullet-curtain patterns.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveShot",
        menuName = "Enemies/Behaviors/Ranged Attack Patterns/Wave Shot")]
    public class WaveShotPatternSO : RangedAttackPatternSOBase
    {
        [Tooltip("Number of waves to fire.")]
        [Min(1)]
        public int WaveCount = 3;

        [Tooltip("Projectiles per wave.")]
        [Min(1)]
        public int ProjectilesPerWave = 4;

        [Tooltip("Total spread angle per wave in degrees.")]
        [Range(0f, 360f)]
        public float SpreadAngle = 60f;

        [Tooltip("Seconds between each wave.")]
        [Min(0f)]
        public float DelayBetweenWaves = 0.4f;

        public override void Execute(BaseEnemy enemy, ProjectilePool pool)
        {
            if (ProjectileData == null || pool == null) return;

            CoroutineRunner.Instance.StartCoroutine(FireWaves(enemy, pool));
        }

        private IEnumerator FireWaves(BaseEnemy enemy, ProjectilePool pool)
        {
            for (int w = 0; w < WaveCount; w++)
            {
                if (enemy == null || pool == null) yield break;

                FireSingleWave(enemy, pool);

                if (w < WaveCount - 1)
                    yield return new WaitForSeconds(DelayBetweenWaves);
            }
        }

        private void FireSingleWave(BaseEnemy enemy, ProjectilePool pool)
        {
            Vector2 baseDir  = DirectionToPlayer(enemy);
            Vector2 spawnPos = GetSpawnPosition(enemy);

            if (ProjectilesPerWave == 1)
            {
                pool.Get(ProjectileData, spawnPos, baseDir);
                return;
            }

            float step       = SpreadAngle / (ProjectilesPerWave - 1);
            float startAngle = -SpreadAngle / 2f;

            for (int i = 0; i < ProjectilesPerWave; i++)
            {
                float   angle = startAngle + step * i;
                Vector2 dir   = Rotate(baseDir, angle);
                pool.Get(ProjectileData, spawnPos, dir);
            }
        }
    }
}

