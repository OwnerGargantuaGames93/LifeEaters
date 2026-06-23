using Control.Shop;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boundary.UI.Shop
{
    public class ShopItemButton: MonoBehaviour, ISelectHandler
    {
        [Header("UI Components")]
        
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI typeText;
        [SerializeField] private Image itemIcon;
        
        private int _selectedItemIndex = -1;
        
        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }

        public void SetData(ShopItemSlot shopItemSlot)
        {
            titleText.text = shopItemSlot.DisplayName;
            descriptionText.text = shopItemSlot.ShopDescription;
            priceText.text = $"Price: {shopItemSlot.Price}";
            quantityText.text = $"Qty: {shopItemSlot.Quantity}";
            // TODO: Add new field in ShopItemSlot for item type display value
            typeText.text = $"Type: {shopItemSlot.SlotType.ToString()}";
            itemIcon.sprite = shopItemSlot.Icon;
        }
        
        public void SetSelectedItemIndex(int index)
        {
            _selectedItemIndex = index;
        }
        
        public void SelectButton()
        {
            button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            _eventBus.Publish(new EUpdateShopItemSelectedIndex(_selectedItemIndex));
        }
    }
}