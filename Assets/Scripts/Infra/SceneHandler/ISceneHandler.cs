using Data.Entities.Game;

namespace Infra.SceneHandler
{
    public interface ISceneHandler
    {
        public void Save(GameSessionData gameData);
    }
}