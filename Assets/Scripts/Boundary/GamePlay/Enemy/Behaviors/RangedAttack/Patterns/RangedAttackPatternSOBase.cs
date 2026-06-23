using Boundary.GamePlay.Enemy.Base;
using Boundary.GamePlay.Projectile;
using Data.Entities.Projectile;
using UnityEngine;

namespace Boundary.GamePlay.Enemy.Behaviors.RangedAttack.Patterns
{
    /// <summary>
    /// Abstract base for all ranged-attack pattern ScriptableObjects.
    /// Subclasses define HOW a set of projectiles is fired (single, spread, burst, etc.).
    /// Assign one subclass instance to EnemyRangedAttackPatternSO.Pattern in the inspector.
    /// </summary>
    public abstract class RangedAttackPatternSOBase : ScriptableObject
    {
        [Header("Projectile to fire")]
        public ProjectileData ProjectileData;

        [Header("Spawn position offset relative to the enemy transform")]
        public Vector2 SpawnOffset = Vector2.zero;

        /// <summary>
        /// Called by the ranged attack behaviour SO to fire the pattern.
        /// The coroutine runner is required for patterns that fire over multiple frames (burst/wave).
        /// </summary>
        public abstract void Execute(BaseEnemy enemy, ProjectilePool pool);

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>Returns the world-space spawn position for a given enemy.</summary>
        protected Vector2 GetSpawnPosition(BaseEnemy enemy)
        {
            var pos = (Vector2)enemy.transform.position + SpawnOffset;
            var flipX  = enemy.transform.localScale.x < 0 ? -1f : 1f;
            pos.x += SpawnOffset.x * flipX;
            return pos;
        }

        /// <summary>Returns a normalised direction vector from the enemy toward the player.</summary>
        protected static Vector2 DirectionToPlayer(BaseEnemy enemy)
        {
            if (enemy.Player == null) return Vector2.right;
            var dir = (Vector2)(enemy.Player.transform.position - enemy.transform.position);
            return dir.normalized;
        }

        /// <summary>Rotates a direction vector by <paramref name="degrees"/>.</summary>
        protected static Vector2 Rotate(Vector2 dir, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(cos * dir.x - sin * dir.y,
                               sin * dir.x + cos * dir.y).normalized;
        }
    }
}

