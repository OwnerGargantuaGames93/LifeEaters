using System;
using Data.Entities.Game;

namespace Control.DialogueHandler
{
    public interface IDialogueHandler: IDisposable
    {
        public bool IsDialoguePlaying();
        public void Save(GameSessionData gameData);
    }
}