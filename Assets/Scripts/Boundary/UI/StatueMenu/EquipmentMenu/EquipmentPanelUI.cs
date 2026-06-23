using System;
using System.Collections.Generic;
using Boundary.Commands;
using Control.Inventory;
using Data.Entities.Item;
using Infra.EventBus;
using UnityEngine;

namespace Boundary.UI.StatueMenu.EquipmentMenu
{
    public class EquipmentPanelUI: MonoBehaviour
    {
        [Header("Components")]
        
        [SerializeField] private EquipmentSlotButton bodyEquipmentSlot;
        [SerializeField] private EquipmentSlotButton acc1EquipmentSlot;
        [SerializeField] private EquipmentSlotButton acc2EquipmentSlot;
        [SerializeField] private EquipmentSlotButton acc3EquipmentSlot;
        [SerializeField] private AvailableEquipmentItemButton equipmentItemPrefab;
        [SerializeField] private GameObject availableEquipmentListContent;

        [SerializeField] private GameObject contentPanel;
        
        private List<AvailableEquipmentItemButton> _equipmentItemButtons = new();
        private int _currentEquipmentSlot = -1;
        private InventoryEquipmentItem _currentSelectedAvailableEquipment = null;
        
        private IEventBus _eventBus;
        private IInventoryModel _inventoryModel;
        
        private bool _isMenuOpen;

        private EquipmentMenuState _state = EquipmentMenuState.SelectEquipmentSlot;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _inventoryModel = GameContext.Instance.Inventory;
            
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
            _eventBus.Subscribe<EOpenEquipmentMenu>(OnOpenEquipmentMenu);
            _eventBus.Subscribe<EUpdateEquipmentSlotIndex>(OnUpdateEquipmentSlotIndex);
            _eventBus.Subscribe<EUpdateAvailableEquipmentSlotIndex>(OnUpdateAvailableEquipmentSlotIndex);
            _eventBus.Subscribe<EItemsEquipped>(OnItemEquipped);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EOpenEquipmentMenu>(OnOpenEquipmentMenu);
            _eventBus.Unsubscribe<EUpdateEquipmentSlotIndex>(OnUpdateEquipmentSlotIndex);
            _eventBus.Unsubscribe<EUpdateAvailableEquipmentSlotIndex>(OnUpdateAvailableEquipmentSlotIndex);
            _eventBus.Unsubscribe<EItemsEquipped>(OnItemEquipped);
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
                OnConfirm();
            }
        }
        
        private void OnOpenEquipmentMenu(EOpenEquipmentMenu e)
        {
            contentPanel.SetActive(true);
            _isMenuOpen = true;

            _state = EquipmentMenuState.SelectEquipmentSlot;
            RefreshUI();
        }

        private void OnConfirm()
        {
            if (_state == EquipmentMenuState.SelectEquipmentSlot)
            {
                // Change state to SelectAvailableEquipment
                _state = EquipmentMenuState.SelectAvailableEquipment;
                RefreshUI();
            }
            else if (_state == EquipmentMenuState.SelectAvailableEquipment)
            {
                // Equip the selected equipment
                if (_currentSelectedAvailableEquipment != null)
                {
                    var equipmentSlot = new InventoryItemSlot(_currentSelectedAvailableEquipment);
                    var equipmentSlotType = _currentEquipmentSlot switch
                    {
                        0 => EquipmentSlot.Body,
                        1 => EquipmentSlot.Acc1,
                        2 => EquipmentSlot.Acc2,
                        3 => EquipmentSlot.Acc3,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    _eventBus.Publish(new EEquipmentSelected(equipmentSlot, equipmentSlotType));
                }
            }
        }
        
        private void OnItemEquipped(EItemsEquipped e)
        {
            ResetPanel();
            _state = EquipmentMenuState.SelectEquipmentSlot;
            RefreshUI();
        }
        
        private void OnUpdateEquipmentSlotIndex(EUpdateEquipmentSlotIndex e)
        {
            var index = e.SelectedIndex;
            _currentEquipmentSlot = index;
        }
        
        private void OnUpdateAvailableEquipmentSlotIndex(EUpdateAvailableEquipmentSlotIndex e)
        {
            var index = e.SelectedIndex;
            var type = _currentEquipmentSlot == 0 ? EquipmentType.Set : EquipmentType.Accessory;
            var itemSlots = _inventoryModel.GetEquipmentItems(type);
            _currentSelectedAvailableEquipment = itemSlots[index];
        }

        private void ResetPanel()
        {
            foreach (var button in _equipmentItemButtons)
            {
                Destroy(button.gameObject);
            }
            _equipmentItemButtons.Clear();

            _currentEquipmentSlot = -1;
            _currentSelectedAvailableEquipment = null;
        }

        private void RefreshUI()
        {
            if (_state == EquipmentMenuState.SelectEquipmentSlot)
            {
                // Pulisci la lista degli equipaggiamenti disponibili
                foreach (var button in _equipmentItemButtons)
                {
                    Destroy(button.gameObject);
                }
                _equipmentItemButtons.Clear();
                
                RefreshEquipmentSlotUI();
            }
            else
            {
                bodyEquipmentSlot.SetUnresponsive();
                acc1EquipmentSlot.SetUnresponsive();
                acc2EquipmentSlot.SetUnresponsive();
                acc3EquipmentSlot.SetUnresponsive();

                RefreshAvailableEquipmentsUI();
            }
        }

        private void RefreshEquipmentSlotUI()
        {
            var bodyInventoryItem = _inventoryModel.Body;
            bodyEquipmentSlot.SetResponsive();
            bodyEquipmentSlot.SetSelectedItemIndex(0);
            bodyEquipmentSlot.SetData(bodyInventoryItem);
            bodyEquipmentSlot.SelectButton();
            _eventBus.Publish(new EUpdateEquipmentSlotIndex(0));
            
            var acc1InventoryItem = _inventoryModel.Acc1;
            acc1EquipmentSlot.SetResponsive();
            acc1EquipmentSlot.SetSelectedItemIndex(1);
            acc1EquipmentSlot.SetData(acc1InventoryItem);
            
            var acc2InventoryItem = _inventoryModel.Acc2;
            acc2EquipmentSlot.SetResponsive();
            acc2EquipmentSlot.SetSelectedItemIndex(2);
            acc2EquipmentSlot.SetData(acc2InventoryItem);
            
            var acc3InventoryItem = _inventoryModel.Acc3;
            acc3EquipmentSlot.SetResponsive();
            acc3EquipmentSlot.SetSelectedItemIndex(2);
            acc3EquipmentSlot.SetData(acc3InventoryItem);
        }

        private void RefreshAvailableEquipmentsUI()
        {
            var type = _currentEquipmentSlot == 0 ? EquipmentType.Set : EquipmentType.Accessory;
            var itemSlots = _inventoryModel.GetEquipmentItems(type);
            
            for (var i = 0; i < itemSlots.Count; i++)
            {
                var equipmentItem = itemSlots[i];
                var equipmentItemButton = Instantiate(equipmentItemPrefab.gameObject, availableEquipmentListContent.transform);
                var button = equipmentItemButton.GetComponent<AvailableEquipmentItemButton>();
                button.SetData(equipmentItem);
                button.SetSelectedItemIndex(i);
                
                // Select the first item by default
                if (i == 0)
                {
                    button.SelectButton();
                    _eventBus.Publish(new EUpdateAvailableEquipmentSlotIndex(0));
                }

                _equipmentItemButtons.Add(button);
            }
        }

        private void CloseMenu()
        {
            contentPanel.SetActive(false);
            _isMenuOpen = false;

            ResetPanel();
            
            _eventBus.Publish(new EStatueSubMenuClosed());
        }
    }

    public enum EquipmentMenuState
    {
        SelectEquipmentSlot,
        SelectAvailableEquipment,
    }

    public enum EquipmentSlot
    {
        Body,
        Acc1,
        Acc2,
        Acc3,
    }
    
    #region Events
    public struct EOpenEquipmentMenu
    {
    }

    public struct EEquipmentSelected
    {
        public readonly InventoryItemSlot SelectedEquipment;
        public readonly EquipmentSlot Slot;
        
        public EEquipmentSelected(InventoryItemSlot selectedEquipment, EquipmentSlot slot)
        {
            SelectedEquipment = selectedEquipment;
            Slot = slot;
        }
    }
    
    public struct EEquipmentRemoved
    {
        public readonly InventoryItemSlot SelectedEquipment;
        public readonly EquipmentSlot Slot;
        
        public EEquipmentRemoved(InventoryItemSlot selectedEquipment, EquipmentSlot slot)
        {
            SelectedEquipment = selectedEquipment;
            Slot = slot;
        }
    }
    
    
    #endregion
}