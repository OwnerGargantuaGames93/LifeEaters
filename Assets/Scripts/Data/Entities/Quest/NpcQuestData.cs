using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Entities.Quest
{
    [CreateAssetMenu(fileName = "NpcQuestData", menuName = "Npc/NpcQuestData")]
    public class NpcQuestData: ScriptableObject
    {
        public string npcName;
        
        /// <summary>
        /// List of quest phases with all related information (name and location).
        /// The index represents the phase number.
        /// </summary>
        public List<NpcQuestPhase> questPhases;
    }
    
    [Serializable]
    public class NpcQuestPhase
    {
        /// <summary>
        /// The name/identifier of this quest phase (e.g., "initial", "helped_player", "moved_to_city")
        /// </summary>
        public string phaseName;
        
        /// <summary>
        /// The identifier of the NPC GameObject in the scene.
        /// This should match the Id from IdentifiableMonoBehaviour.
        /// If empty, the NPC is hidden.
        /// </summary>
        public string npcGameObjectId;
    }
}