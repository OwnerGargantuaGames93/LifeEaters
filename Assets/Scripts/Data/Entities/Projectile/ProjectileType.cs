namespace Data.Entities.Projectile
{
    /// <summary>
    /// Defines the physical behaviour of a projectile.
    /// Ballistic: travels in a straight line, no outline — passes through walls.
    /// Physical: affected by gravity, has a solid outline, bounces/stops on ground/walls.
    /// </summary>
    public enum ProjectileType
    {
        Ballistic,
        Physical
    }

    /// <summary>
    /// Danger tier of a projectile — drives visual saturation and glow intensity.
    /// </summary>
    public enum ProjectileDamageTier
    {
        I   = 1,
        II  = 2,
        III = 3,
        IV  = 4,
        V   = 5
    }

    /// <summary>
    /// Visual colour of the projectile, indicating the type of effect it carries.
    /// </summary>
    public enum ProjectileEffectColor
    {
        White,   // HP damage only
        Green,   // Poison
        Red,     // Burn
        Cyan     // Frost
    }
}

