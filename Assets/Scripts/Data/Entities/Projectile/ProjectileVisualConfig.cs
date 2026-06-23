using UnityEngine;

namespace Data.Entities.Projectile
{
    /// <summary>
    /// Shader parameters for a single damage tier.
    /// Create one SO per tier (I–V) and reference them from ProjectileData.
    /// All projectiles share a single Material; values are pushed at runtime
    /// via MaterialPropertyBlock so no material duplication occurs.
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectileVisualConfig_TierX",
        menuName = "Projectiles/Visual Config")]
    public class ProjectileVisualConfig : ScriptableObject
    {
        [Header("Tier")]
        public ProjectileDamageTier Tier;

        [Header("Colour saturation multiplier applied on top of the base effect colour")]
        [Range(0.1f, 3f)]
        public float Saturation = 1f;

        [Header("Glow (outer light halo) radius — 0 = no glow")]
        [Range(0f, 1f)]
        public float GlowRadius = 0f;

        [Header("If true the glow pulses using sin(Time) — use for tier IV and V")]
        public bool GlowPulse = false;
    }
}

