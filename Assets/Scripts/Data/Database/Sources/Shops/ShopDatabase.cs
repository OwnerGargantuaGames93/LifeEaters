using System.Collections.Generic;
using Data.Entities.Shops;
using UnityEngine;

namespace Data.Database.Sources.Shops
{
    [CreateAssetMenu(fileName = "ShopDatabase", menuName = "GameData/ShopDatabase")]
    public class ShopDatabase: ScriptableObject
    {
        public List<Shop> shops;
    }
}