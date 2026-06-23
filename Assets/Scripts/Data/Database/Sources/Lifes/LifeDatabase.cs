using System.Collections.Generic;
using Data.Entities.Item;
using UnityEngine;

namespace Data.Database.Sources.Lifes
{
    [CreateAssetMenu(fileName = "LifeDatabase", menuName = "GameData/LifeDatabase")]
    public class LifeDatabase: ScriptableObject
    {
        public List<LifeItem> items;
    }
}