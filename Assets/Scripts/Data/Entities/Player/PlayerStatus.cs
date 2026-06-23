using System;

// This class represents the current status of the player
// By "status" we mean the current situation of the player in terms of health, power-ups, collectibles, etc.
// This class is used to keep track of the player's status during the game and should be saved during the game save
namespace Data.Entities.Player
{
  [Serializable]
  public class PlayerStatus
  {
    // The current health of the player
    // This value should be between 0 and the maximum health of the player
    // If the health reaches 0, the player loses a life and the health is reset to the maximum health
    public float currentHealth;

    // The current number of lives of the player
    // This value should be between 0 and the maximum number of lives of the player
    // If the number of lives reaches 0, game over
    public float currentLifes;

    // This value be between 0 and the maximum energy of the player
    // The energy is consumed when the player creating pits
    // The energy is regenerated performing jump attacks, headbutts and use some consumables
    // The player can't create pits if the energy is 0
    public float currentEnergy;

    // The current amount of burn damage on the player
    // This value should be between 0 and the burn resistance of the player
    // If the burn amount reaches the burn resistance, the player is burned
    // When the player is burned, the current burn amount is reset to 0 and can't be burned again
    // until the burn effect is over
    public float currentBurnAmount;

    // A flag indicating if the player is burned
    // If the player is burned, every damage taken is increased by a certain amount
    // The burn effect ends after 30 seconds
    public bool isBurned;

    // The current amount of poison damage on the player
    // This value should be between 0 and the poison resistance of the player
    // If the poison amount reaches the poison resistance, the player is poisoned
    // When the player is poisoned, the current poison amount is reset to 0 and can't be poisoned again
    // until the poison effect is over
    public float currentPoisonAmount;

    // A flag indicating if the player is poisoned
    // If the player is poisoned, the player loses health over time
    // The poison effect ends after 30 seconds
    public bool isPoisoned;
  
    // The current amount of frost damage on the player
    // This value should be between 0 and the frost resistance of the player
    // If the frost amount reaches the frost resistance, the player is frostbitten
    // When the player is frostbitten, the current frost amount is reset to 0 and can't be frostbitten again
    // until the frost effect is over
    public float currentFrostAmount;

    // A flag indicating if the player is frostbitten
    // If the player is frostbitten, the player moves slower
    // The frostbite effect ends after 30 seconds
    public bool isFrostbitten;

    // The number of coins collected by the player
    // Coins can be used to buy equipments, items, etc.
    // In the games exists a fixed number of coins that can be collected
    public float coins;

    // The amount of points collected by the player
    // Points can be used to level up the player, learn life essences and add talents from the containers.
    // Is collected form the bonus (BAR, fruits, symbols, diamonds, etc.), enemies and special consumables.
    public float points;

    // A flag indicating if the player is invulnerable
    // If the player is invulnerable, the player doesn't take damage
    // The invulnerability effect ends after 10 seconds
    // The player can reach the invulnerability state by using special consumables.
    public bool isInvulnerable;

    // A flag indicating if the player is speed boosted
    // If the player is speed boosted, the player moves faster
    // The speed boost effect ends after 10 seconds
    // The player can reach the speed boost state by using special consumables.
    public bool isSpeedBoosted;

    // A flag indicating if the player is power boosted
    // If the player is power boosted, the player deals more damage
    // The power boost effect ends after 10 seconds
    // The player can reach the power boost state by using special consumables.
    public bool isPowerBoosted;

    public float bonuses;

    public float sBonuses;

    public float memories;
    

    public PlayerStatus(
      float currentHealth,
      float currentLifes,
      float currentEnergy,
      float currentBurnAmount,
      bool isBurned,
      float currentPoisonAmount,
      bool isPoisoned,
      float currentFrostAmount,
      bool isFrostbitten,
      float coins,
      float points,
      bool isInvulnerable,
      bool isSpeedBoosted,
      bool isPowerBoosted,
      float bonuses,
      float sBonuses,
      float memories
      ) {
      this.currentHealth = currentHealth;
      this.currentLifes = currentLifes;
      this.currentEnergy = currentEnergy;
      this.currentBurnAmount = currentBurnAmount;
      this.isBurned = isBurned;
      this.currentPoisonAmount = currentPoisonAmount;
      this.isPoisoned = isPoisoned;
      this.currentFrostAmount = currentFrostAmount;
      this.isFrostbitten = isFrostbitten;
      this.coins = coins;
      this.points = points;
      this.isInvulnerable = isInvulnerable;
      this.isSpeedBoosted = isSpeedBoosted;
      this.isPowerBoosted = isPowerBoosted;
      this.bonuses = bonuses;
      this.sBonuses = sBonuses;
      this.memories = memories;
    }

    public PlayerStatus()
    {
      currentHealth = 0;
      currentLifes = 0;
      currentEnergy = 0;
      currentBurnAmount = 0;
      isBurned = false;
      currentPoisonAmount = 0;
      isPoisoned = false;
      currentFrostAmount = 0;
      isFrostbitten = false;
      coins = 0;
      points = 0;
      isInvulnerable = false;
      isSpeedBoosted = false;
      isPowerBoosted = false;
      bonuses = 0;
      sBonuses = 0;
      memories = 0;
    }

    public static PlayerStatus Identity()
    {
      return new PlayerStatus(
        0, 0, 0f,
        0, false,
        0, false,
        0, false,
        0, 0,
        false, false, false, 0, 0, 0
      );
    }

    public PlayerStatus(PlayerAttributes attributes) {
      currentHealth = attributes.MaxHealth;
      currentLifes = attributes.MaxLifes;
      currentEnergy = attributes.MaxEnergy;
      currentBurnAmount = 0;
      currentPoisonAmount = 0;
      currentFrostAmount = 0;
      isBurned = false;
      isPoisoned = false;
      isFrostbitten = false;
      coins = 0;
      points = 0;
      isInvulnerable = false;
      isSpeedBoosted = false;
      isPowerBoosted = false;
      bonuses = 0;
      sBonuses = 0;
      memories = 0;
    }

    public static PlayerStatus operator +(PlayerStatus a, PlayerStatus b)
    {
      return new PlayerStatus(
        a.currentHealth + b.currentHealth,
        a.currentLifes + b.currentLifes,
        a.currentEnergy + b.currentEnergy,
        a.currentBurnAmount + b.currentBurnAmount,
        a.isBurned && b.isBurned,
        a.currentPoisonAmount + b.currentPoisonAmount,
        a.isPoisoned && b.isPoisoned,
        a.currentFrostAmount + b.currentFrostAmount,
        a.isFrostbitten && b.isFrostbitten,
        a.coins + b.coins,
        a.points + b.points,
        a.isInvulnerable && b.isInvulnerable,
        a.isSpeedBoosted && b.isSpeedBoosted,
        a.isPowerBoosted && b.isPowerBoosted,
        a.bonuses + b.bonuses,
        a.sBonuses + b.sBonuses,
        a.memories + b.memories
      );
    }
  }
}