using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Projectile;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.RangedAttack.Patterns
{
    /// <summary>
    /// Fires a single homing projectile that steers toward the player for a limited time.
    /// After TrackingDuration seconds the projectile continues in a straight line.
    /// Tracking is handled inside TrackingProjectile — this SO just configures it.
    /// </summary>
    [CreateAssetMenu(fileName = "TrackingShot",
        menuName = "Enemies/Behaviors/Ranged Attack Patterns/Tracking Shot")]
    public class TrackingShotPatternSO : RangedAttackPatternSOBase
    {
        [Tooltip("Seconds during which the projectile steers toward the player.")]
        [Min(0f)]
        public float TrackingDuration = 1.5f;

        [Tooltip("How quickly the projectile rotates toward the player (degrees per second).")]
        [Min(0f)]
        public float TrackingStrength = 120f;

        public override void Execute(BaseEnemy enemy, ProjectilePool pool)
        {
            if (ProjectileData == null || pool == null) return;

            // We get a base projectile and inject the tracking parameters via the component
            var p = pool.Get(ProjectileData, GetSpawnPosition(enemy), DirectionToPlayer(enemy));

            // Add or configure the tracking component on the projectile
            var tracker = p.GetComponent<TrackingProjectileController>();
            if (tracker == null)
                tracker = p.gameObject.AddComponent<TrackingProjectileController>();

            tracker.Init(TrackingDuration, TrackingStrength);
        }
    }
}

