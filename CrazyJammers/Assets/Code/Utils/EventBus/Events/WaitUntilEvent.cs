using Code.Core.Events;
using System;
using System.Collections;
using UnityEngine;

namespace Code.Utility.Events
{
    public class WaitUntilEvent<T> : IEnumerator
    where T : class, IBusEvent
    {
        private bool hasEventOccurred;
        private Func<T, bool> filter;
        private float? timeoutTime;

        public WaitUntilEvent(Func<T, bool> filter = null, float? timeoutAfterSeconds = null)
        {
            hasEventOccurred = false;
            this.filter = filter;
            if(timeoutAfterSeconds != null)
                timeoutTime = Time.time + timeoutAfterSeconds;

            EventBus.Subscribe<T>(OnEvent);
        }

        public object Current => null;

        public bool MoveNext()
        {
            if (timeoutTime != null && Time.time >= timeoutTime)
            {
                Debug.LogError($"Timed out waiting for the {typeof(T)}");
                EventBus.Unsubscribe<T>(OnEvent);
                return false;
            }

            return !hasEventOccurred;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        private void OnEvent(T busEvent)
        {
            if (filter == null || filter(busEvent))
            {
                hasEventOccurred = true;
                EventBus.Unsubscribe<T>(OnEvent);
            }
        }
    }
}