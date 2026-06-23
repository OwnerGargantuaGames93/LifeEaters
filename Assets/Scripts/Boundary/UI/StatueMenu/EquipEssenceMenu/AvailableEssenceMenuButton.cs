using Data.Database;
using Data.Entities.Item;
using Infra.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boundary.UI.StatueMenu
{
    public class AvailableEssenceMenuButton : MonoBehaviour, ISelectHandler
    {
        [Header("UI Components")] [SerializeField]
        private Button button;

        [SerializeField] private TextMeshProUGUI nameLabel;

        private int _choiceIndex = -1;

        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
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
            _eventBus.Publish(new EUpdateAvailableEssenceMenuButtonIndex(_choiceIndex));
        }

        public void MakeUnresponsive()
        {
            button.interactable = false;
        }

        public void MakeResponsive()
        {
            button.interactable = true;
        }

        public void SetData(EssenceVersion essence)
        {
            var essenceData = DataSource.Instance.GetEssenceItem(essence.sourceId);

            // Show energy cost for Standard essences, "Behavioural" label for Behavioural essences
            var displayText = essenceData.type == EssenceType.Behavioural
                ? $"{essenceData.Name} [B]"
                : $"{essenceData.Name} - {essence.GetTotalEnergyCost()} EP";

            nameLabel.text = displayText;
        }
    }

    #region Events

    public struct EUpdateAvailableEssenceMenuButtonIndex
    {
        public readonly int ChoiceIndex;

        public EUpdateAvailableEssenceMenuButtonIndex(int choiceIndex)
        {
            ChoiceIndex = choiceIndex;
        }
    }
    
    #endregion
}