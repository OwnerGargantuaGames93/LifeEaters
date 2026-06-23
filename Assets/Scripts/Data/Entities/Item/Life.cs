using System;
using System.Collections.Generic;
using Data.Entities.Effects;
using UnityEngine;

namespace Data.Entities.Item
{
    [Serializable]
    public class LifeItem: Item
    {
        public LifeId id;
        
        public List<EffectData> effects;
        
        public EssenceId associatedEssence;
    }
    
    public enum LifeId
    {
        ClayOperatorLife,
        WheelFitter, // Aka Mechanical
        ShepardLife,
        Life,
        PrematureLife,
        SectSoldierLife,
        BlueCultLeaderLife
    }
}