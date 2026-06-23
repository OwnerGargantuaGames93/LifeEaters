using System.Collections.Generic;
using Data.Entities.Item;
using UnityEngine;

namespace Data.Database.Sources.Essences
{
    [CreateAssetMenu(fileName = "EssenceDatabase", menuName = "GameData/EssenceDatabase")]
    public class EssenceDatabase: ScriptableObject
    {
        public List<EssenceItem> items;
    }
}