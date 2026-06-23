using System.Collections;
using System.Collections.Generic;
using Control.GameData;
using Control.Inventory;
using Control.Player;
using Control.Shop;
using Data.Entities.Game;
using Data.Entities.Item;
using Infra.EventBus;
using Ink.Runtime;
using UnityEngine;
using Utils;

namespace Control.DialogueHandler
{
    public class DialogueHandler: MonoBehaviour, IDialogueHandler
    {
        [Header("Ink Story")]
        [SerializeField] private TextAsset inkJson;
        
        private IEventBus _eventBus;
        private IPlayerModel _player;
        private IInventoryModel _inventory;
        
        private bool _dialogueIsPlaying;
        private string _currentKnotName;
        private int _currentChoiceIndex = -1;
        
        private Story _story;

        private InkExternalFunctions _inkExternalFunctions;
        private InkVariables _inkVariables;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            _player = GameContext.Instance.Player;
            _inventory = GameContext.Instance.Inventory;
            
            _story = new Story(inkJson.text);

            _inkExternalFunctions = new InkExternalFunctions(_eventBus);
            _inkExternalFunctions.Bind(_story);
            _inkVariables = new InkVariables(_story);
            
            SubscribeToEvents();
        }
        
        #region Events Handling

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EEnterDialogue>(OnDialogueEntered);
            _eventBus.Subscribe<ECloseShop>(OnCloseShop);
            _eventBus.Subscribe<EContinueDialogueCommandExecuted>(OnContinueDialogCommandExecuted);
            _eventBus.Subscribe<EUpdateDialogueChoiceIndex>(OnUpdateChoiceIndex);
            _eventBus.Subscribe<EGameDataLoaded>(OnGameDataLoaded);
            _eventBus.Subscribe<EInkVariableChanged>(OnInkVariableChanged);
            _eventBus.Subscribe<EPlayerStatsUpdated>(OnPlayerStatsUpdated);
            _eventBus.Subscribe<EItemAddedToInventory>(OnItemAddedToInventory);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EEnterDialogue>(OnDialogueEntered);
            _eventBus.Unsubscribe<ECloseShop>(OnCloseShop);
            _eventBus.Unsubscribe<EContinueDialogueCommandExecuted>(OnContinueDialogCommandExecuted);
            _eventBus.Unsubscribe<EUpdateDialogueChoiceIndex>(OnUpdateChoiceIndex);
            _eventBus.Unsubscribe<EGameDataLoaded>(OnGameDataLoaded);
            _eventBus.Unsubscribe<EInkVariableChanged>(OnInkVariableChanged);
            _eventBus.Unsubscribe<EPlayerStatsUpdated>(OnPlayerStatsUpdated);
            _eventBus.Unsubscribe<EItemAddedToInventory>(OnItemAddedToInventory);
        }
        
        private void OnDialogueEntered(EEnterDialogue e)
        {
            if (_dialogueIsPlaying)
            {
                Debug.LogWarning("[DialogueHandler] Dialogue is already playing. Ignoring new dialogue request.");
                return;
            }
            
            _dialogueIsPlaying = true;
            
            _eventBus.Publish(new EDialogueStarted());
            
            var knotName = e.KnotName;

            // Start listening to variable changes and sync BEFORE choosing path
            _inkVariables.SyncVariablesAndStartListening(_story);

            if (!knotName.Equals(""))
            {
                _story.ChoosePathString(knotName);
            }
            else
            {
                Debug.LogWarning("[DialogueHandler] Knot name for NPM is not specified.");
            }
            
            _currentKnotName = knotName;
            
            ContinueOrExitDialogue();
        }
        
        private void OnContinueDialogCommandExecuted(EContinueDialogueCommandExecuted e)
        {
            if (!_dialogueIsPlaying)
            {
                return;
            }
            
            ContinueOrExitDialogue();
        }
        
        private void OnUpdateChoiceIndex(EUpdateDialogueChoiceIndex e)
        {
            var choiceIndex = e.ChoiceIndex;
            UpdateChoiceIndex(choiceIndex);
        }

        private void OnGameDataLoaded(EGameDataLoaded e)
        {
            var gameData = e.GameData;
            var inkDialogueState = gameData.inkDialogueState;
            
            if (inkDialogueState == null || inkDialogueState.Trim().Equals(""))
            {
                Debug.LogWarning("No ink saved state found in game session data");
                return;
            }
            
            _story.state.LoadJson(inkDialogueState);
            
            _inkVariables.ReloadFromStory(_story);

            CheckPlayerStats();
            CheckItemsInPossessions();

            _inkVariables.SyncVariablesToStory(_story);
            // _inkVariables.DebugInkVariables(_story);
        }

        // TODO: Not used at the moment
        private void OnGameDataCancelled()
        {
            _story.ResetState();
        }
        
        private void OnInkVariableChanged(EInkVariableChanged e)
        {
            var variableName = e.VariableName;
            var newValue = e.NewValue;
            
            _inkVariables.UpdateVariableState(variableName, newValue);
        }
        
        private void OnCloseShop(ECloseShop e)
        {
            if (!_dialogueIsPlaying)
            {
                return;
            }
            
            ContinueOrExitDialogue();
        }
        
        private void OnPlayerStatsUpdated(EPlayerStatsUpdated e)
        {
            CheckPlayerStats();
        }
        
        private void OnItemAddedToInventory(EItemAddedToInventory e)
        {
            CheckItemsInPossessions();
        }
        
        private void CheckItemsInPossessions()
        {
            _inkVariables.UpdateVariableState(Constants.IVHasClownKey, new BoolValue(_inventory.HasKeyObject(KeyId.ClownPrisonKey)));
        }
        
        private void CheckPlayerStats()
        {
            var alienOratory = _player.Attributes.AlienOratory;
            var floatAlienOratoryInkValue = new FloatValue(alienOratory);
            _inkVariables.UpdateVariableState(Constants.IVAlienOratoryValue, floatAlienOratoryInkValue);

            var humanOratory = _player.Attributes.Oratory;
            var floatHumanOratoryInkValue = new FloatValue(humanOratory);
            _inkVariables.UpdateVariableState(Constants.IVHumanOratoryValue, floatHumanOratoryInkValue);
        }
        
        // TODO: Handle item removal if needed in the future

        #endregion

        private void UpdateChoiceIndex(int index)
        {
            _currentChoiceIndex = index;
        }

        private void ContinueOrExitDialogue()
        {
            // make a choice, if any
            if (_story.currentChoices.Count > 0 && _currentChoiceIndex >= 0)
            {
                _story.ChooseChoiceIndex(_currentChoiceIndex);
                _currentChoiceIndex = -1;
            }
            
            if (_story.canContinue)
            {
                var line = _story.Continue();
                
                // skip blank lines
                while (IsLineBlank(line) && _story.canContinue)
                {
                    line = _story.Continue();
                }
                
                _eventBus.Publish(new EDialogueLineDisplayed(_currentKnotName, line, _story.currentChoices));
            } 
            else if (_story.currentChoices.Count == 0)
            {
                StartCoroutine(ExitDialogue());                
            }
        }
        
        private IEnumerator ExitDialogue()
        {
            yield return null;
            
            _dialogueIsPlaying = false;
            _currentKnotName = "";
            
            _eventBus.Publish(new EDialogueFinished());
            
            _inkVariables.StopListening(_story);
        }
        
        private static bool IsLineBlank(string line)
        {
            return line.Trim().Equals("") || line.Trim().Equals("\n");
        }

        #region Public Interface Implementation

        public void Dispose()
        {
            UnsubscribeFromEvents();
            
            _inkExternalFunctions.Unbind(_story);
        }

        public bool IsDialoguePlaying()
        {
            return _dialogueIsPlaying;
        }
        
        public void Save(GameSessionData gameData)
        {
            var savedState = _story.state.ToJson();
            gameData.inkDialogueState = savedState;
        }

        #endregion
    }
    
    #region Events

    /// <summary>
    /// Sent when entering a dialogue knot.
    /// </summary>
    public struct EEnterDialogue
    {
        public readonly string KnotName;
        
        public EEnterDialogue(string knotName)
        {
            KnotName = knotName;
        }
    }
    
    public struct EContinueDialogueCommandExecuted { }
    
    public struct EUpdateDialogueChoiceIndex
    {
        public readonly int ChoiceIndex;
        
        public EUpdateDialogueChoiceIndex(int choiceIndex)
        {
            ChoiceIndex = choiceIndex;
        }
    }
    
    public struct EDialogueStarted { }
    
    public struct EDialogueFinished { }
    
    public struct EDialogueLineDisplayed
    {
        public readonly string NpcName;
        public readonly string Line;
        public readonly List<Choice> Choices;
        
        public EDialogueLineDisplayed(string npcName, string line, List<Choice> choices)
        {
            NpcName = npcName;
            Line = line;
            Choices = choices;
        }
    }
    
    #endregion
}