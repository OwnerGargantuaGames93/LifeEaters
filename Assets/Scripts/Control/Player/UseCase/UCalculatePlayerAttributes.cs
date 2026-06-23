using Data.Entities.Player;

namespace Control.Player.UseCase
{
    /// <summary>
    /// Determinate the player attributes based on their characteristics.
    /// Calculate
    /// </summary>
    public static class UCalculatePlayerAttributes
    {
        public static PlayerAttributes VitBasedAttributes(PlayerCharacteristics characteristics,
        PlayerAttributes attributesModifier)
    {
        var vitality = characteristics.Vitality;
        switch (vitality)
        {
            case 2:
            {
                attributesModifier.MaxHealth += 1;
                attributesModifier.Defense += 1;
                break;
            }
            case 3:
            {
                attributesModifier.MaxHealth += 1;
                attributesModifier.Defense += 2;
                break;
            }
            case 4:
            {
                attributesModifier.MaxHealth += 2;
                attributesModifier.Defense += 3;
                break;
            }
            case 5:
            {
                attributesModifier.MaxHealth += 2;
                attributesModifier.Defense += 4;
                break;
            }
            case 6:
            {
                attributesModifier.MaxHealth += 3;
                attributesModifier.Defense += 4;
                break;
            }
            case 7:
            {
                attributesModifier.MaxHealth += 3;
                attributesModifier.Defense += 5;
                break;
            }
            case 8:
            {
                attributesModifier.MaxHealth += 4;
                attributesModifier.Defense += 5;
                break;
            }
            case 9:
            {
                attributesModifier.MaxHealth += 4;
                attributesModifier.Defense += 6;
                break;
            }
            case 10:
            {
                attributesModifier.MaxHealth += 5;
                attributesModifier.Defense += 7;
                break;
            }
            default:
            {
                // NOOP
                break;
            }
        }

        return attributesModifier;
    }

        public static PlayerAttributes StrBasedAttributes(PlayerCharacteristics characteristics,
            PlayerAttributes attributesModifier)
        {
            var strength = characteristics.Strength;
            switch (strength)
            {
                case 2:
                {
                    attributesModifier.ThrowAttackBaseDamage += 1;
                    break;
                }
                case 3:
                {
                    attributesModifier.ThrowAttackBaseDamage += 2;
                    break;
                }
                case 4:
                {
                    attributesModifier.ThrowAttackBaseDamage += 3;
                    break;
                }
                case 5:
                {
                    attributesModifier.ThrowAttackBaseDamage += 4;
                    break;
                }
                case 6:
                {
                    attributesModifier.ThrowAttackBaseDamage += 5;
                    break;
                }
                case 7:
                {
                    attributesModifier.ThrowAttackBaseDamage += 6;
                    break;
                }
                case 8:
                {
                    attributesModifier.ThrowAttackBaseDamage += 7;
                    break;
                }
                case 9:
                {
                    attributesModifier.ThrowAttackBaseDamage += 8;
                    break;
                }
                case 10:
                {
                    attributesModifier.ThrowAttackBaseDamage += 9;
                    break;
                }
            }

            return attributesModifier;
        }

        public static PlayerAttributes AgiBasedAttributes(PlayerCharacteristics characteristics,
            PlayerAttributes attributesModifier)
        {
            var agility = characteristics.Agility;
            switch (agility)
            {
                case 2:
                {
                    attributesModifier.JumpAttackBaseDamage += 1;
                    attributesModifier.DashDuration += 1;
                    break;
                }
                case 3:
                {
                    attributesModifier.JumpAttackBaseDamage += 2;
                    attributesModifier.DashDuration += 1;
                    break;
                }
                case 4:
                {
                    attributesModifier.JumpAttackBaseDamage += 3;
                    attributesModifier.DashDuration += 1;
                    break;
                }
                case 5:
                {
                    attributesModifier.JumpAttackBaseDamage += 4;
                    attributesModifier.DashDuration += 1;
                    break;
                }
                case 6:
                {
                    attributesModifier.JumpAttackBaseDamage += 5;
                    attributesModifier.DashDuration += 1;
                    break;
                }
                case 7:
                {
                    attributesModifier.JumpAttackBaseDamage += 6;
                    attributesModifier.DashDuration += 2;
                    break;
                }
                case 8:
                {
                    attributesModifier.JumpAttackBaseDamage += 7;
                    attributesModifier.DashDuration += 2;
                    break;
                }
                case 9:
                {
                    attributesModifier.JumpAttackBaseDamage += 8;
                    attributesModifier.DashDuration += 2;
                    break;
                }
                case 10:
                {
                    attributesModifier.JumpAttackBaseDamage += 9;
                    attributesModifier.DashDuration += 3;
                    break;
                }
            }

            return attributesModifier;
        }

        public static PlayerAttributes HumBasedAttributes(PlayerCharacteristics characteristics,
            PlayerAttributes attributesModifier)
        {
            var human = characteristics.Human;
            switch (human)
            {
                case 2:
                {
                    attributesModifier.DropRate += 1;
                    attributesModifier.CritRate += 0;
                    attributesModifier.Oratory += 1;
                    attributesModifier.PoisonResistance += 1;
                    break;
                }
                case 3:
                {
                    attributesModifier.DropRate += 1;
                    attributesModifier.CritRate += 0;
                    attributesModifier.Oratory += 1;
                    attributesModifier.PoisonResistance += 2;
                    break;
                }
                case 4:
                {
                    attributesModifier.DropRate += 2;
                    attributesModifier.CritRate += 1;
                    attributesModifier.Oratory += 2;
                    attributesModifier.PoisonResistance += 3;
                    break;
                }
                case 5:
                {
                    attributesModifier.DropRate += 2;
                    attributesModifier.CritRate += 1;
                    attributesModifier.Oratory += 2;
                    attributesModifier.PoisonResistance += 4;
                    break;
                }
                case 6:
                {
                    attributesModifier.DropRate += 3;
                    attributesModifier.CritRate += 1;
                    attributesModifier.Oratory += 3;
                    attributesModifier.PoisonResistance += 5;
                    break;
                }
                case 7:
                {
                    attributesModifier.DropRate += 3;
                    attributesModifier.CritRate += 2;
                    attributesModifier.Oratory += 3;
                    attributesModifier.PoisonResistance += 6;
                    break;
                }
                case 8:
                {
                    attributesModifier.DropRate += 4;
                    attributesModifier.CritRate += 2;
                    attributesModifier.Oratory += 4;
                    attributesModifier.PoisonResistance += 7;
                    break;
                }
                case 9:
                {
                    attributesModifier.DropRate += 4;
                    attributesModifier.CritRate += 2;
                    attributesModifier.Oratory += 4;
                    attributesModifier.PoisonResistance += 8;
                    break;
                }
                case 10:
                {
                    attributesModifier.DropRate += 5;
                    attributesModifier.CritRate += 3;
                    attributesModifier.Oratory += 5;
                    attributesModifier.PoisonResistance += 9;
                    break;
                }
            }

            return attributesModifier;
        }
        
        public static PlayerAttributes GodBasedAttributes(PlayerCharacteristics characteristics,
            PlayerAttributes attributesModifier)
        {
            var god = characteristics.God;
            switch (god)
            {
                case 2:
                {
                    attributesModifier.MaxEnergy += 0;
                    attributesModifier.EssenceSlots += 1;
                    attributesModifier.BurnResistance += 1;
                    break;
                }
                case 3:
                {
                    attributesModifier.MaxEnergy += 1;
                    attributesModifier.EssenceSlots += 2;
                    attributesModifier.BurnResistance += 2;
                    break;
                }
                case 4:
                {
                    attributesModifier.MaxEnergy += 1;
                    attributesModifier.EssenceSlots += 3;
                    attributesModifier.BurnResistance += 3;
                    break;
                }
                case 5:
                {
                    attributesModifier.MaxEnergy += 2;
                    attributesModifier.EssenceSlots += 4;
                    attributesModifier.BurnResistance += 4;
                    break;
                }
                case 6:
                {
                    attributesModifier.MaxEnergy += 2;
                    attributesModifier.EssenceSlots += 5;
                    attributesModifier.BurnResistance += 5;
                    break;
                }
                case 7:
                {
                    attributesModifier.MaxEnergy += 3;
                    attributesModifier.EssenceSlots += 6;
                    attributesModifier.BurnResistance += 6;
                    break;
                }
                case 8:
                {
                    attributesModifier.MaxEnergy += 3;
                    attributesModifier.EssenceSlots += 7;
                    attributesModifier.BurnResistance += 7;
                    break;
                }
                case 9:
                {
                    attributesModifier.MaxEnergy += 4;
                    attributesModifier.EssenceSlots += 8;
                    attributesModifier.BurnResistance += 8;
                    break;
                }
                case 10:
                {
                    attributesModifier.MaxEnergy += 5;
                    attributesModifier.EssenceSlots += 10;
                    attributesModifier.BurnResistance += 9;
                    break;
                }
            }

            return attributesModifier;
        }

        public static PlayerAttributes AlnBasedAttributes(PlayerCharacteristics characteristics,
            PlayerAttributes attributesModifier)
        {
            var alien = characteristics.Alien;
            switch (alien)
            {
                case 2:
                    attributesModifier.MaxLifes += 0;
                    attributesModifier.AlienOratory += 1;
                    attributesModifier.FrostResistance += 1;
                    break;
                case 3:
                    attributesModifier.MaxLifes += 1;
                    attributesModifier.AlienOratory += 1;
                    attributesModifier.FrostResistance += 2;
                    break;
                case 4:
                    attributesModifier.MaxLifes += 2;
                    attributesModifier.AlienOratory += 2;
                    attributesModifier.FrostResistance += 2;
                    break;
                case 5:
                    attributesModifier.MaxLifes += 3;
                    attributesModifier.AlienOratory += 2;
                    attributesModifier.FrostResistance += 2;
                    break;
                case 6:
                    attributesModifier.MaxLifes += 4;
                    attributesModifier.AlienOratory += 3;
                    attributesModifier.FrostResistance += 2;
                    break;
                case 7:
                    attributesModifier.MaxLifes += 4;
                    attributesModifier.AlienOratory += 3;
                    attributesModifier.FrostResistance += 2;
                    break;
                case 8:
                    attributesModifier.MaxLifes += 5;
                    attributesModifier.AlienOratory += 4;
                    attributesModifier.FrostResistance += 2;
                    break;
                case 9:
                    attributesModifier.MaxLifes += 6;
                    attributesModifier.AlienOratory += 4;
                    attributesModifier.FrostResistance += 2;
                    break;
                case 10:
                    attributesModifier.MaxLifes += 7;
                    attributesModifier.AlienOratory += 5;
                    attributesModifier.FrostResistance += 2;
                    break;
            }

            return attributesModifier;
        }
        
        public static PlayerAttributes AttributesFromCharacteristics(PlayerCharacteristics characteristics, PlayerAttributes initialValue)
        {
            var attributesModifier = initialValue;
            
            attributesModifier = VitBasedAttributes(characteristics, attributesModifier);
            attributesModifier = StrBasedAttributes(characteristics, attributesModifier);
            attributesModifier = AgiBasedAttributes(characteristics, attributesModifier);
            attributesModifier = HumBasedAttributes(characteristics, attributesModifier);
            attributesModifier = GodBasedAttributes(characteristics, attributesModifier);
            attributesModifier = AlnBasedAttributes(characteristics, attributesModifier);

            return attributesModifier;
        }
    }
}