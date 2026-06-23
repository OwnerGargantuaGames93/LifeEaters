using Control.GameData;
using UnityEngine;
using UnityEngine.UIElements;

namespace Boundary.UI.StatueMenu.SaveMenu.SaveSlotListItem
{
    public class SaveSlotRow
    {
        Label slotNumberLabel;
        
        Label gameTimeLabel;
        
        Label lastGameLabel;
        
        Label levelLabel;

        public void SetVisualElement(VisualElement itemRow)
        {
            slotNumberLabel = itemRow.Q<Label>("SlotNumberLabel");
            gameTimeLabel = itemRow.Q<Label>("GameTimeLabel");
            lastGameLabel = itemRow.Q<Label>("LastGameLabel");
            levelLabel = itemRow.Q<Label>("LevelLabel");
        }

        public void SetData(SaveSlot saveSlotData)
        {
            if (saveSlotData.Status == SaveSlotStatus.Empty)
            {
                slotNumberLabel.text = "Slot Number: " + saveSlotData.SlotNumber;
                gameTimeLabel.text = "Game Time: -";
                lastGameLabel.text = "Last Game: -";
                levelLabel.text = "Level: -";
                return;
            }
            
            slotNumberLabel.text = "Slot Number: " + saveSlotData.SlotNumber;
            gameTimeLabel.text = "Game Time (hh:mm:ss): " + System.TimeSpan.FromSeconds(saveSlotData.GameTimeInSeconds).ToString(@"hh\:mm\:ss");
            lastGameLabel.text = "Last Game: " + saveSlotData.SaveDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.Log("Last Game Data: " +saveSlotData.SaveDateTime);
            levelLabel.text = "Level: " + saveSlotData.PlayerLevel;
        }
    }
}