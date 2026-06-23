using Control.Inventory;
using Control.Player;
using Data.Database;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boundary.UI.StatueMenu
{
    public class EquippedEssenceMenuButton: MonoBehaviour, ISelectHandler
    {
        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;

        private int _choiceIndex = -1;
        
        private IEventBus _eventBus;
        private IPlayerModel _player;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _player = GameContext.Instance.Player;
        }
        
        public void SetChoiceIndex(int index)
        {
            _choiceIndex = index;
        }

        public void SelectButton()
        {
            button.Select();
        }

        public void OnSelect(BaseEventData eventData)
        {
            _eventBus.Publish(new EUpdateEquippedEssenceMenuButtonIndex(_choiceIndex));
        }

        public void SetData(InventoryEssenceItem essence)
        {
            var prefix = GetEssenceSlotPrefix(_choiceIndex);
            var essenceSlots = _player.Attributes.EssenceSlots;
            
            // Check if the essence slot is locked
            // Choice index is 0-based, essence slots is 1-based
            if (_choiceIndex >= essenceSlots)
            {
                label.text = prefix + "Locked Slot";
                button.interactable = false;
                return;
            }
            
            if (essence == null)
            {
                label.text = prefix + "Empty Slot";
                return;
            }
            
            var essenceData = DataSource.Instance.GetEssenceItem(essence.item.sourceId);
            
            label.text = prefix + essenceData.Name;
        }
        
        private static string GetEssenceSlotPrefix(int index)
        {
            return index switch
            {
                0 => "S1: ",
                1 => "S2: ",
                2 => "B1: ",
                3 => "S3: ",
                4 => "B2: ",
                5 => "S4: ",
                6 => "B3: ",
                7 => "S5: ",
                _ => "Unknown Essence Slot: "
            };
        }

    }
    #region Events

    public struct EUpdateEquippedEssenceMenuButtonIndex
    {
        public readonly int ChoiceIndex;

        public EUpdateEquippedEssenceMenuButtonIndex(int choiceIndex)
        {
            ChoiceIndex = choiceIndex;
        }
    }
    
    #endregion
}