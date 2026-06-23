// The PlayerCharacteristics class is a data class that stores the player's characteristics that control the player's attributes.
// The player's characteristics are:
// - Vitality
// - Jump
// - Throw
// - Grab
// - Agility
// - Human
// - God
// - Alien
// The player's characteristics are used to calculate the player's attributes and for enable some abilities.
// When leveling up, you can spend the level points to increase the player's characteristics.

using System;

namespace Data.Entities.Player
{
  [Serializable]
  public class PlayerCharacteristics {
    // The player's characteristics that control the following attributes:
    // - maxHealth
    // - defense
    // Based on the player's vitality level, the values of these characteristics vary following a predefined advancement.
    public int Vitality;
  
    // The player's characteristics that control the following attributes:
    // - throwAttackBaseDamage
    // Based on the player's throe level, the values of these characteristics vary following a predefined advancement.
    // Also, this characteristic controls the Grab talent level up.
    public int Strength;

    // The player's characteristics that control the following attributes:
    // - dashDuration
    // - jumpAttackBaseDamage
    // Based on the player's agility level, the values of these characteristics vary following a predefined advancement.
    public int Agility;

    // The player's characteristics that control the following attributes:
    // - dropRate
    // - critRate
    // - oratory
    // - poisonResistance
    // Based on the player's human level, the values of these characteristics vary following a predefined advancement.
    public int Human;

    // The player's characteristics that control the following attributes:
    // - maxEnergy
    // - pitSlots
    // - burnResistance
    // Based on the player's god level, the values of these characteristics vary following a predefined advancement.
    public int God;

    // The player's characteristics that control the following attributes:
    // - maxLifes
    // - alienOratory
    // - frostResistance
    // Based on the player's alien level, the values of these characteristics vary following a predefined advancement.
    public int Alien;

    public PlayerCharacteristics(
      int vtl, int str, int agl, int hmn, int god, int aln
    ) {
      Vitality = vtl;
      Strength = str;
      Agility = agl;
      Human = hmn;
      God = god;
      Alien = aln;
    }

    public PlayerCharacteristics() {
      Vitality = 1;
      Strength = 1;
      Agility = 1;
      Human = 1;
      God = 1;
      Alien = 1;
    }

    public static PlayerCharacteristics Identity() {
      return new PlayerCharacteristics(
        0, 0, 0, 0, 0, 0
      );
    }

    public static PlayerCharacteristics operator +(PlayerCharacteristics a, PlayerCharacteristics b) {
      return new PlayerCharacteristics(
        a.Vitality + b.Vitality,
        a.Strength + b.Strength,
        a.Agility + b.Agility,
        a.Human + b.Human,
        a.God + b.God,
        a.Alien + b.Alien
      );
    }
  
    public static PlayerCharacteristics operator -(PlayerCharacteristics a, PlayerCharacteristics b) {
      return new PlayerCharacteristics(
        a.Vitality - b.Vitality,
        a.Strength - b.Strength,
        a.Agility - b.Agility,
        a.Human - b.Human,
        a.God - b.God,
        a.Alien - b.Alien
      );
    }

    public override bool Equals(object obj)
    {
      if (obj is PlayerCharacteristics characteristics)
      {
        return Vitality == characteristics.Vitality &&
               Strength == characteristics.Strength &&
               Agility == characteristics.Agility &&
               Human == characteristics.Human &&
               God == characteristics.God &&
               Alien == characteristics.Alien;
      }

      // otherwise return the default equals
      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return HashCode.Combine(Vitality, Strength, Agility, Human, God, Alien);
    }

    public override string ToString()
    {
      var result = "";
      if (Vitality != 0)
      {
        result += "VIT: " + Vitality + "\n";
      }
    
      if (Strength != 0)
      {
        result += "STR: " + Strength + "\n";
      }
    
      if (Agility != 0)
      {
        result += "AGI: " + Agility + "\n";
      }
    
      if (Human != 0)
      {
        result += "HUM: " + Human + "\n";
      }
    
      if (God != 0)
      {
        result += "GOD: " + God + "\n";
      }
    
      if (Alien != 0)
      {
        result += "ALI: " + Alien + "\n";
      }
    
      return result;
    }
  
    public int GetLevelPoints()
    {
      return Vitality + Strength + Agility + Human + God + Alien;
    }

    public bool IsIdentity()
    {
      return Vitality == 0 && Strength == 0 && Agility == 0 && Human == 0 && God == 0 && Alien == 0;
    }
  }
}