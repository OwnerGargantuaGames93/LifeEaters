using System.Collections.Generic;
using Data.Damage;
using Data.Entities.Effects;
using UnityEngine;

namespace Data.Entities.Projectile
{
    /// <summary>
    /// Complete definition of a projectile.
    /// Create one SO per "type" of projectile (e.g. "Basic Poison Tier II Ballistic").
    /// The same SO can be reused by multiple enemies / attack patterns.
    ///
    /// NOTE: DamageOutput is derived automatically from DamageTier — do not set it manually.
    ///   Tier I  = 1 HP,  Tier II = 2 HP,  Tier III = 3 HP,  Tier IV = 4 HP,  Tier V = 5 HP.
    ///   The player's Defense stat will reduce this value (minimum 1) at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectileData_New",
        menuName = "Projectiles/Projectile Data")]
    public class ProjectileData : ScriptableObject
    {
        // ── Visual ──────────────────────────────────────────────────────────
        [Header("Visual")]
        [Tooltip("Ballistic = no outline.  Physical = solid outline, interacts with walls.")]
        public ProjectileType ProjectileType = ProjectileType.Ballistic;

        [Tooltip("Danger tier — controls BOTH the visual (saturation/glow) AND the base health damage (I=1, II=2, III=3, IV=4, V=5).")]
        public ProjectileDamageTier DamageTier = ProjectileDamageTier.I;

        [Tooltip("Base colour that communicates the on-hit effect to the player.")]
        public ProjectileEffectColor EffectColor = ProjectileEffectColor.White;

        [Tooltip("Visual config SO for the chosen tier. Assign the matching ProjectileVisualConfig asset.")]
        public ProjectileVisualConfig VisualConfig;

        // ── Damage ──────────────────────────────────────────────────────────
        [Header("Damage")]
        [Tooltip("Optional on-hit status effects (poison build-up, burn, etc.).")]
        public List<EffectData> OnHitEffects = new();

        /// <summary>
        /// DamageOutput is automatically derived from DamageTier.
        /// Tier I=1 HP, II=2 HP, III=3 HP, IV=4 HP, V=5 HP.
        /// Status build-ups (burn, frost, poison) come from OnHitEffects.
        /// </summary>
        public DamageOutput DamageOutput => new DamageOutput((int)DamageTier, 0, 0, 0, 0);

        // ── Movement ────────────────────────────────────────────────────────
        [Header("Movement")]
        [Tooltip("Travel speed in units per second.")]
        public float Speed = 8f;

        [Tooltip("Seconds before the projectile is automatically returned to the pool.")]
        public float Lifetime = 6f;

        [Tooltip("Rigidbody gravity scale.  0 = Ballistic (straight line).  >0 = Physical (arc).")]
        public float GravityScale = 0f;

        [Tooltip("Only used when ProjectileType = Physical.  " +
                 "0 = stops on impact.  0.5 = moderate bounce.  1 = full elastic bounce.")]
        [Range(0f, 1f)]
        public float BounceFactor = 0f;
    }
}

