using Infra.EventBus;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Boundary.UI.Dialogue
{
    public class StatueMenuButton: MonoBehaviour, ISelectHandler
    {
        [Header("UI Components")]
        
        [SerializeField] private Button button;

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
            _eventBus.Publish(new EUpdateStatueMenuButtonIndex(_choiceIndex));
        }
        
        public void MakeUnresponsive()
        {
            button.interactable = false;
        }
        
        public void MakeResponsive()
        {
            button.interactable = true;
        }
    }
}