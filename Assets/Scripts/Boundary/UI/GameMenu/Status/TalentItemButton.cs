using Control.Inventory;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boundary.UI.GameMenu
{
    public class TalentItemButton : MonoBehaviour, ISelectHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI description;

        private int _itemIndex;
    
        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }

        public void SetIndex(int index)
        {
            _itemIndex = index;
        }

        public void SetData(InventoryItemSlot data)
        {
            if (data == null)
            {
                Debug.LogWarning("Trying to set null data to TalentItemButton. This should not happen.");
                title.text = "";
                description.text = "";
                return;
            }
            
            title.text = data.Name;
            description.text = data.Description;
        }

        public void Select()
        {
            button.Select();
        }
    
        public void OnSelect(BaseEventData eventData)
        {
            _eventBus.Publish(new ESelectTalentItemIndex(_itemIndex));
        }
    }
    
    #region Events

    /// <summary>
    /// Fired when a talent item is selected in the talent list. The event carries the index of the selected button.
    /// </summary>
    public struct ESelectTalentItemIndex
    {
        public readonly int Index;
        
        public ESelectTalentItemIndex(int index)
        {
            Index = index;
        }
    }
    
    #endregion
}
