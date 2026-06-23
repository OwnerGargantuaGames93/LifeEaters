using System.Collections.Generic;
using Control.DialogueHandler;
using Control.GameData;
using Data.Entities.Game;
using Data.Entities.Quest;
using Infra.EventBus;
using Ink.Runtime;
using UnityEngine;

namespace Control.Quests
{
    public class QuestModel: MonoBehaviour, IQuestModel
    {
        private IEventBus _eventBus;
        
        // NPC quests current status - tracks which phase index each NPC is at
        private readonly List<NpcQuestState> _npcQuestStates = new();
        
        [Header("Npc Quests Data")]
        [SerializeField] public List<NpcQuestData> npcQuestsData;
        
        private void Awake()
        {
            _eventBus = GameContext.Instance.EventBus;
            
            SubscribeToEvents();
        }

        #region Events Handling

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<EAdvanceNpcQuest>(OnAdvanceNpcQuest);
            _eventBus.Subscribe<EGameDataLoaded>(OnGameDataLoaded);
        }

        private void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<EAdvanceNpcQuest>(OnAdvanceNpcQuest);
            _eventBus.Unsubscribe<EGameDataLoaded>(OnGameDataLoaded);
        }

        private void OnAdvanceNpcQuest(EAdvanceNpcQuest e)
        {
            var npcName = e.NpcName;
            
            // Find the corresponding NpcQuestData
            var npcQuestData = npcQuestsData.Find(data => data.npcName == npcName);
            
            if (npcQuestData == null)
            {
                Debug.LogError($"[QuestModel] No NpcQuestData found for NPC: {npcName}");
                return;
            }
            
            // Find or create the NpcQuestState for this NPC
            var npcQuestState = _npcQuestStates.Find(state => state.GetNpcName() == npcName);
            if (npcQuestState == null)
            {
                var firstPhase = npcQuestData.questPhases[0];
                npcQuestState = new NpcQuestState(npcName, firstPhase.phaseName);
                _npcQuestStates.Add(npcQuestState);
            }
            
            // Advance the quest phase
            npcQuestState.AdvancePhase(npcQuestData);
            
            var currentPhase = npcQuestState.GetCurrentPhase(npcQuestData);
            
            Debug.Log($"[QuestModel] NPC '{npcName}' advanced to phase '{currentPhase.phaseName}' (name {npcQuestState.GetCurrentQuestPhase()})");
            
            // Update ink quest state variable for dialogues
            // The name of the Ink quest variable to update is always npcName capitalized + "QuestState"
            var inkVarName = GetInkQuestStateVariableName(npcName);
            var value = new StringValue(currentPhase.phaseName);
            
            _eventBus.Publish(new EInkVariableChanged(inkVarName, value));
        }
        
        private void OnGameDataLoaded(EGameDataLoaded e)
        {
            // When game is loaded, restore _npcQuestStates from saved data
            LoadQuestStatesFromGameData(e.GameData);
            
            // Sync Ink variables with loaded quest states
            SyncInkVariablesFromQuestStates();
        }
        
        #endregion
        
        private static string GetInkQuestStateVariableName(string npcName)
        {
            return npcName.Substring(0, 1).ToUpper() + npcName.Substring(1) + "QuestState";
        }
        
        public void Dispose()
        {
            UnsubscribeFromEvents();
        }
        
        #region Interface Methods
        
        public NpcQuestPhase GetNpcCurrentPhase(string npcName)
        {
            // Find the corresponding NpcQuestData
            var npcQuestData = npcQuestsData.Find(data => data.npcName == npcName);
            
            if (npcQuestData == null)
            {
                Debug.LogWarning($"[QuestModel] No NpcQuestData found for NPC: {npcName}");
                return null;
            }
            
            // Find the NpcQuestState for this NPC
            var npcQuestState = _npcQuestStates.Find(state => state.GetNpcName() == npcName);
            
            if (npcQuestState == null)
            {
                // NPC quest hasn't started yet, return the first phase (phase 0)
                if (npcQuestData.questPhases.Count > 0)
                {
                    return npcQuestData.questPhases[0];
                }
                
                Debug.LogWarning($"[QuestModel] No quest phases defined for NPC: {npcName}");
                return null;
            }
            
            return npcQuestState.GetCurrentPhase(npcQuestData);
        }
        
        public void Save(GameSessionData gameData)
        {
            // Save current quest states to game session data
            gameData.npcQuestStates.Clear();
            
            foreach (var questState in _npcQuestStates)
            {
                gameData.npcQuestStates.Add(new NpcQuestStateData
                {
                    npcName = questState.GetNpcName(),
                    currentPhaseName = questState.GetCurrentQuestPhase()
                });
            }
            
            Debug.Log($"[QuestModel] Saved {_npcQuestStates.Count} NPC quest states");
        }
        
        #endregion
        
        #region Load Methods
        
        private void LoadQuestStatesFromGameData(GameSessionData gameData)
        {
            _npcQuestStates.Clear();
            
            if (gameData.npcQuestStates == null || gameData.npcQuestStates.Count == 0)
            {
                Debug.Log("[QuestModel] No saved NPC quest states found");
                return;
            }
            
            foreach (var savedState in gameData.npcQuestStates)
            {
                var questState = new NpcQuestState(savedState.npcName, savedState.currentPhaseName);
                _npcQuestStates.Add(questState);
                
                Debug.Log($"[QuestModel] Loaded quest state for '{savedState.npcName}': phase index {savedState.currentPhaseName}");
            }
        }
        
        private void SyncInkVariablesFromQuestStates()
        {
            // Update Ink variables to match the loaded quest states
            foreach (var questState in _npcQuestStates)
            {
                var npcName = questState.GetNpcName();
                var npcQuestData = npcQuestsData.Find(data => data.npcName == npcName);
                
                if (npcQuestData == null)
                {
                    Debug.LogWarning($"[QuestModel] No NpcQuestData found for '{npcName}' during sync");
                    continue;
                }
                
                var currentPhase = questState.GetCurrentPhase(npcQuestData);
                if (currentPhase == null)
                {
                    Debug.LogWarning($"[QuestModel] Could not get current phase for '{npcName}' during sync");
                    continue;
                }
                
                // Update Ink variable
                var inkVarName = GetInkQuestStateVariableName(npcName);
                var value = new StringValue(currentPhase.phaseName);
                
                _eventBus.Publish(new EInkVariableChanged(inkVarName, value));
                
                Debug.Log($"[QuestModel] Synced Ink variable '{inkVarName}' = '{currentPhase.phaseName}'");
            }
        }
        
        #endregion
    }
    
    #region Events

    public struct EAdvanceNpcQuest
    {
        public readonly string NpcName;

        public EAdvanceNpcQuest(string npcName)
        {
            NpcName = npcName;
        }
    }
    
    #endregion
}