using System.Collections.Generic;
using Control.DialogueHandler;
using Infra.EventBus;
using TMPro;
using UnityEngine;

namespace Boundary.UI.Dialogue
{
    public class DialoguePanelUI : MonoBehaviour
    {
        [Header("UI Components")]
        
        [SerializeField] private GameObject contentPanel;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private List<DialogueChoiceButton> choiceButtons;
        
        private IEventBus _eventBus;

        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            
            contentPanel.SetActive(false);
            ResetPanel();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EDialogueStarted>(DialogueStarted);
            _eventBus.Subscribe<EDialogueFinished>(DialogueFinished);
            _eventBus.Subscribe<EDialogueLineDisplayed>(DisplayDialogue);
        }
        
        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EDialogueStarted>(DialogueStarted);
            _eventBus.Unsubscribe<EDialogueFinished>(DialogueFinished);
            _eventBus.Unsubscribe<EDialogueLineDisplayed>(DisplayDialogue);
        }

        private void DialogueStarted(EDialogueStarted e)
        {
            contentPanel.SetActive(true);
        }
        
        private void DialogueFinished(EDialogueFinished e)
        {
            contentPanel.SetActive(false);
            ResetPanel();
        }

        private void DisplayDialogue(EDialogueLineDisplayed e)
        {
            var dialogueLine = e.Line;
            var choices = e.Choices;

            if (choices.Count > choiceButtons.Count)
            {
                Debug.LogError("[DialoguePanelUI] Not enough choice buttons to display all choices.");
            }
            
            // Because the buttons are always displayed we need to reverse 
            // the order of choices to match the button layout:
            // Example:
            // Button 1: Choice 0
            // Button 2: Choice 1
            
            // Default: all buttons inactive
            foreach (var button in choiceButtons)
            {
                button.gameObject.SetActive(false);
            }

            var choiceButtonIndex = choices.Count - 1;
            for (var inkChoiceIndex = 0; inkChoiceIndex < choices.Count; inkChoiceIndex++)
            {
                var choice = choices[inkChoiceIndex];
                var button = choiceButtons[choiceButtonIndex];

                button.SetChoiceText(choice.text);
                button.SetChoiceIndex(inkChoiceIndex);
                button.gameObject.SetActive(true);

                if (inkChoiceIndex == 0)
                {
                    button.SelectButton();
                    _eventBus.Publish(new EUpdateDialogueChoiceIndex(0));
                }
                
                choiceButtonIndex--;
            }
            
            dialogueText.text = dialogueLine;
        }

        private void ResetPanel()
        {
            dialogueText.text = "";
        }
    }
}