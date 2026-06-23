using Control.GameData;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boundary.UI.StatueMenu.SaveMenu
{
    public class SaveSlotButton: MonoBehaviour, ISelectHandler
    {
        [Header("Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI slotNumberLabel;
        [SerializeField] private TextMeshProUGUI saveDateTimeLabel;
        [SerializeField] private TextMeshProUGUI gameTimeLabel;
        [SerializeField] private TextMeshProUGUI playerLevelLabel;
        
        private int _selectedItemIndex = -1;
        
        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }

        public void SetData(SaveSlot saveSlot)
        {
            if (saveSlot.Status == SaveSlotStatus.Empty)
            {
                slotNumberLabel.text = $"Slot {saveSlot.SlotNumber}";
                saveDateTimeLabel.text = "Empty Slot";
                gameTimeLabel.text = "Game Time (hh:mm:ss): 00:00:00";
                playerLevelLabel.text = "Player Level: N/A";
                return;
            }
            
            slotNumberLabel.text = $"Slot {saveSlot.SlotNumber}";
            saveDateTimeLabel.text = "Last Game: " + saveSlot.SaveDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            gameTimeLabel.text = "Game Time (hh:mm:ss): " + System.TimeSpan.FromSeconds(saveSlot.GameTimeInSeconds).ToString(@"hh\:mm\:ss");
            playerLevelLabel.text = $"Player Level: {saveSlot.PlayerLevel}";
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
            _eventBus.Publish(new EUpdateSaveSlotSelectedIndex(_selectedItemIndex));
        }
    }
    
    #region Events

    public struct EUpdateSaveSlotSelectedIndex
    {
        public int SelectedIndex;

        public EUpdateSaveSlotSelectedIndex(int selectedIndex)
        {
            SelectedIndex = selectedIndex;
        }
    }
    
    #endregion
}