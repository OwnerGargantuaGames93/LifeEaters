using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Infra.EventBus
{
    /// <summary>
    /// A simple event bus implementation for decoupled communication between components.
    /// </summary>
    public class EventBus: IEventBus
    {
        private readonly Dictionary<Type, Delegate> _subscribers = new();
        private const bool LogEnabled = false;

        public void Subscribe<T>(Action<T> callback)
        {
            var eventType = typeof(T);
            if (!_subscribers.TryAdd(eventType, callback))
            {
                _subscribers[eventType] = Delegate.Combine(_subscribers[eventType], callback);
            }
        }

        public void Unsubscribe<T>(Action<T> callback)
        {
            var eventType = typeof(T);
            if (!_subscribers.ContainsKey(eventType))
            {
                return;
            }
        
            var currentDel = _subscribers[eventType];
            var removedDel = Delegate.Remove(currentDel, callback);
            if (removedDel == null)
            {
                _subscribers.Remove(eventType);
            }
            else
            {
                _subscribers[eventType] = removedDel;
            }
        }

        public void Publish<T>(
            T eventData,
            [CallerMemberName] string publisher = null,
            [CallerFilePath] string file = null,
            [CallerLineNumber] int line = 0)
        {
            var eventType = typeof(T);
            
            if (LogEnabled)
            {
                Debug.Log($"Event published: {eventType.Name} by {publisher} ({file}:{line})");    
            }

            if (!_subscribers.TryGetValue(eventType, out var subscriber))
            {
                return;
            }

            if (subscriber is not Action<T> callback)
            {
                return;
            }

            foreach (var handler in callback.GetInvocationList())
            {
                if (LogEnabled)
                {
                    Debug.Log($"Event {eventType.Name} consumed by handler {handler.Target?.GetType().FullName ?? "static"}");    
                }
                
                ((Action<T>)handler).Invoke(eventData);
            }
        }
    }
}