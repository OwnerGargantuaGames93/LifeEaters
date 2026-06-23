using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Entities.Item
{
    [Serializable]
    public class Item
    {
        /**
         * The name of the item.
         */
        public string Name;

        /**
         * Texts that describe the item.
         */
        public string Description;

        public string ShortDescription;
        public string GamePlayDescription;

        /**
         * Icon that will be displayed in the inventory. (maybe also in the game??)
         */
        public Sprite SmallIcon;

        public Sprite Icon;

        // Description used in shops to describe the item.
        // Is different from the main description because the item in shop doesn't show all details.
        // It can be different from npc to npc as well.
        public List<ShopItemDescription> ShopDescriptions;
    }
    
    [Serializable]
    public class ShopItemDescription
    {
        public string ShopName;

        public string Description;
    }
}

