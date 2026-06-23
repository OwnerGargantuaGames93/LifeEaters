using System.Collections.Generic;
using Boundary.Commands;
using Control.Inventory;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Boundary.UI.GameMenu
{
    public class InventoryPanelUI: MonoBehaviour
    {
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private InventoryItemButton inventoryItemPrefab;
        [SerializeField] private GameObject inventoryListContent;
        [SerializeField] private Image itemDetailImage;
        [SerializeField] private TextMeshProUGUI itemDetailText;
        
        // TODO: Filter using tab
        
        private List<InventoryItemButton> _inventoryItemButtons = new();
        private InventoryItemSlot _selectedInventoryItemSlot;
        
        private IEventBus _eventBus;
        private IInventoryModel _inventoryModel;
        private IGameMenuHandler _gameMenuHandler;

        private bool _isOpen = false;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _inventoryModel = GameContext.Instance.Inventory;
            _gameMenuHandler = GameContext.Instance.GameMenuHandler;
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
            _eventBus.Subscribe<EShowGameSubMenu>(OnGameSubMenuSelected);
            _eventBus.Subscribe<ECloseGameMenu>(OnGameMenuClosed);
            _eventBus.Subscribe<EUpdateInventoryItemIndex>(OnInventoryButtonSelected);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EShowGameSubMenu>(OnGameSubMenuSelected);
            _eventBus.Unsubscribe<ECloseGameMenu>(OnGameMenuClosed);
            _eventBus.Unsubscribe<EUpdateInventoryItemIndex>(OnInventoryButtonSelected);
        }

        private void OnInventoryButtonSelected(EUpdateInventoryItemIndex e)
        {
            var i = e.ItemIndex;

            var items = _inventoryModel.GetItemSlots();
            var selectedItem = items[i];

            if (selectedItem == null) return;

            itemDetailImage.sprite = selectedItem.Icon;
            itemDetailText.text = $"{selectedItem.Description}\n{selectedItem.GamePlayDescription}";
            
            _selectedInventoryItemSlot = selectedItem;
        }
        
        private void OnGameSubMenuSelected(EShowGameSubMenu eShowGameSubMenu)
        {
            if (eShowGameSubMenu.SubMenu == GameSubMenu.Inventory)
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
        
        private void OnGameMenuClosed(ECloseGameMenu eCloseGameMenu)
        {
            _isOpen = false;
            ResetPanel();
        }

        private void Update()
        {
            if (!_isOpen) return;
            
            if (UserInput.instance.GameMenuConfirmWasPressedThisFrame())
            {
                OnConfirmPressed();
            }
        }

        // TODO: Add a modal to choose quantity to use
        private void OnConfirmPressed()
        {
            if (_selectedInventoryItemSlot == null)
            {
                Debug.LogWarning("No item selected");
                return;
            }

            if (_selectedInventoryItemSlot.SlotType != InventoryItemSlotType.CONSUMABLE &&
                _selectedInventoryItemSlot.SlotType != InventoryItemSlotType.LIFE)
            {
                Debug.LogWarning("Selected item is not usable");
                return;
            }

            
            _eventBus.Publish(new EItemUsed(_selectedInventoryItemSlot, 1));
            _gameMenuHandler.ToggleMenu();
        }

        private void ResetPanel()
        {
            // Clear selection and details
            foreach (var button in _inventoryItemButtons) 
            {
                Destroy(button.gameObject);
            }
            _inventoryItemButtons.Clear();
            itemDetailImage.sprite = null;
            itemDetailText.text = "";
            
            contentPanel.SetActive(false);
        }

        private void RefreshUI()
        {
            // TODO: Filter items based on selected tab
            var data = _inventoryModel.GetItemSlots();
            
            var index = 0;
            foreach (var d in data)
            {
                var button = Instantiate(inventoryItemPrefab, inventoryListContent.transform);
                _inventoryItemButtons.Add(button);

                var inventoryItemButton = button.GetComponent<InventoryItemButton>();
                inventoryItemButton.SetIndex(index);
                
                inventoryItemButton.SetData(d);
                
                if (index == 0)
                {
                    inventoryItemButton.SelectButton();
                    _eventBus.Publish(new EUpdateInventoryItemIndex(0));
                }
                index++;
            }
        }
    }
}