using System.Collections.Generic;
using Data.Entities.Item;
using UnityEngine;

namespace Data.Database.Sources.Talents
{
    [CreateAssetMenu(fileName = "TalentDatabase", menuName = "GameData/TalentDatabase")]
    public class TalentDatabase: ScriptableObject
    {
        public List<TalentItem> Items;
    }
}