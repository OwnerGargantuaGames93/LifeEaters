using System.Collections.Generic;
using Data.Entities.Item;
using UnityEngine;

namespace Data.Database.Sources.Keys
{

    [CreateAssetMenu(fileName = "KeyDatabase", menuName = "GameData/KeyDatabase")]
    public class KeyDatabase: ScriptableObject
    {
        public List<KeyItem> Items;
    }
}