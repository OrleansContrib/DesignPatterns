using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Orleans;

namespace ObserverGrains
{
    using ObserverInterfaces;

    public class MyGrain : Orleans.GrainBase
    {
        // manage subscriptions 
        private ObserverSubscriptionManager<IObserve> subscribers;

        public override Task ActivateAsync()
        {
            subscribers = new ObserverSubscriptionManager<IObserve>();

            // set up timer to simulate events to subscribe to
            RegisterTimer(SendOutUpdates, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

            return TaskDone.Done;
        }


        // accept subscriptions 
        public Task SubscribeForUpdates(IObserve subscriber)
        {
            // add new subscriber to list of subscribers
            subscribers.Subscribe(subscriber);
            return TaskDone.Done;
        }


        // Notify subscribers 
        private Task SendOutUpdates(object _)
        {
            subscribers.Notify( s => s.StuffUpdate(DateTime.Now.Millisecond));

            return TaskDone.Done;
        }
    }
}

