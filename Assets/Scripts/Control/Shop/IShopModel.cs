using System;
using Data.Entities.Game;

namespace Control.Shop
{
    public interface IShopModel: IDisposable
    {
        public bool IsShopOpen();
        public void Save(GameSessionData gameData);
    }
}