using System;
using System.Runtime.CompilerServices;

namespace Infra.EventBus
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> callback);

        void Unsubscribe<T>(Action<T> callback);

        public void Publish<T>(T eventData, [CallerMemberName] string publisher = null, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0);
    }
}