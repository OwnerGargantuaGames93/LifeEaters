using System;
using UnityEngine;

namespace Data.Entities.Player
{
  [Serializable]
  public class PlayerAttributes
  {
    public float MaxHealth;
    public float MaxLifes;
    public float MaxEnergy;
    public float JumpAttackBaseDamage;
    public float ThrowAttackBaseDamage;
    public float RipAttackBaseDamage;
    public float Defense;
    public float DashDuration;
    public float Recovery;
    public float BurnResistance;
    public float PoisonResistance;
    public float FrostResistance;
    public float DropRate;
    public float CritRate;
    public float PitNumber;
    public float Oratory;
    public float AlienOratory;
    public float Grabbing;
    public float EssenceSlots;
    public float JumpAttackPoisonBuildUp;
    public float JumpAttackBurnBuildUp;
    public float JumpAttackFrostBuildUp;

    public static PlayerAttributes Identity() {
      return new PlayerAttributes(
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
      );
    }

    public static PlayerAttributes InitialValue()
    {
      return new PlayerAttributes(
        3, 2, 3, 1, 1, 1, 1, 1, 1, 5, 5, 5, 1, 1, 2, 1, 1, 1, 3, 0, 0, 0
      );
    }

    public PlayerAttributes() {
      MaxHealth = 0;
      MaxLifes = 0;
      MaxEnergy = 0;
      JumpAttackBaseDamage = 0;
      ThrowAttackBaseDamage = 0;
      RipAttackBaseDamage = 0;
      Defense = 0;
      DashDuration = 0;
      Recovery = 0;
      BurnResistance = 0;
      PoisonResistance = 0;
      FrostResistance = 0;
      DropRate = 0;
      CritRate = 0;
      PitNumber = 0;
      Oratory = 0;
      AlienOratory = 0;
      Grabbing = 0;
      EssenceSlots = 0;
      JumpAttackPoisonBuildUp = 0;
      JumpAttackBurnBuildUp = 0;
      JumpAttackFrostBuildUp = 0;
    }

    public PlayerAttributes(
      float maxHealth,
      float maxLifes,
      float maxEnergy,
      float jumpAttackBaseDamage,
      float throwAttackBaseDamage,
      float ripAttackBaseDamage,
      float defense,
      float dashDuration,
      float recovery,
      float burnResistance,
      float poisonResistance,
      float frostResistance,
      float dropRate,
      float critRate,
      float pitNumber,
      float oratory,
      float alienOratory,
      float grabbing,
      float essenceSlots,
      float jumpAttackPoisonBuildUp,
      float jumpAttackBurnBuildUp,
      float jumpAttackFrostBuildUp
    ) {
      MaxHealth = maxHealth;
      MaxLifes = maxLifes;
      MaxEnergy = maxEnergy;
      JumpAttackBaseDamage = jumpAttackBaseDamage;
      ThrowAttackBaseDamage = throwAttackBaseDamage;
      RipAttackBaseDamage = ripAttackBaseDamage;
      Defense = defense;
      DashDuration = dashDuration;
      Recovery = recovery;
      BurnResistance = burnResistance;
      PoisonResistance = poisonResistance;
      FrostResistance = frostResistance;
      DropRate = dropRate;
      CritRate = critRate;
      PitNumber = pitNumber;
      Oratory = oratory;
      AlienOratory = alienOratory;
      Grabbing = grabbing;
      EssenceSlots = essenceSlots;
      JumpAttackPoisonBuildUp = jumpAttackPoisonBuildUp;
      JumpAttackBurnBuildUp = jumpAttackBurnBuildUp;
      JumpAttackFrostBuildUp = jumpAttackFrostBuildUp;
    }

    public static PlayerAttributes operator +(PlayerAttributes a, PlayerAttributes b) {
      return new PlayerAttributes(
        a.MaxHealth + b.MaxHealth,
        a.MaxLifes + b.MaxLifes,
        a.MaxEnergy + b.MaxEnergy,
        a.JumpAttackBaseDamage + b.JumpAttackBaseDamage,
        a.ThrowAttackBaseDamage + b.ThrowAttackBaseDamage,
        a.RipAttackBaseDamage + b.RipAttackBaseDamage,
        a.Defense + b.Defense,
        a.DashDuration + b.DashDuration,
        a.Recovery + b.Recovery,
        a.BurnResistance + b.BurnResistance,
        a.PoisonResistance + b.PoisonResistance,
        a.FrostResistance + b.FrostResistance,
        a.DropRate + b.DropRate,
        a.CritRate + b.CritRate,
        a.PitNumber + b.PitNumber,
        a.Oratory + b.Oratory,
        a.AlienOratory + b.AlienOratory,
        a.Grabbing + b.Grabbing,
        a.EssenceSlots + b.EssenceSlots,
        a.JumpAttackPoisonBuildUp + b.JumpAttackPoisonBuildUp,
        a.JumpAttackBurnBuildUp + b.JumpAttackBurnBuildUp,
        a.JumpAttackFrostBuildUp + b.JumpAttackFrostBuildUp
      );
    }

    public static PlayerAttributes operator -(PlayerAttributes a, PlayerAttributes b)
    {
      return new PlayerAttributes(
        a.MaxHealth - b.MaxHealth,
        a.MaxLifes - b.MaxLifes,
        a.MaxEnergy - b.MaxEnergy,
        a.JumpAttackBaseDamage - b.JumpAttackBaseDamage,
        a.ThrowAttackBaseDamage - b.ThrowAttackBaseDamage,
        a.RipAttackBaseDamage - b.RipAttackBaseDamage,
        a.Defense - b.Defense,
        a.DashDuration - b.DashDuration,
        a.Recovery - b.Recovery,
        a.BurnResistance - b.BurnResistance,
        a.PoisonResistance - b.PoisonResistance,
        a.FrostResistance - b.FrostResistance,
        a.DropRate - b.DropRate,
        a.CritRate - b.CritRate,
        a.PitNumber - b.PitNumber,
        a.Oratory - b.Oratory,
        a.AlienOratory - b.AlienOratory,
        a.Grabbing - b.Grabbing,
        a.EssenceSlots - b.EssenceSlots,
        a.JumpAttackPoisonBuildUp - b.JumpAttackPoisonBuildUp,
        a.JumpAttackBurnBuildUp - b.JumpAttackBurnBuildUp,
        a.JumpAttackFrostBuildUp - b.JumpAttackFrostBuildUp
      );
    }

    public override string ToString()
    {
      String result = "";
      if (this.MaxHealth != 0)
      {
        result += $"MHP: {this.MaxHealth}\n";
      }
    
      if (this.MaxLifes != 0)
      {
        result += $"MLF: {this.MaxLifes}\n";
      }
    
      if (this.MaxEnergy != 0)
      {
        result += $"MEN: {this.MaxEnergy}\n";
      }
    
      if (this.JumpAttackBaseDamage != 0)
      {
        result += $"JATT: {this.JumpAttackBaseDamage}\n";
      }
    
      if (this.ThrowAttackBaseDamage != 0)
      {
        result += $"TATT: {this.ThrowAttackBaseDamage}\n";
      }
    
      if (this.RipAttackBaseDamage != 0)
      {
        result += $"RATT: {this.RipAttackBaseDamage}\n";
      }
    
      if (this.Defense != 0)
      {
        result += $"DEF: {this.Defense}\n";
      }
    
      if (this.DashDuration != 0)
      {
        result += $"DUR: {this.DashDuration}\n";
      }
    
      if (this.Recovery != 0)
      {
        result += $"REC: {this.Recovery}\n";
      }
    
      if (this.BurnResistance != 0)
      {
        result += $"BRN: {this.BurnResistance}\n";
      }
    
      if (this.PoisonResistance != 0)
      {
        result += $"PSN: {this.PoisonResistance}\n";
      }
    
      if (this.FrostResistance != 0)
      {
        result += $"FRS: {this.FrostResistance}\n";
      }
    
      if (this.DropRate != 0)
      {
        result += $"DRP: {this.DropRate}\n";
      }
    
      if (this.CritRate != 0)
      {
        result += $"CRT: {this.CritRate}\n";
      }
    
      if (this.PitNumber != 0)
      {
        result += $"PNO: {this.PitNumber}\n";
      }
    
      if (this.Oratory != 0)
      {
        result += $"ORT: {this.Oratory}\n";
      }
    
      if (this.AlienOratory != 0)
      {
        result += $"AORT: {this.AlienOratory}\n";
      }
    
      if (this.Grabbing != 0)
      {
        result += $"GRB: {this.Grabbing}\n";
      }
    
      if (this.EssenceSlots != 0)
      {
        result += $"ESS: {this.EssenceSlots}\n";
      }

      if (this.JumpAttackPoisonBuildUp != 0)
      {
        result += $"JPSN: {this.JumpAttackPoisonBuildUp}\n";
      }

      if (this.JumpAttackBurnBuildUp != 0)
      {
        result += $"JBRN: {this.JumpAttackBurnBuildUp}\n";
      }

      if (this.JumpAttackFrostBuildUp != 0)
      {
        result += $"JFRS: {this.JumpAttackFrostBuildUp}\n";
      }

      return result;
    }

    public string GetDifferential(PlayerAttributes attributesModifier)
    {
      if (attributesModifier == null)
      {
        return ToString();
      }
    
      var result = "";
      if (MaxHealth != 0)
      {
        result += $"MHP: {MaxHealth}";
      
        if (attributesModifier.MaxHealth != 0)
        {
          var sign = attributesModifier.MaxHealth > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.MaxHealth + ")";
        }
      
        result += "\n";
      }
    
      if (MaxLifes != 0)
      {
        result += $"MLF: {MaxLifes}";
      
        if (attributesModifier.MaxLifes != 0)
        {
          var sign = attributesModifier.MaxLifes > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.MaxLifes + ")";
        }
      
        result += "\n";
      }
    
      if (MaxEnergy != 0)
      {
        result += $"MEN: {MaxEnergy}";
      
        if (attributesModifier.MaxEnergy != 0)
        {
          var sign = attributesModifier.MaxEnergy > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.MaxEnergy + ")";
        }
      
        result += "\n";
      }
    
      if (JumpAttackBaseDamage != 0)
      {
        result += $"JATT: {JumpAttackBaseDamage}";
      
        if (attributesModifier.JumpAttackBaseDamage != 0)
        {
          var sign = attributesModifier.JumpAttackBaseDamage > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.JumpAttackBaseDamage + ")";
        }
      
        result += "\n";
      }
    
      if (ThrowAttackBaseDamage != 0)
      {
        result += $"TATT: {ThrowAttackBaseDamage}";
      
        if (attributesModifier.ThrowAttackBaseDamage != 0)
        {
          var sign = attributesModifier.ThrowAttackBaseDamage > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.ThrowAttackBaseDamage + ")";
        }
      
        result += "\n";
      }
    
      if (RipAttackBaseDamage != 0)
      {
        result += $"RATT: {RipAttackBaseDamage}";
      
        if (attributesModifier.RipAttackBaseDamage != 0)
        {
          var sign = attributesModifier.RipAttackBaseDamage > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.RipAttackBaseDamage + ")";
        }
      
        result += "\n";
      }
    
      if (Defense != 0)
      {
        result += $"DEF: {Defense}";
      
        if (attributesModifier.Defense != 0)
        {
          var sign = attributesModifier.Defense > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.Defense + ")";
        }
      
        result += "\n";
      }
    
      if (DashDuration != 0)
      {
        result += $"DUR: {DashDuration}";
      
        if (attributesModifier.DashDuration != 0)
        {
          var sign = attributesModifier.DashDuration > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.DashDuration + ")";
        }
      
        result += "\n";
      }
    
      if (Recovery != 0)
      {
        result += $"REC: {Recovery}";
      
        if (attributesModifier.Recovery != 0)
        {
          var sign = attributesModifier.Recovery > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.Recovery + ")";
        }
      
        result += "\n";
      }
    
      if (BurnResistance != 0)
      {
        result += $"BRN: {BurnResistance}";
      
        if (attributesModifier.BurnResistance != 0)
        {
          var sign = attributesModifier.BurnResistance > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.BurnResistance + ")";
        }
      
        result += "\n";
      }
    
      if (PoisonResistance != 0)
      {
        result += $"PSN: {PoisonResistance}";
      
        if (attributesModifier.PoisonResistance != 0)
        {
          var sign = attributesModifier.PoisonResistance > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.PoisonResistance + ")";
        }
      
        result += "\n";
      }
    
      if (FrostResistance != 0)
      {
        result += $"FRS: {FrostResistance}";
      
        if (attributesModifier.FrostResistance != 0)
        {
          var sign = attributesModifier.FrostResistance > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.FrostResistance + ")";
        }
      
        result += "\n";
      }
    
      if (DropRate != 0)
      {
        result += $"DRP: {DropRate}";
      
        if (attributesModifier.DropRate != 0)
        {
          var sign = attributesModifier.DropRate > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.DropRate + ")";
        }
      
        result += "\n";
      }
    
      if (CritRate != 0)
      {
        result += $"CRT: {CritRate}\n";
      
        if (attributesModifier.CritRate != 0)
        {
          var sign = attributesModifier.CritRate > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.CritRate + ")";
        }
      
        result += "\n";
      }
    
      if (PitNumber != 0)
      {
        result += $"PNO: {PitNumber}";
      
        if (attributesModifier.PitNumber != 0)
        {
          var sign = attributesModifier.PitNumber > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.PitNumber + ")";
        }
      
        result += "\n";
      }
    
      if (Oratory != 0)
      {
        result += $"ORT: {Oratory}";
      
        if (attributesModifier.Oratory != 0)
        {
          var sign = attributesModifier.Oratory > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.Oratory + ")";
        }
      
        result += "\n";
      }
    
      if (AlienOratory != 0)
      {
        result += $"AORT: {AlienOratory}";
      
        if (attributesModifier.AlienOratory != 0)
        {
          var sign = attributesModifier.AlienOratory > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.AlienOratory + ")";
        }
      
        result += "\n";
      }
    
      if (Grabbing != 0)
      {
        result += $"GRB: {Grabbing}";
      
        if (attributesModifier.Grabbing != 0)
        {
          var sign = attributesModifier.Grabbing > 0 ? "+" : "";
        
          result += " (" + sign + attributesModifier.Grabbing + ")";
        }
      
        result += "\n";
      }
    
      if (EssenceSlots != 0)
      {
        result += $"ESS: {EssenceSlots}";

        if (attributesModifier.EssenceSlots != 0)
        {
          var sign = attributesModifier.EssenceSlots > 0 ? "+" : "";
          result += " (" + sign + attributesModifier.EssenceSlots + ")";
        }

        result += "\n";
      }

      if (JumpAttackPoisonBuildUp != 0)
      {
        result += $"JPSN: {JumpAttackPoisonBuildUp}";

        if (attributesModifier.JumpAttackPoisonBuildUp != 0)
        {
          var sign = attributesModifier.JumpAttackPoisonBuildUp > 0 ? "+" : "";
          result += " (" + sign + attributesModifier.JumpAttackPoisonBuildUp + ")";
        }

        result += "\n";
      }

      if (JumpAttackBurnBuildUp != 0)
      {
        result += $"JBRN: {JumpAttackBurnBuildUp}";

        if (attributesModifier.JumpAttackBurnBuildUp != 0)
        {
          var sign = attributesModifier.JumpAttackBurnBuildUp > 0 ? "+" : "";
          result += " (" + sign + attributesModifier.JumpAttackBurnBuildUp + ")";
        }

        result += "\n";
      }

      if (JumpAttackFrostBuildUp != 0)
      {
        result += $"JFRS: {JumpAttackFrostBuildUp}";

        if (attributesModifier.JumpAttackFrostBuildUp != 0)
        {
          var sign = attributesModifier.JumpAttackFrostBuildUp > 0 ? "+" : "";
          result += " (" + sign + attributesModifier.JumpAttackFrostBuildUp + ")";
        }

        result += "\n";
      }

      return result;
    }

    public void CalculatePercentage(float percentage)
    {
      if (percentage is < 0 or > 100)
      {
        throw new ArgumentOutOfRangeException("Percentage must be between 0 and 100");
      }

      if (EssenceSlots != 0 || PitNumber != 0)
      {
        Debug.LogWarning("You can't apply percentage to some attributes.");
      }
      
      if (MaxHealth != 0)
      {
        var isPositive = MaxHealth > 0;
        var value = (MaxHealth * (percentage / 100f));
        
        if (isPositive)
        {
          MaxHealth += value;  
        }
        else
        {
          MaxHealth -= value;  
        }
      }
      
      if (MaxLifes != 0)
      {
        var isPositive = MaxLifes > 0;
        var value = (MaxLifes * (percentage / 100f));
        
        if (isPositive)
        {
          MaxLifes += value;  
        }
        else
        {
          MaxLifes -= value;  
        }
      }

      if (MaxEnergy != 0)
      {
        var isPositive = MaxEnergy > 0;
        var value = (MaxEnergy * (percentage / 100f));

        if (isPositive)
        {
          MaxEnergy += value;
        }
        else
        {
          MaxEnergy -= value;
        }
      }
      
      if (DropRate != 0)
      {
        var isPositive = DropRate > 0;
        var value = DropRate * (percentage / 100f);
        
        if (isPositive)
        {
          DropRate += value;  
        }
        else
        {
          DropRate -= value;  
        }
      }

      if (Defense != 0)
      {
        var isPositive = Defense > 0;
        var value = Defense * (percentage / 100f);

        if (isPositive)
        {
          Defense += value;
        }
        else
        {
          Defense -= value;
        }
      }

      if (CritRate != 0)
      {
        var isPositive = CritRate > 0;
        var value = (CritRate * (percentage / 100f));

        if (isPositive)
        {
          CritRate += value;
        }
        else
        {
          CritRate -= value;
        }
      }
      
      if (JumpAttackBaseDamage != 0)
      {
        var isPositive = JumpAttackBaseDamage > 0;
        var value = (JumpAttackBaseDamage * (percentage / 100f));
        
        if (isPositive)
        {
          JumpAttackBaseDamage += value;  
        }
        else
        {
          JumpAttackBaseDamage -= value;  
        }
      }

      if (ThrowAttackBaseDamage != 0)
      {
        var isPositive = ThrowAttackBaseDamage > 0;
        var value = (ThrowAttackBaseDamage * (percentage / 100f));

        if (isPositive)
        {
          ThrowAttackBaseDamage += value;
        }
        else
        {
          ThrowAttackBaseDamage -= value;
        }
      }
      
      if (RipAttackBaseDamage != 0)
      {
        var isPositive = RipAttackBaseDamage > 0;
        var value = (RipAttackBaseDamage * (percentage / 100f));

        if (isPositive)
        {
          RipAttackBaseDamage += value;
        }
        else
        {
          RipAttackBaseDamage -= value;
        }
      }
      
      if (BurnResistance != 0)
      {
        var isPositive = BurnResistance > 0;
        var value = (BurnResistance * (percentage / 100f));
        
        if (isPositive)
        {
          BurnResistance += value;  
        }
        else
        {
          BurnResistance -= value;  
        }
      }
      
      if (PoisonResistance != 0)
      {
        var isPositive = PoisonResistance > 0;
        var value = (PoisonResistance * (percentage / 100f));
        
        if (isPositive)
        {
          PoisonResistance += value;  
        }
        else
        {
          PoisonResistance -= value;  
        }
      }
      
      if (FrostResistance != 0)
      {
        var isPositive = FrostResistance > 0;
        var value = (FrostResistance * (percentage / 100f));
        
        if (isPositive)
        {
          FrostResistance += value;  
        }
        else
        {
          FrostResistance -= value;  
        }
      }
      
      if (DashDuration != 0)
      {
        var isPositive = DashDuration > 0;
        var value = (DashDuration * (percentage / 100f));
        
        if (isPositive)
        {
          DashDuration += value;  
        }
        else
        {
          DashDuration -= value;  
        }
      }

      if (Recovery != 0)
      {
        var isPositive = Recovery > 0;
        var value = (Recovery * (percentage / 100f));

        if (isPositive)
        {
          Recovery += value;
        }
        else
        {
          Recovery -= value;
        }
      }
      
      if (Oratory != 0)
      {
        var isPositive = Oratory > 0;
        var value = (Oratory * (percentage / 100f));
        
        if (isPositive)
        {
          Oratory += value;  
        }
        else
        {
          Oratory -= value;  
        }
      }
      
      if (AlienOratory != 0)
      {
        var isPositive = AlienOratory > 0;
        var value = (AlienOratory * (percentage / 100f));
        
        if (isPositive)
        {
          AlienOratory += value;  
        }
        else
        {
          AlienOratory -= value;  
        }
      }

      if (Grabbing != 0)
      {
        var isPositive = Grabbing > 0;
        var value = (Grabbing * (percentage / 100f));

        if (isPositive)
        {
          Grabbing += value;
        }
        else
        {
          Grabbing -= value;
        }
      }

      if (JumpAttackPoisonBuildUp != 0)
      {
        var isPositive = JumpAttackPoisonBuildUp > 0;
        var value = (JumpAttackPoisonBuildUp * (percentage / 100f));

        if (isPositive)
        {
          JumpAttackPoisonBuildUp += value;
        }
        else
        {
          JumpAttackPoisonBuildUp -= value;
        }
      }

      if (JumpAttackBurnBuildUp != 0)
      {
        var isPositive = JumpAttackBurnBuildUp > 0;
        var value = (JumpAttackBurnBuildUp * (percentage / 100f));

        if (isPositive)
        {
          JumpAttackBurnBuildUp += value;
        }
        else
        {
          JumpAttackBurnBuildUp -= value;
        }
      }

      if (JumpAttackFrostBuildUp != 0)
      {
        var isPositive = JumpAttackFrostBuildUp > 0;
        var value = (JumpAttackFrostBuildUp * (percentage / 100f));

        if (isPositive)
        {
          JumpAttackFrostBuildUp += value;
        }
        else
        {
          JumpAttackFrostBuildUp -= value;
        }
      }
    }
  }
}