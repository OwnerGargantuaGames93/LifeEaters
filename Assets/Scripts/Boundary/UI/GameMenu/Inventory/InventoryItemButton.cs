using System;
using Control.Inventory;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boundary.UI.GameMenu
{
    public class InventoryItemButton : MonoBehaviour, ISelectHandler
    {
        [Header("Components")]
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI shortDescription;
        // TODO: In final version, this will be an predefined icon
        [SerializeField] private TextMeshProUGUI type;
        [SerializeField] private TextMeshProUGUI quantity;
        [SerializeField] private Button button;

        private IEventBus _eventBus;

        private int _currentIndex = -1;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }

        public void OnSelect(BaseEventData eventData)
        {
            _eventBus.Publish(new EUpdateInventoryItemIndex(_currentIndex));
        }

        public void SetIndex(int index)
        {
            _currentIndex = index;
        }

        public void SetData(InventoryItemSlot data)
        {
            icon.sprite = data.Icon;
            title.text = data.Name;
            shortDescription.text = data.ShortDescription;
            type.text = FromSlotTypeToText(data.SlotType);
            quantity.text = data.Quantity.ToString();
        }

        public void SelectButton()
        {
            button.Select();
        }

        private string FromSlotTypeToText(InventoryItemSlotType slotType)
        {
            return slotType switch
            {
                InventoryItemSlotType.CONSUMABLE => "C",
                InventoryItemSlotType.EQUIPMENT => "E",
                InventoryItemSlotType.KEY => "K",
                InventoryItemSlotType.ESSENCE => "E",
                InventoryItemSlotType.LIFE => "L",
                _ => throw new ArgumentOutOfRangeException(nameof(slotType), slotType, null)
            };
        }
    }

    #region Events
    
    /// <summary>
    /// Fired when an inventory item button is selected. Return his index.
    /// </summary>
    public struct EUpdateInventoryItemIndex
    {
        public readonly int ItemIndex;
        
        public EUpdateInventoryItemIndex(int itemIndex)
        {
            ItemIndex = itemIndex;
        }
    }
    
    #endregion
}
