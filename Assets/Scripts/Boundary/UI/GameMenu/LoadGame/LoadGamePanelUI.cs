using System;
using System.Collections.Generic;
using Boundary.Commands;
using Boundary.UI.StatueMenu.SaveMenu;
using Control.GameData;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.UI.GameMenu.LoadGame
{
    public class LoadGamePanelUI: MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private SaveSlotButton saveSlotButtonPrefab;
        [SerializeField] private GameObject saveSlotButtonListContent;
        
        private IEventBus _eventBus;
        private IGameDataModel _gameData;
        private IGameMenuHandler _gameMenuHandler;

        private bool _isOpen;
        private SaveSlot _selectedSaveSlot;
        private readonly List<SaveSlotButton> _saveSlotButtons = new();

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;
            _gameMenuHandler = GameContext.Instance.GameMenuHandler;
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeToEvents();
        }

        private void Update()
        {
            if (!_isOpen) return;

            if (UserInput.instance.GameMenuConfirmWasPressedThisFrame())
            {
                OnConfirm();
            }
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EShowGameSubMenu>(OnSubMenuOpen);
            _eventBus.Subscribe<ECloseGameMenu>(OnGameMenuClosed);
            _eventBus.Subscribe<EUpdateSaveSlotSelectedIndex>(OnUpdateSaveSlotSelectedIndex);
        }
        
        private void UnsubscribeToEvents()
        {
            _eventBus.Unsubscribe<EShowGameSubMenu>(OnSubMenuOpen);
            _eventBus.Unsubscribe<ECloseGameMenu>(OnGameMenuClosed);
            _eventBus.Unsubscribe<EUpdateSaveSlotSelectedIndex>(OnUpdateSaveSlotSelectedIndex);
        }

        private void OnSubMenuOpen(EShowGameSubMenu e)
        {
            if (e.SubMenu == GameSubMenu.LoadGame)
            {
                contentPanel.SetActive(true);
                _isOpen = true;
                RefreshUI();
            }
            else if (_isOpen)
            {
                _isOpen = false;
                ResetPanel();
            }
        }

        private void OnUpdateSaveSlotSelectedIndex(EUpdateSaveSlotSelectedIndex e)
        {
            var saveSlots = _gameData.LoadableSaveSlots;
            _selectedSaveSlot = saveSlots[e.SelectedIndex];
        }
        
        private void OnGameMenuClosed(ECloseGameMenu eCloseGameMenu)
        {
            _isOpen = false;
            ResetPanel();
        }

        private void ResetPanel()
        {
            contentPanel.SetActive(false);
            
            foreach (var saveSlotButton in _saveSlotButtons)
            {
                Destroy(saveSlotButton.gameObject);
            }
            _saveSlotButtons.Clear();
            _selectedSaveSlot = null;
        }

        private void RefreshUI()
        {
            var saveSlots = _gameData.LoadableSaveSlots;
            
            for (var i = 0; i < saveSlots.Count; i++)
            {
                var saveSlot = saveSlots[i];
                var saveSlotButton = Instantiate(saveSlotButtonPrefab, saveSlotButtonListContent.transform);
                var position = saveSlotButton.transform.position;
                saveSlotButton.gameObject.transform.position = new Vector3(position.x, position.y, 0);
                var saveSlotButtonComponent = saveSlotButton.GetComponent<SaveSlotButton>();
                saveSlotButtonComponent.SetSelectedItemIndex(i);
                saveSlotButtonComponent.SetData(saveSlot);

                if (i == 0)
                {
                    saveSlotButtonComponent.SelectButton();
                    _eventBus.Publish(new EUpdateSaveSlotSelectedIndex(i));
                }
                
                _saveSlotButtons.Add(saveSlotButton);
            }
        }

        private void OnConfirm()
        {
            if (_selectedSaveSlot == null)
            {
                Debug.LogWarning("No save slot selected");
                return;
            }
            
            var slotNumber = _selectedSaveSlot.SlotNumber;
            _eventBus.Publish(new EGameDataToLoadSelected(slotNumber));
            
            _gameMenuHandler.ToggleMenu();
        }
    }
    
    #region Events

    public struct EGameDataToLoadSelected
    {
        public readonly int SlotNumber;

        public EGameDataToLoadSelected(int slotNumber)
        {
            SlotNumber = slotNumber;
        }
    }

    #endregion
}