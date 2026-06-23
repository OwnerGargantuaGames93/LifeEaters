using System.Collections.Generic;
using Boundary.Commands;
using Control.GameData;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.UI.StatueMenu.SaveMenu
{
    public class SaveGamePanelUI: MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private SaveSlotButton saveSlotButtonPrefab;
        [SerializeField] private GameObject saveSlotsListContent;
        
        private List<GameObject> _saveSlotButtons = new();
        private SaveSlot _currentSaveSlot = null;
        
        private bool _isMenuOpen = false;
        
        private IEventBus _eventBus;
        private IGameDataModel _gameData;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _gameData = GameContext.Instance.GameData;
            
            contentPanel.SetActive(false);
            _isMenuOpen = false;

            ResetData();
            ResetPanel();
        }
        
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
            
            ResetData();
            ResetPanel();
        }

        private void Update()
        {
            if (!_isMenuOpen) return;
            
            if (UserInput.instance.StatueMenuGoBackWasPressedThisFrame())
            {
                CloseMenu();
            }
            
            if (UserInput.instance.StatueMenuConfirmWasPressedThisFrame())
            {
                OnSaveConfirmed();
            }
        }
        
        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EOpenSaveGameMenu>(OnSaveGameMenuOpened);
            _eventBus.Subscribe<EGameDataSaved>(OnGameSaved);
            _eventBus.Subscribe<EUpdateSaveSlotSelectedIndex>(OnUpdateSaveSlotSelectedIndex);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EOpenSaveGameMenu>(OnSaveGameMenuOpened);
            _eventBus.Unsubscribe<EGameDataSaved>(OnGameSaved);
            _eventBus.Unsubscribe<EUpdateSaveSlotSelectedIndex>(OnUpdateSaveSlotSelectedIndex);
        }

        private void OnSaveGameMenuOpened(EOpenSaveGameMenu menu)
        {
            contentPanel.SetActive(true);
            _isMenuOpen = true;
            
            var currentSaveSlots = _gameData.SaveSlots;
            
            _saveSlotButtons = new List<GameObject>();
            for (var i = 0; i < currentSaveSlots.Count; i++)
            {
                var saveSlot = currentSaveSlots[i];
                var saveSlotButtonObj = Instantiate(saveSlotButtonPrefab.gameObject, saveSlotsListContent.transform);
                var saveSlotButton = saveSlotButtonObj.GetComponent<SaveSlotButton>();
                saveSlotButton.SetData(saveSlot);
                saveSlotButton.SetSelectedItemIndex(i);
                
                // Select the first item by default
                if (i == 0)
                {
                    saveSlotButton.SelectButton();
                    _eventBus.Publish(new EUpdateSaveSlotSelectedIndex(0));
                }
                
                _saveSlotButtons.Add(saveSlotButtonObj);
            }
        }

        private void OnUpdateSaveSlotSelectedIndex(EUpdateSaveSlotSelectedIndex saveSlotSelectedIndex)
        {
            var currentSaveSlots = _gameData.SaveSlots;
            var index = saveSlotSelectedIndex.SelectedIndex;
            if (index < 0 || index >= currentSaveSlots.Count)
            {
                _currentSaveSlot = null;
                return;
            }

            _currentSaveSlot = currentSaveSlots[index];
        }
        
        private void OnGameSaved(EGameDataSaved e)
        {
            CloseMenu();
        }

        private void OnSaveConfirmed()
        {
            if (_currentSaveSlot == null)
            {
                Debug.LogError("SaveGamePanelUI: No save slot selected!");
                return;
            }
            
            _eventBus.Publish(new ESaveGameDataUiCommand(_currentSaveSlot.SlotNumber));
        }

        private void CloseMenu()
        {
            contentPanel.SetActive(false);
            _isMenuOpen = false;

            ResetData();
            ResetPanel();
            
            _eventBus.Publish(new EStatueSubMenuClosed());
        }

        private void ResetData()
        {
            _currentSaveSlot = null;
        }
        
        private void ResetPanel()
        {
            foreach (var item in _saveSlotButtons)
            {
                Destroy(item);
            }

            _saveSlotButtons.Clear();
        }
        
        
    }
    
    #region Events
    public struct EOpenSaveGameMenu
    {
    }

    public struct ESaveGameDataUiCommand
    {
        public readonly int SlotNumber;

        public ESaveGameDataUiCommand(int slotNumber)
        {
            SlotNumber = slotNumber;
        }
    }
    #endregion
}