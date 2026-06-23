using Control.Inventory;
using Data.Database;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boundary.UI.StatueMenu.EquipmentMenu
{
    public class AvailableEquipmentItemButton: MonoBehaviour, ISelectHandler
    {
        [Header("Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI label;
        
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
            var data = DataSource.Instance.GetEquipmentItem(item.item);
            
            if (item != null)
            {
                if (data.Icon != null)
                {
                    icon.sprite = data.Icon;    
                }
                
                label.text = data.Name;        
            }
            else
            {
                label.text = "No name";
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            _eventBus.Publish(new EUpdateAvailableEquipmentSlotIndex(_selectedItemIndex));
        }
        
        public void SelectButton()
        {
            button.Select();
        }
    }
    
    public struct EUpdateAvailableEquipmentSlotIndex
    {
        public int SelectedIndex;

        public EUpdateAvailableEquipmentSlotIndex(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }
}