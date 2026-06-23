using System;
using Data.Entities.Player;

namespace Control.Player.UseCase
{
  /// <summary>
  /// Logic related to level up and calculating player attributes
  /// </summary>
  public static class ULevelUp
  {
    // The table below shows the expected energy points for each level looking this formula: baseXP * (n - offset) ^ growth + n * scale
    // Level | Expected Energy Points | Actual Energy Points
    // 6     | 0                      | 0
    // 7     | 300                    | 300
    // 8     | 900                    | 900
    // 9     | 1800                   | 1800
    // 10    | 3000                   | 3000
    // 11    | 4500                   | 4500
    // 12    | 6300                   | 6300
    // 13    | 8400                   | 8400
    // 14    | 10800                  | 10800
    // 15    | 13500                  | 13500
    // 16    | 16500                  | 16500
    // 17    | 19800                  | 19800
    // 18    | 23400                  | 23400
    // 19    | 27300                  | 27300
    // 20    | 31500                  | 31500
    // .........
    // 57    | 397800                 | 397800
    // 58    | 413400                 | 413400
    // 59    | 429300                 | 429300
    // 60    | 445500                 | 445500
    private static int LevelUpCostForLevel(int level)
    {
      if (level <= 6)
      {
        return 0;
      }
      
      // baseXP * (n - offset) ^ growth + n * scale
      const int baseXp = 250;
      // Because the first level is 6, we need to offset the current level by 1
      const int offset = 5;
      const float growth = 1.7f;

      var scale = level switch
      {
        <= 10 => 50,
        <= 15 => 100,
        <= 20 => 200,
        <= 25 => 400,
        _ => 700
      };

      return (int)(baseXp * Math.Pow(level - offset, growth) + level * scale);
    }

    public static int GetNextLevelPoints(PlayerCharacteristics characteristics)
    {
      var currentLevel = characteristics.GetLevelPoints();
      
      return LevelUpCostForLevel(currentLevel + 1);
    }

    public static int GetLevelUpCost(PlayerCharacteristics startingCharacteristics, PlayerCharacteristics characteristicsModifier)
    {
      // count the sum of all characteristics in the modifier
      var currentLevelPoints = startingCharacteristics.GetLevelPoints();
      var modifierLevelPoints = characteristicsModifier.GetLevelPoints();
      
      // each point in the modifier costs the next level points
      var cost = 0;
      for (var i = currentLevelPoints; i <= currentLevelPoints + modifierLevelPoints; i++)
      {
        cost += LevelUpCostForLevel(i);
      }

      return cost;
    }
  }
}