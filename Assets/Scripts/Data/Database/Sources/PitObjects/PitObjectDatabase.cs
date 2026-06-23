using System.Collections.Generic;
using Data.Entities.Item;
using UnityEngine;

namespace Data.Database.Sources.PitObjects
{
    [CreateAssetMenu(fileName = "PitObjectDatabase", menuName = "GameData/PitObjectDatabase")]
    public class PitObjectDatabase: ScriptableObject
    {
        public List<PitObjectData> Items;
    }
}