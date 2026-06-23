using System.Collections.Generic;
using Control.Shop;
using Infra.EventBus;
using TMPro;
using UnityEngine;

namespace Boundary.UI.Shop
{
    public class ShopPanelUI: MonoBehaviour
    {
        [Header("UI Components")]

        [SerializeField] private GameObject contentPanel;
        [SerializeField] private TextMeshProUGUI shopNameText;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private GameObject shopItemListContent;
        
        private List<GameObject> _shopItems = new();
        
        private List<ShopItemSlot> _currentShopItemSlots = new();
        private ShopItemSlot _currentShopItemSlot = null;
        private string _currentShopName = "";

        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            
            contentPanel.SetActive(false);

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

        #region Events Handling

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EOpenShop>(OnShopOpened);
            _eventBus.Subscribe<ECloseShop>(OnShopClosed);
            _eventBus.Subscribe<EShopCancelCommandExecuted>(OnShopCancelCommandExecuted);
            _eventBus.Subscribe<EShopConfirmCommandExecuted>(OnShopConfirmCommandExecuted);
            _eventBus.Subscribe<EDisplayShopItemsList>(OnDisplayShopItemsList);
            _eventBus.Subscribe<EUpdateShopItemSelectedIndex>(OnUpdateShopItemSelectedIndex);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EOpenShop>(OnShopOpened);
            _eventBus.Unsubscribe<ECloseShop>(OnShopClosed);
            _eventBus.Unsubscribe<EShopCancelCommandExecuted>(OnShopCancelCommandExecuted);
            _eventBus.Unsubscribe<EShopConfirmCommandExecuted>(OnShopConfirmCommandExecuted);
            _eventBus.Unsubscribe<EDisplayShopItemsList>(OnDisplayShopItemsList);
            _eventBus.Unsubscribe<EUpdateShopItemSelectedIndex>(OnUpdateShopItemSelectedIndex);
        }

        private void OnShopOpened(EOpenShop e)
        {
            contentPanel.SetActive(true);
        }
        
        private void OnShopClosed(ECloseShop e)
        {
            contentPanel.SetActive(false);
            ResetData();
            ResetPanel();
        }

        private void OnDisplayShopItemsList(EDisplayShopItemsList e)
        {
            var shopDisplayName = e.ShopDisplayName;
            var shopName = e.ShopName;
            _currentShopName = shopName;

            var shopItemSlots = e.BuyableItems;

            shopNameText.text = shopDisplayName;
            
            foreach (var item in _shopItems)
            {
                Destroy(item);
            }

            _shopItems.Clear();
            _currentShopItemSlots.Clear();
            _currentShopItemSlot = null;
            
            var index = 0;
            foreach (var shopItemSlot in shopItemSlots)
            {
                var shopItemUIObject = Instantiate(shopItemPrefab, shopItemListContent.transform);
                var shopItemUI = shopItemUIObject.GetComponent<ShopItemButton>();
                shopItemUI.SetData(shopItemSlot);
                shopItemUI.SetSelectedItemIndex(index);
                
                _shopItems.Add(shopItemUIObject);
                _currentShopItemSlots.Add(shopItemSlot);

                // Select the first item by default
                if (index == 0)
                {
                    shopItemUI.SelectButton();
                    _eventBus.Publish(new EUpdateShopItemSelectedIndex(0));
                }

                index++;
            }
        }
        
        private void OnUpdateShopItemSelectedIndex(EUpdateShopItemSelectedIndex e)
        {
            var selectedIndex = e.SelectedIndex;

            if (selectedIndex < 0 || selectedIndex >= _currentShopItemSlots.Count)
            {
                Debug.LogWarning($"[ShopPaneUI] Invalid shop item selected index: {selectedIndex}");
                return;
            }

            _currentShopItemSlot = _currentShopItemSlots[selectedIndex];
        }

        private void OnShopConfirmCommandExecuted(EShopConfirmCommandExecuted e)
        {
            if (_currentShopItemSlot == null)
            {
                Debug.LogWarning("[ShopPaneUI] No shop item selected to execute select command.");
                return;
            }

            _eventBus.Publish(new EShopItemSelected(_currentShopItemSlot, _currentShopName));
        }
        
        private void OnShopCancelCommandExecuted(EShopCancelCommandExecuted e)
        {
            _eventBus.Publish(new ECloseShop());
        }

        #endregion

        private void ResetData()
        {
            _currentShopItemSlots.Clear();
            _currentShopItemSlot = null;
            _currentShopName = "";
        }
        
        private void ResetPanel()
        {
            shopNameText.text = "";
            
            foreach (var item in _shopItems)
            {
                Destroy(item);
            }

            _shopItems.Clear();
        }
    }
}