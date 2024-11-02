using Code.Core.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Code.Utility.Events.EventBus;

namespace Code.Utility.Events
{
    public class EventChannel<T> : IEventChannel
        where T : class, IBusEvent
    {
        private readonly struct CallbackEntry
        {
            public readonly BusCallback<T> Callback;
            public readonly bool IsPermanent;

            public CallbackEntry(BusCallback<T> callback, bool isPermanent)
            {
                Callback = callback;
                IsPermanent = isPermanent;
            }
        }

        private readonly struct DeferredChangeEntry
        {
            public readonly BusCallback<T> Callback;
            public readonly Operation Operation;
            public readonly bool IsPermanent;

            public DeferredChangeEntry(BusCallback<T> callback, Operation operation, bool isPermanent)
            {
                Callback = callback;
                Operation = operation;
                IsPermanent = isPermanent;
            }
        }
        
        private enum Operation { Subscribe, Unsubscribe }

        private readonly List<CallbackEntry> callbacks = new List<CallbackEntry>();

        private readonly List<DeferredChangeEntry> deferredChanges = new List<DeferredChangeEntry>();

        private uint activePublishCallsCount = 0;
        private bool removeAllNonPermanentListeners = false;

        public bool InProcessOfPublishing => activePublishCallsCount > 0;
        
        public bool Subscribe(BusCallback<T> callback, bool isPermanent = false)
            => PerformOperation(callback, Operation.Subscribe, isPermanent);

        public bool Unsubscribe(BusCallback<T> callback)
            => PerformOperation(callback, Operation.Unsubscribe, false);

        void IEventChannel.Publish(IBusEvent busEvent)
            => Publish((T)busEvent);

        public void Publish(T busEvent)
        {
            if (callbacks.Count == 0)
                return;

            #if DEBUG
            if (activePublishCallsCount > 0)
            {
                Debug.LogWarning($"Recursive event publishing detected. Event type: {typeof(T)}");
            }
            #endif
            
            activePublishCallsCount++;
            foreach (var entry in callbacks)
            {
                try
                {
                    entry.Callback(busEvent);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            
            Debug.Assert(activePublishCallsCount > 0);
            activePublishCallsCount--;

            if (activePublishCallsCount > 0)
            {
                return;
            }
            
            if (deferredChanges.Count > 0)
            {
                foreach (var entry in deferredChanges)
                {
                    PerformOperation(entry.Callback, entry.Operation, entry.IsPermanent);
                }
                deferredChanges.Clear();
            }

            if (removeAllNonPermanentListeners)
            {
                RemoveAllNonPermanentCallbacks();
            }
        }


        public void RemoveAllNonPermanentCallbacks()
        {
            if (InProcessOfPublishing)
            {
                removeAllNonPermanentListeners = true;
                return;
            }

            callbacks.RemoveAll(entry => entry.IsPermanent == false);
            
            removeAllNonPermanentListeners = false;
        }

        private bool PerformOperation(BusCallback<T> callback, Operation operation, bool isPermanent)
        {
            if (InProcessOfPublishing)
            {
                deferredChanges.Add(new DeferredChangeEntry(callback, operation, isPermanent));
                return false;
            }
            
            switch(operation)
            {
                case Operation.Subscribe:
                    callbacks.Add(new CallbackEntry(callback, isPermanent));
                    break;
                case Operation.Unsubscribe:
                    callbacks.RemoveAll(entry => entry.Callback == callback);
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            return true;
        }
    }
}