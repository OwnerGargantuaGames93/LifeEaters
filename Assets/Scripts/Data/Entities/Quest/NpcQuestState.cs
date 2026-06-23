using UnityEngine;

namespace Data.Entities.Quest
{
    public class NpcQuestState
    {
        private readonly string _npcName;
        
        private string _currentQuestPhase;
        
        public NpcQuestState(string npcName, string currentQuestPhase)
        {
            _npcName = npcName;
            _currentQuestPhase = currentQuestPhase;
        }
        
        public string GetNpcName()
        {
            return _npcName;
        }
        
        public string GetCurrentQuestPhase()
        {
            return _currentQuestPhase;
        }

        public void SetCurrentQuestPhase(string newState)
        {
            _currentQuestPhase = newState;
        }

        public void AdvancePhase(NpcQuestData npcQuestData)
        {
            if (npcQuestData.npcName != _npcName)
            {
                throw new System.ArgumentException($"NpcQuestData npcName '{npcQuestData.npcName}' does not match NpcName '{_npcName}'");
            }
            
            var currentPhaseIndex = npcQuestData.questPhases.IndexOf(npcQuestData.questPhases.Find(phase => phase.phaseName == _currentQuestPhase));
            if (currentPhaseIndex == -1)
            {
                throw new System.InvalidOperationException($"CurrentQuestPhase '{_currentQuestPhase}' not found in npcQuestData.questPhases");
            }
            
            if (currentPhaseIndex + 1 < npcQuestData.questPhases.Count)
            {
                _currentQuestPhase = npcQuestData.questPhases[currentPhaseIndex + 1].phaseName;
            }
            else
            {
                Debug.Log($"[NpcQuestState] NPC '{_npcName}' is already at the final quest phase (index {currentPhaseIndex})");
            }
        }
        
        public NpcQuestPhase GetCurrentPhase(NpcQuestData npcQuestData)
        {
            if (npcQuestData.npcName != _npcName)
            {
                throw new System.ArgumentException($"NpcQuestData npcName '{npcQuestData.npcName}' does not match NpcName '{_npcName}'");
            }
            
            var currentPhase = npcQuestData.questPhases.Find(phase => phase.phaseName == _currentQuestPhase);
            if (currentPhase == null)
            {
                Debug.LogWarning($"[NpcQuestState] Current quest phase '{_currentQuestPhase}' not found in npcQuestData for NPC '{_npcName}'");
            }
            
            return currentPhase;
        }
    }
}