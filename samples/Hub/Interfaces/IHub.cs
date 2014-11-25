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

        Task Add(Notification[] notifications);
        Task Add(Notification notification);
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
    public class Notification
    {
        public readonly string Sender;
        public readonly long Id;
        public readonly DateTime Time;

        public Notification(string sender, long id, DateTime time)
        {
            Sender = sender;
            Id = id;
            Time = time;
        }
    }
}
