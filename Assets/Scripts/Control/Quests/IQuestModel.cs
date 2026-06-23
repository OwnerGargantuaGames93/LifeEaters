using System;
using Data.Entities.Game;
using Data.Entities.Quest;

namespace Control.Quests
{
    public interface IQuestModel: IDisposable
    {
        /// <summary>
        /// Gets the current quest phase for an NPC.
        /// Returns null if the NPC has no quest data or if the NPC quest hasn't started yet.
        /// </summary>
        NpcQuestPhase GetNpcCurrentPhase(string npcName);
        
        /// <summary>
        /// Saves the current quest states to game session data.
        /// </summary>
        void Save(GameSessionData gameData);
    }
}