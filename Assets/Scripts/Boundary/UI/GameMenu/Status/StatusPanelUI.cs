using System.Collections.Generic;
using System.Linq;
using Boundary.UI.StatueMenu.EquipmentMenu;
using Control.Inventory;
using Control.Player;
using Infra.EventBus;
using TMPro;
using UnityEngine;

namespace Boundary.UI.GameMenu
{
    public class StatusPanelUI: MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameObject contentPanel;
        
        // Status Summary
        [SerializeField] private TextMeshProUGUI leftStatusSummaryText;
        [SerializeField] private TextMeshProUGUI rightStatusSummaryText;
        
        // Current equipments
        [SerializeField] private EquipmentSlotButton bodyEquipmentSlot;
        [SerializeField] private EquipmentSlotButton acc1EquipmentSlot;
        [SerializeField] private EquipmentSlotButton acc2EquipmentSlot;
        [SerializeField] private EquipmentSlotButton acc3EquipmentSlot;
        
        // Talents
        [SerializeField] private GameObject talentListContent;
        [SerializeField] private TalentItemButton talentItemPrefab;
        
        private List<TalentItemButton> _talentItemButtons = new();
        private bool _isOpen;

        private IEventBus _eventBus;
        private IPlayerModel _player;
        private IInventoryModel _inventoryModel;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _player = GameContext.Instance.Player;
            _inventoryModel = GameContext.Instance.Inventory;
            
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
            _eventBus.Subscribe<EShowGameSubMenu>(OnGameSubMenuSelected);
            _eventBus.Subscribe<ECloseGameMenu>(OnGameMenuClosed);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EShowGameSubMenu>(OnGameSubMenuSelected);
            _eventBus.Unsubscribe<ECloseGameMenu>(OnGameMenuClosed);
        }
        
        private void OnGameSubMenuSelected(EShowGameSubMenu eShowGameSubMenu)
        {
            if (eShowGameSubMenu.SubMenu == GameSubMenu.Status)
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

        private void ResetPanel()
        {
            contentPanel.SetActive(false);
            
            leftStatusSummaryText.text = "";
            rightStatusSummaryText.text = "";

            bodyEquipmentSlot.Clear();
            acc1EquipmentSlot.Clear();
            acc2EquipmentSlot.Clear();
            acc3EquipmentSlot.Clear();
            
            foreach (var talentItemButton in _talentItemButtons)
            {
                Destroy(talentItemButton.gameObject);
            }
            _talentItemButtons.Clear();
        }
        
        private void RefreshUI()
        {
            leftStatusSummaryText.text = string.Join("\n", _player.GetAttributeLabels());
            rightStatusSummaryText.text = string.Join("\n", _player.GetOtherLabels());
            
            bodyEquipmentSlot.SetUnresponsive();
            bodyEquipmentSlot.SetData(_inventoryModel.Body);
            acc1EquipmentSlot.SetUnresponsive();
            acc1EquipmentSlot.SetData(_inventoryModel.Acc1);
            acc2EquipmentSlot.SetUnresponsive();
            acc2EquipmentSlot.SetData(_inventoryModel.Acc2);
            acc3EquipmentSlot.SetUnresponsive();
            acc3EquipmentSlot.SetData(_inventoryModel.Acc3);
            
            var talents = _player.GetTalentsSlots();
            for (var i = 0; i < talents.Count; i++)
            {
                var button = Instantiate(talentItemPrefab, talentListContent.transform);
                var talentItemButton = button.GetComponent<TalentItemButton>();
                talentItemButton.SetIndex(i);
                talentItemButton.SetData(talents[i]);
                _talentItemButtons.Add(talentItemButton);
                
                if (i == 0)
                {
                    talentItemButton.Select();
                    _eventBus.Publish(new ESelectTalentItemIndex(0));
                }
            }
        }

    }
}