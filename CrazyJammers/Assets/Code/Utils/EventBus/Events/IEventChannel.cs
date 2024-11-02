using Code.Core.Events;

namespace Code.Utility.Events
{
    public interface IEventChannel
    {
        void Publish(IBusEvent busEvent);
        void RemoveAllNonPermanentCallbacks();
    }
}