using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;
using Orleans.Placement;

namespace Sample
{
    [ExtendedPrimaryKey]
    [PreferLocalPlacement]
    public interface IHubBuffer : IGrainWithStringKey
    {
        Task Init();
        
        Task Publish(Event e);
    }

    [ExtendedPrimaryKey]
    [PreferLocalPlacement]
    public interface IHub : IGrainWithStringKey
    {
        Task Init();
        
        Task Subscribe(IHubObserver observer);
        Task Unsubscribe(IHubObserver observer);
        
        Task Publish(Notification[] notifications);
    }

    public interface IHubObserver : IGrainObserver
    {
        void On(Notification[] notifications, string hub);
    }

    [Immutable]
    [Serializable]
    public class Event
    {
        public readonly string Sender;
        public readonly long Id;
        public readonly DateTime Time;

        public Event(string sender, long id, DateTime time)
        {
            Sender = sender;
            Id = id;
            Time = time;
        }
    }

    [Immutable]
    [Serializable]
    public class Notification
    {
        public readonly Event Event;
        public readonly DateTime Received;

        public Notification(Event e, DateTime received)
        {
            Event = e;
            Received = received;
        }
    }
}
