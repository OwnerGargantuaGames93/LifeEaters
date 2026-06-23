using Control.Inventory;
using Data.Database;
using Infra.EventBus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boundary.UI.StatueMenu.EquipmentMenu
{
    public class EquipmentSlotButton: MonoBehaviour, ISelectHandler
    {
        [Header("Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        
        private int _selectedItemIndex = -1;
        
        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }
        
        public void SetSelectedItemIndex(int index)
        {
            _selectedItemIndex = index;
        }

        public void SetData(InventoryEquipmentItem item)
        {
            if (item == null)
            {
                icon.sprite = null;
                return;
            }
            
            var data = DataSource.Instance.GetEquipmentItem(item.item);
            if (data != null && data.Icon != null)
            {
                icon.sprite = data.Icon;    
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            _eventBus.Publish(new EUpdateEquipmentSlotIndex(_selectedItemIndex));
        }
        
        public void SelectButton()
        {
            button.Select();
        }

        public void SetResponsive()
        {
            button.interactable = true;
        }

        public void SetUnresponsive()
        {
            button.interactable = false;
        }

        public void Clear()
        {
            icon.sprite = null;
        }
    }
    
    #region Events

    public struct EUpdateEquipmentSlotIndex
    {
        public int SelectedIndex;

        public EUpdateEquipmentSlotIndex(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }
    
    #endregion
}