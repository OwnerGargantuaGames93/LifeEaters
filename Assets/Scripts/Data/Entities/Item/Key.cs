using System;

namespace Data.Entities.Item
{
    [Serializable]
    public class KeyItem : Item
    {
        public KeyId Id;
    }
    
    public enum KeyId
    {
        HouseKey,
        SistersLetter,
        FridgeKey,
        SisterRoomKey,
        FloweredTowerKey,
        BridgeControlRoomKey,
        CarriageNo2Key,
        ClownPrisonKey,
        HolProgramGroup,
        HoeghCultWesternOutpostKey,
    }
}