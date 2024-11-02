using Code.Core.Events;
using Code.Utility.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Code.Utility.Events
{
    public static class EventBus
    {
        private static class SimpleEventInstance<T> where T : class, IBusEvent, new()
        {
            public static T Instance { get; } = new T();
        }
        
        public delegate void BusCallback<T>(T command) where T : class, IBusEvent;
        
        private static readonly Dictionary<ulong, IEventChannel> eventChannelMap = new Dictionary<ulong, IEventChannel>();

        public static void Publish<T>(T busEvent) where T : class, IBusEvent
            => GetEventChannel<T>().Publish(busEvent);


        public static void Publish<T>() where T : class, IBusEvent, new()
        {
            GetEventChannel<T>().Publish(SimpleEventInstance<T>.Instance);
        }

        public static bool Subscribe<T>(BusCallback<T> callback, bool isPermanent = false) where T : class, IBusEvent
            => GetEventChannel<T>().Subscribe(callback, isPermanent);

        public static bool Unsubscribe<T>(BusCallback<T> callback) where T : class, IBusEvent
            => GetEventChannel<T>().Unsubscribe(callback);


        public static void RemoveAllNonPermanentListeners()
        {
            foreach (var channel in eventChannelMap)
            {
                channel.Value.RemoveAllNonPermanentCallbacks();
            }
        }


        public static void RemoveAllNonPermanentListeners<T>() where T : class, IBusEvent
        {
            var channel = GetEventChannel<T>();
            channel.RemoveAllNonPermanentCallbacks();
        }
        
        
        private static EventChannel<T> GetEventChannel<T>()
            where T : class, IBusEvent
        {
            var hash = StaticClassHash<T>();
            if (eventChannelMap.TryGetValue(hash, out var existingEventChannel))
            {
                return (EventChannel<T>)existingEventChannel;
            }
            else
            {
                var newEventChannel = new EventChannel<T>();
                eventChannelMap.Add(hash, newEventChannel);
                return newEventChannel;
            }
        }

        static ulong StaticClassHash<T>()
        {
            if (StaticHashCache<T>.Initialized)
            {
                return StaticHashCache<T>.Id;
            }

            ulong hash = 14695981039346656037;
            var fullName = typeof(T).FullName;
            if (fullName != null)
            {
                foreach (ulong tempNum in fullName)
                {
                    hash = (hash ^ tempNum) * 1099511628211UL;
                }
            }

            StaticHashCache<T>.Initialized = true;
            StaticHashCache<T>.Id = hash;
            return hash;
        }

        static class StaticHashCache<T>
        {
            public static bool Initialized;
            public static ulong Id;
        }
    }
}