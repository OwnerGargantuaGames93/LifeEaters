using System.Collections.Generic;
using System.Linq;
using Boundary.Commands;
using Boundary.UI.StatueMenu;
using Control.Inventory;
using Data.Database;
using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.UI.Shop
{
    public class EssencesUIPanel: MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject contentPanel;

        // Celle della lista delle essenze attualmente equipaggiate.
        // Chiamate anche essence slots.
        private readonly List<EquippedEssenceMenuButton> _equippedEssenceButtons = new();
        
        // Celle della lista delle essenze disponibili per l'equipaggiamento dopo aver selezionato uno slot 
        private readonly List<AvailableEssenceMenuButton> _availableEssenceMenuButtons = new();
        
        // Lista di celle che rappresentano gli oggetti recuperabili dai tombini.
        // Nella fase di equipaggiamento dell'essenza, rappresenta gli oggetti generati da quell'essenza.
        // Nella fase di selezione di uno slot di essenza, rappresenta tutti gli oggetti recuperabili dai tombini.
        [SerializeField] private List<PitObjectSummaryPanel> pitObjectSummaryItems = new();
        
        [SerializeField] private EquippedEssenceMenuButton equippedEssenceButtonPrefab;
        [SerializeField] private AvailableEssenceMenuButton availableEssenceButtonPrefab;
        [SerializeField] private PitObjectSummaryPanel pitObjectPanelPrefab;
        
        // Contenitore delle celle delle essenze attualmente equipaggiate
        // Sono sempre 12 celle. Quindi non c'è bisogno di una scroll view.
        [SerializeField] private GameObject equippedEssencePanel;
        
        // Contenitore delle celle delle essenze disponibili per l'equipaggiamento
        // Può contenere un numero variabile di celle. Quindi serve una scroll view
        [SerializeField] private GameObject selectAvailableEssenceContent;
        [SerializeField] private GameObject selectAvailableEssencePanel;
        
        // Contenitore delle celle degli oggetti recuperabili dai tombini
        // Può contenere un numero variabile di celle. Quindi serve una scroll view
        [SerializeField] private GameObject pitObjectsSummaryPanel;
        
        private int _selectedEssenceSlotIndex = -1;
        private int _selectedAvailableEssenceIndex = -1;
        private EssenceType _selectedEssenceType = EssenceType.Standard;

        private bool _isMenuOpen;

        private EssenceMenuState _state = EssenceMenuState.ViewingEquippedEssences;
        
        private IEventBus _eventBus;
        private IInventoryModel _inventory;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _inventory = GameContext.Instance.Inventory;
            
            contentPanel.SetActive(false);
            _isMenuOpen = false;
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
            _eventBus.Subscribe<EOpenEssenceMenu>(OnOpen);
            _eventBus.Subscribe<EUpdateEquippedEssenceMenuButtonIndex>(OnUpdateEquippedEssenceMenuButtonIndex);
            _eventBus.Subscribe<EUpdateAvailableEssenceMenuButtonIndex>(OnUpdateAvailableEssenceMenuButtonIndex);
            _eventBus.Subscribe<EEssenceEquipped>(OnEssenceEquipped);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EOpenEssenceMenu>(OnOpen);
            _eventBus.Unsubscribe<EUpdateEquippedEssenceMenuButtonIndex>(OnUpdateEquippedEssenceMenuButtonIndex);
            _eventBus.Unsubscribe<EUpdateAvailableEssenceMenuButtonIndex>(OnUpdateAvailableEssenceMenuButtonIndex);
            _eventBus.Unsubscribe<EEssenceEquipped>(OnEssenceEquipped);
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
                Debug.Log("Confirm Called in EssencesUIPanel.");
                if (_state == EssenceMenuState.ViewingEquippedEssences)
                {
                    // Debug.Log($"[EssencesUIPanel] Switching to SelectingAvailableEssence. Selected slot: {_selectedEssenceSlotIndex}, type: {_selectedEssenceType}");
                    // Switch to selecting available essence state
                    _state = EssenceMenuState.SelectingAvailableEssence;
                    RefreshUI();
                }
                else if (_state == EssenceMenuState.SelectingAvailableEssence)
                {
                    // Equip the selected essence to the selected slot
                    var availableEssences = _inventory.GetAvailableEssences(_selectedEssenceType);
                    if (_selectedAvailableEssenceIndex >= 0 && _selectedAvailableEssenceIndex < availableEssences.Count)
                    {
                        var selectedEssence = availableEssences[_selectedAvailableEssenceIndex];
                        _eventBus.Publish(new EEssenceSelected(selectedEssence, _selectedEssenceSlotIndex));
                    }
                }
            }
        }
        
        private void OnEssenceEquipped(EEssenceEquipped e)
        {
            // After equipping an essence, return to viewing equipped essences state
            _state = EssenceMenuState.ViewingEquippedEssences;
            RefreshUI();
        }
        
        private void OnOpen(EOpenEssenceMenu e)
        {
            contentPanel.SetActive(true);
            
            _state = EssenceMenuState.ViewingEquippedEssences;
            
            RefreshUI();
            
            _isMenuOpen = true;
        }
        
        private void OnUpdateEquippedEssenceMenuButtonIndex(EUpdateEquippedEssenceMenuButtonIndex e)
        {
            _selectedEssenceSlotIndex = e.ChoiceIndex;
            _selectedEssenceType = _inventory.GetEssenceTypeByIndex(_selectedEssenceSlotIndex);
        }
        
        private void OnUpdateAvailableEssenceMenuButtonIndex(EUpdateAvailableEssenceMenuButtonIndex e)
        {
            _selectedAvailableEssenceIndex = e.ChoiceIndex;
            RefreshPitObjectsSummary();
        }

        private void RefreshUI()
        {
            if (_state == EssenceMenuState.ViewingEquippedEssences)
            {
                selectAvailableEssencePanel.SetActive(false);
                equippedEssencePanel.SetActive(true);
                RefreshEquippedEssences();
            }
            else
            {
                selectAvailableEssencePanel.SetActive(true);
                equippedEssencePanel.SetActive(false);
                
                RefreshAvailableEssences();
                RefreshPitObjectsSummary();
            } 
        }

        private void CloseMenu()
        {
            contentPanel.SetActive(false);
            _isMenuOpen = false;
            
            _eventBus.Publish(new EStatueSubMenuClosed());
        }

        private void RefreshEquippedEssences()
        {
            foreach (var button in _equippedEssenceButtons)
            {
                Destroy(button.gameObject);
            }
            _equippedEssenceButtons.Clear();
            
            var equippedEssences = _inventory.GetEquippedEssences();
            
            for (var i = 0; i < equippedEssences.Count; i++)
            {
                var equippedEssence = equippedEssences[i];
                var essenceButtonObj = Instantiate(equippedEssenceButtonPrefab, equippedEssencePanel.transform);
                var essenceButton = essenceButtonObj.GetComponent<EquippedEssenceMenuButton>();
                essenceButton.SetChoiceIndex(i);
                essenceButton.SetData(equippedEssence);
                _equippedEssenceButtons.Add(essenceButton);

                if (i == 0)
                {
                    essenceButton.SelectButton();
                    _eventBus.Publish(new EUpdateEquippedEssenceMenuButtonIndex(0));
                }
            }
        }

        private void RefreshAvailableEssences()
        {
            foreach (var button in _availableEssenceMenuButtons)
            {
                Destroy(button.gameObject);
            }
            _availableEssenceMenuButtons.Clear();
            
            var availableEssences = _inventory.GetAvailableEssences(_selectedEssenceType);
            
            for (var i = 0; i < availableEssences.Count; i++)
            {
                var availableEssence = availableEssences[i];
                
                var essenceButtonObj =
                    Instantiate(availableEssenceButtonPrefab, selectAvailableEssenceContent.transform);
                
                var essenceButton = essenceButtonObj.GetComponent<AvailableEssenceMenuButton>();
                essenceButton.SetChoiceIndex(i);
                essenceButton.SetData(availableEssence.item);
                
                _availableEssenceMenuButtons.Add(essenceButton);

                if (i == 0)
                {
                    essenceButton.SelectButton();
                    _eventBus.Publish(new EUpdateAvailableEssenceMenuButtonIndex(0));
                }
            }
        }
        
        private void RefreshPitObjectsSummary()
        {
            foreach (var pitObjectPanel in pitObjectSummaryItems)
            {
                Destroy(pitObjectPanel.gameObject);
            }
            pitObjectSummaryItems.Clear();

            var pitObjects = new List<PitObjectData>();
            if (_state == EssenceMenuState.ViewingEquippedEssences)
            {
                pitObjects.AddRange(_inventory.GetAllPitObjects());
            }
            else
            {
                // Use the selected essence type instead of hardcoded Standard
                var availableEssences = _inventory.GetAvailableEssences(_selectedEssenceType);
                if (_selectedAvailableEssenceIndex >= 0 && _selectedAvailableEssenceIndex < availableEssences.Count)
                {
                    var selectedEssence = availableEssences[_selectedAvailableEssenceIndex];
                    var essenceObjects = selectedEssence.item.objects;
                    pitObjects.AddRange(essenceObjects.Select(obj => DataSource.Instance.GetPitObject(obj)));
                }
            }
            
            foreach (var pitObject in pitObjects)
            {
                var pitObjectPanelObj = Instantiate(pitObjectPanelPrefab, pitObjectsSummaryPanel.transform);
                
                var pitObjectPanel = pitObjectPanelObj.GetComponent<PitObjectSummaryPanel>();
                pitObjectPanel.SetData(pitObject);

                pitObjectSummaryItems.Add(pitObjectPanel);
            }
        }
    }
    
    internal enum EssenceMenuState
    {
        ViewingEquippedEssences,
        SelectingAvailableEssence,
    }

    #region EOpenEssenceMenu

    public struct EOpenEssenceMenu {}

    #endregion
}