using System;

namespace Data.Damage
{
  [Serializable]
  public class DamageOutput
  {
    
    public int healthDamage;

    public int burntBuildUp;

    public int frostBuildUp;

    public int poisonBuildUp;
  
    public int directLifeSteal;
    
    public bool criticalHit;

    public DamageOutput(
      int healthDamage,
      int burntBuildUp,
      int frostBuildUp,
      int poisonBuildUp,
      int directLifeSteal
    ) {
      this.healthDamage = healthDamage;
      this.burntBuildUp = burntBuildUp;
      this.frostBuildUp = frostBuildUp;
      this.poisonBuildUp = poisonBuildUp;
      this.directLifeSteal = directLifeSteal;
      criticalHit = false;
    }
    
    public DamageOutput(
      int healthDamage,
      int burntBuildUp,
      int frostBuildUp,
      int poisonBuildUp,
      int directLifeSteal,
      bool criticalHit
    ) {
      this.healthDamage = healthDamage;
      this.burntBuildUp = burntBuildUp;
      this.frostBuildUp = frostBuildUp;
      this.poisonBuildUp = poisonBuildUp;
      this.directLifeSteal = directLifeSteal;
      this.criticalHit = criticalHit;
    }

    public static DamageOutput Identity() {
      return new DamageOutput(
        0,
        0,
        0,
        0,
        0
      );
    }

    public static DamageOutput operator +(DamageOutput a, DamageOutput b) {
      return new DamageOutput(
        a.healthDamage + b.healthDamage,
        a.burntBuildUp + b.burntBuildUp,
        a.frostBuildUp + b.frostBuildUp,
        a.poisonBuildUp + b.poisonBuildUp,
        a.directLifeSteal + b.directLifeSteal
      );
    }

    public override string ToString()
    {
      var result = "";
    
      if (healthDamage != 0)
      {
        result += $"DMG: {healthDamage}\n";
      }
    
      if (burntBuildUp != 0)
      {
        result += $"BRN: {burntBuildUp}\n";
      }
    
      if (frostBuildUp != 0)
      {
        result += $"FST: {frostBuildUp}\n";
      }
    
      if (poisonBuildUp != 0)
      {
        result += $"PSN: {poisonBuildUp}\n";
      }
      
      if (directLifeSteal != 0)
      {
        result += $"LFS: {directLifeSteal}\n";
      }

      return result;
    }
  }
}