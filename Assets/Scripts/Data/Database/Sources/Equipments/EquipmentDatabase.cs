using System.Collections.Generic;
using Data.Entities.Item;
using UnityEngine;

namespace Data.Database.Sources.Equipments
{
    [CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "GameData/EquipmentDatabase")]
    public class EquipmentDatabase: ScriptableObject
    {
        public List<EquipmentItem> items;
    }
}