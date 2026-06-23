namespace Data.Entities.Collectibles
{
    public class BonusCollectibleData
    {
        public string Id { get; }
        private int CustomPointsAmount { get; }
        private BonusType Type { get; }

        public int PointsAmount => CustomPointsAmount == 0 ? GetPointsAmountFromType() : CustomPointsAmount;

        public BonusCollectibleData(BonusType type)
        {
            Type = type;
            CustomPointsAmount = 0;
        }

        public BonusCollectibleData(BonusType type, int customPointsAmount, string id)
        {
            Id = id;
            Type = type;
            CustomPointsAmount = customPointsAmount;
        }

        private int GetPointsAmountFromType()
        {
            return Type switch
            {
                BonusType.WaterMelon => 40,
                BonusType.Mandarin => 30,
                BonusType.Prune => 20,
                BonusType.Cherry => 10,
                BonusType.AceOfHearts => 100,
                BonusType.AceOfDiamonds => 100,
                BonusType.AceOfFlowers => 100,
                BonusType.AceOfSpades => 100,
                BonusType.FourLeafClover => 200,
                BonusType.Diamond => 500,
                BonusType.Bar => 300,
                BonusType.Seven => 700,
                _ => 0
            };
        }
    }

    public enum BonusType
    {
        WaterMelon,
        Mandarin,
        Cherry,
        Prune,
        Diamond,
        AceOfHearts,
        AceOfDiamonds,
        AceOfFlowers,
        AceOfSpades,
        FourLeafClover,
        Bar,
        Seven
    }
}