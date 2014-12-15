using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;

namespace Sample
{
    [Reentrant]
    public class HubGrain : Grain, IHub
    {
        ObserverSubscriptionManager<IHubObserver> listeners;

        public override Task ActivateAsync()
        {
            listeners = new ObserverSubscriptionManager<IHubObserver>();
            return TaskDone.Done;
        }

        public Task Init()
        {
            return TaskDone.Done;
        }

        public Task Subscribe(IHubObserver observer)
        {
            listeners.Subscribe(observer);
            return TaskDone.Done;
        }

        public Task Unsubscribe(IHubObserver observer)
        {
            listeners.Unsubscribe(observer);
            return TaskDone.Done;
        }

        public Task Publish(Notification[] notifications)
        {
            if (listeners.Count > 0)
                listeners.Notify(x => x.On(notifications, HubGateway.LocalHubId()));

            return TaskDone.Done;
        }
    }
}