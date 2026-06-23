using Boundary.Commands;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.UI.GameMenu
{
    public class GameMenuPanelUI : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private GameMenuTabButton inventoryTab;
        [SerializeField] private GameMenuTabButton statusTab;
        [SerializeField] private GameMenuTabButton loadGameTab;
        [SerializeField] private GameMenuTabButton settingsTab;
        
        private GameSubMenu _currentSelectedSubMenu = GameSubMenu.Inventory;
        
        private IGameMenuHandler _gameMenuHandler;
        private IEventBus _eventBus;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _gameMenuHandler = GameContext.Instance.GameMenuHandler;
        }
        
        private void Start()
        {
            contentPanel.SetActive(false);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EOpenGameMenu>(OnGameMenuOpened);
            _eventBus.Subscribe<ECloseGameMenu>(OnGameMenuClosed);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EOpenGameMenu>(OnGameMenuOpened);
            _eventBus.Unsubscribe<ECloseGameMenu>(OnGameMenuClosed);
        }
        
        private void OnGameMenuOpened(EOpenGameMenu eOpenGameMenu)
        {
            contentPanel.SetActive(true);
            
            RefreshUI();
            OpenSelectedSubMenu();
        }
        
        private void OnGameMenuClosed(ECloseGameMenu eCloseGameMenu)
        {
            contentPanel.SetActive(false);
            _currentSelectedSubMenu = GameSubMenu.Inventory;
        }

        private void Update()
        {
            if (!_gameMenuHandler.IsGameMenuOpen()) return;
            
            if (UserInput.instance.GameMenuNextTabWasPressedThisFrame())
            {
                SelectNextSubMenu();
            }

            if (UserInput.instance.GameMenuPreviousTabWasPressedThisFrame())
            {
                SelectPreviousSubMenu();
            }
        }

        private void SelectNextSubMenu()
        {
            switch (_currentSelectedSubMenu)
            {
                case GameSubMenu.Inventory:
                    _currentSelectedSubMenu = GameSubMenu.Status;
                    break;
                case GameSubMenu.Status:
                    _currentSelectedSubMenu = GameSubMenu.LoadGame;
                    break;
                case GameSubMenu.LoadGame:
                    _currentSelectedSubMenu = GameSubMenu.Settings;
                    break;
                case GameSubMenu.Settings:
                    _currentSelectedSubMenu = GameSubMenu.Inventory;
                    break;
                default:
                    Debug.LogError("Invalid sub menu selected");
                    break; 
            }

            RefreshUI();
            OpenSelectedSubMenu();
        }
        
        private void SelectPreviousSubMenu()
        {
            switch (_currentSelectedSubMenu)
            {
                case GameSubMenu.Inventory:
                    _currentSelectedSubMenu = GameSubMenu.Settings;
                    break;
                case GameSubMenu.Status:
                    _currentSelectedSubMenu = GameSubMenu.Inventory;
                    break;
                case GameSubMenu.LoadGame:
                    _currentSelectedSubMenu = GameSubMenu.Status;
                    break;
                case GameSubMenu.Settings:
                    _currentSelectedSubMenu = GameSubMenu.LoadGame;
                    break;
                default:
                    Debug.LogError("Invalid sub menu selected");
                    break; 
            }

            RefreshUI();
            OpenSelectedSubMenu();
        }

        private void OpenSelectedSubMenu()
        {
            _eventBus.Publish(new EShowGameSubMenu(_currentSelectedSubMenu));
        }

        private void RefreshUI()
        {
            // Colora il tab del menu selezionato in modo diverso dagli altri
            inventoryTab.SetSelection(_currentSelectedSubMenu == GameSubMenu.Inventory);
            statusTab.SetSelection(_currentSelectedSubMenu == GameSubMenu.Status);
            loadGameTab.SetSelection(_currentSelectedSubMenu == GameSubMenu.LoadGame);
            settingsTab.SetSelection(_currentSelectedSubMenu == GameSubMenu.Settings);
        }
    }
    
    #region Events

    public struct EShowGameSubMenu
    {
        public GameSubMenu SubMenu;
        
        public EShowGameSubMenu(GameSubMenu subMenu)
        {
            SubMenu = subMenu;
        }
    }
    
    #endregion
    
    public enum GameSubMenu
    {
        Inventory,
        Status,
        LoadGame,
        Settings
    }
}


