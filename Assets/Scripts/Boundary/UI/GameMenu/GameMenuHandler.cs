using Infra.EventBus;

namespace Boundary.UI.GameMenu
{
    public class GameMenuHandler: IGameMenuHandler
    {
        private IEventBus _eventBus;
        
        private bool _isGameMenuOpen;
        
        public GameMenuHandler(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void ToggleMenu()
        {
            _isGameMenuOpen = !_isGameMenuOpen;
            
            if (_isGameMenuOpen)
            {
                _eventBus.Publish(new EOpenGameMenu());
            }
            else
            {
                _eventBus.Publish(new ECloseGameMenu());
            }
        }

        public bool IsGameMenuOpen()
        {
            return _isGameMenuOpen;
        }
    }
    
    #region Events
    
    public struct EOpenGameMenu {}
    
    public struct ECloseGameMenu {}
    
    #endregion
}