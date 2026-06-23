using Control.DialogueHandler;
using Infra.EventBus;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Boundary.UI.Dialogue
{
    public class DialogueChoiceButton: MonoBehaviour, ISelectHandler
    {
        [Header("UI Components")]
        
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI choiceText;

        private int _choiceIndex = -1;
        
        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
        }
        
        public void SetChoiceText(string text)
        {
            choiceText.text = text;
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
            _eventBus.Publish(new EUpdateDialogueChoiceIndex(_choiceIndex));
        }
    }
}