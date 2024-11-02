using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Core.Events
{
    public static class EventDefs
    {
    }

    public interface IBusEvent
    {

    }

    //Temporary stack of T's could use an array of params T with common interface
    public abstract class GenericEvent<T> : IBusEvent
    {
        public GenericEvent(){}
        public GenericEvent(T eventData)
        {
            First = eventData;
        }

        public T First { get; private set; }

        public void Set(T data) { First = data; }
    } 

    public abstract class GenericEvent<T, T1> : GenericEvent<T>
    {
        public GenericEvent() { }
        public GenericEvent(T first, T1 second) : base (first)
        {
            Second = second;
        }

        public T1 Second { get; private set; }
        public void Set(T first, T1 second) { base.Set(first); Second = second; }
    }
    
    public abstract class GenericEvent<T, T1, T2> : GenericEvent<T, T1>
    {
        public GenericEvent() { }
        public GenericEvent(T first, T1 second, T2 third) : base (first, second)
        {
            Third = third;
        }

        public T2 Third { get; private set; }
        public void Set(T first, T1 second, T2 third) { base.Set(first, second); Third = third; }
    }
}
