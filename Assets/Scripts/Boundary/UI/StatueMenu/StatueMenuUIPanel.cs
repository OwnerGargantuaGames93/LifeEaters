using System.Collections;
using System.Collections.Generic;
using Boundary.Commands;
using Boundary.Interactable;
using Boundary.UI.Dialogue;
using Boundary.UI.Shop;
using Boundary.UI.StatueMenu.EquipmentMenu;
using Boundary.UI.StatueMenu.SaveMenu;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.UI
{
    public class StatueMenuUIPanel: MonoBehaviour
    {
        private IEventBus _eventBus;
        
        [Header("UI Components")]
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private List<StatueMenuButton> statueMenuButtons;
        
        private int _currentSelectedButtonIndex = -1;
        
        // -1 means no menu is open, 0: save, 1: equipments, 2: essences, 3: level up, 4: teleport (disabled)
        private int _currentStatueMenuOpen = -1;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            
            contentPanel.SetActive(false);
            ResetPanel();
            
            var index = 0;
            foreach (var statueMenuButton in statueMenuButtons)
            {
                statueMenuButton.SetChoiceIndex(index);
                index++;
            }
        }

        private void Update()
        {
            // If any other statue menu is open, do not process input here
            if (_currentStatueMenuOpen >= 0)
            {
                return;
            }
            
            if (UserInput.instance.CloseStatueMenuWasPressedThisFrame() || UserInput.instance.StatueMenuGoBackWasPressedThisFrame())
            {
                CloseMenu();
            }
            
            if (UserInput.instance.StatueMenuConfirmWasPressedThisFrame())
            {
                switch (_currentSelectedButtonIndex)
                {
                    case 0:
                    {
                        StartCoroutine(OpenSaveGameMenu());
                        break;
                    }
                    case 1:
                    {
                        StartCoroutine(OpenEquipmentMenu());
                        break;
                    }
                    case 2:
                    {
                        StartCoroutine(OpenEssenceMenu());
                        break;
                    }
                    case 3:
                    {
                        StartCoroutine(OpenLevelUpMenu());
                        break;
                    }
                    default:
                    {
                        Debug.LogWarning("StatueMenuUIPanel: Menu option not implemented yet!");
                        break;
                    }
                }
            }
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
            _eventBus.Subscribe<EPlayerRestOnStatue>(OnStatueMenuOpened);
            _eventBus.Subscribe<EStatueSubMenuClosed>(OnStatueSubMenuClosed);
            _eventBus.Subscribe<EUpdateStatueMenuButtonIndex>(OnUpdateStatueMenuButtonIndex);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EPlayerRestOnStatue>(OnStatueMenuOpened);
            _eventBus.Unsubscribe<EStatueSubMenuClosed>(OnStatueSubMenuClosed);
            _eventBus.Unsubscribe<EUpdateStatueMenuButtonIndex>(OnUpdateStatueMenuButtonIndex);
        }
        
        private void OnStatueMenuOpened(EPlayerRestOnStatue e)
        {
            contentPanel.SetActive(true);
            
            // Select the first button by default
            if (statueMenuButtons.Count <= 0)
            {
                Debug.LogError("StatueMenuUIPanel: No statue menu buttons found!");
                return;
            }
            
            statueMenuButtons[0].SelectButton();
            _currentSelectedButtonIndex = 0;
        }
        
        private void OnStatueSubMenuClosed(EStatueSubMenuClosed e)
        {
            _currentStatueMenuOpen = -1;
            
            // Enable statue menu buttons
            foreach (var button in statueMenuButtons)
            {
                button.MakeResponsive();
            }
            
            // Select the first button by default
            if (statueMenuButtons.Count <= 0)
            {
                Debug.LogError("StatueMenuUIPanel: No statue menu buttons found!");
                return;
            }
            
            statueMenuButtons[0].SelectButton();
            _currentSelectedButtonIndex = 0;
        }
        
        private void OnUpdateStatueMenuButtonIndex(EUpdateStatueMenuButtonIndex e)
        {
            _currentSelectedButtonIndex = e.ButtonIndex;
        }

        private void ResetPanel()
        {
            _currentSelectedButtonIndex = -1;
        }
        
        private void CloseMenu()
        {
            StartCoroutine(CloseMenuCoroutine());
        }

        private IEnumerator CloseMenuCoroutine()
        {
            yield return null;
            
            contentPanel.SetActive(false);
            ResetPanel();
            
            _eventBus.Publish(new EStatueMenuClosed());
        }
        
        private IEnumerator OpenLevelUpMenu()
        {
            yield return null;
            
            _currentStatueMenuOpen = 3;
            _currentSelectedButtonIndex = -1;
            
            // Disable statue menu buttons
            foreach (var button in statueMenuButtons)
            {
                button.MakeUnresponsive();
            }
            
            _eventBus.Publish(new EOpenLevelUpMenu());
        }

        private IEnumerator OpenSaveGameMenu()
        {
            yield return null;
            
            _currentStatueMenuOpen = 0;
            _currentSelectedButtonIndex = -1;
            
            // Disable statue menu buttons
            foreach (var button in statueMenuButtons)
            {
                button.MakeUnresponsive();
            }
            
            _eventBus.Publish(new EOpenSaveGameMenu());
        }

        private IEnumerator OpenEssenceMenu()
        {
            yield return null;
            
            _currentStatueMenuOpen = 2;
            _currentSelectedButtonIndex = -1;
            
            // Disable statue menu buttons
            foreach (var button in statueMenuButtons)
            {
                button.MakeUnresponsive();
            }
            
            _eventBus.Publish(new EOpenEssenceMenu());
        }
        
        private IEnumerator OpenEquipmentMenu()
        {
            yield return null;
            
            _currentStatueMenuOpen = 1;
            _currentSelectedButtonIndex = -1;
            
            // Disable statue menu buttons
            foreach (var button in statueMenuButtons)
            {
                button.MakeUnresponsive();
            }
            
            _eventBus.Publish(new EOpenEquipmentMenu());
        }
    }
    
    #region Events

    public struct EUpdateStatueMenuButtonIndex
    {
        public int ButtonIndex;

        public EUpdateStatueMenuButtonIndex(int buttonIndex)
        {
            ButtonIndex = buttonIndex;
        }
    }
    
    public struct EStatueMenuClosed
    {
    }

    public struct EOpenLevelUpMenu
    {
    }
    
    public struct EStatueSubMenuClosed
    {
    }
    
    #endregion
}