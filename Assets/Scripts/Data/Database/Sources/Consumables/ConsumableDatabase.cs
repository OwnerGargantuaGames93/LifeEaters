using System.Collections.Generic;
using Data.Entities.Item;
using UnityEngine;

namespace Data.Database.Sources.Consumables
{
    [CreateAssetMenu(fileName = "ConsumableDatabase", menuName = "GameData/ConsumableDatabase")]
    public class ConsumableDatabase: ScriptableObject
    {
        public List<ConsumableItem> Items;
    }
}