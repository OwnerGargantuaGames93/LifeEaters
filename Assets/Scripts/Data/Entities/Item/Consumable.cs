using System;
using System.Collections.Generic;
using Data.Entities.Effects;

namespace Data.Entities.Item
{
    [Serializable]
    public class ConsumableItem: Item
    {
        public ConsumableId Id;
        
        public List<EffectData> Effects;
    }

    public enum ConsumableId
    {
        HeartLeaf,
        SmallVioletClump,
        VioletClump,
        LargeVioletClump,
        GiantVioletClump,
        PopjuBalm,
        Battery,
        FriendPostcard,
        CannedFood,
        PsiWaveGenerator,
        MagicFormula,
        WelcomeCocktail,
        CrustaceanSoup
    }
}