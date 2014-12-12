using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Orleans;
using Orleans.Concurrency;

namespace Sample
{
    [Reentrant]
    public class HubBufferGrain : Grain, IHubBuffer
    {
        readonly TimeSpan flushPeriod = TimeSpan.FromSeconds(1);
        readonly TimeSpan keepAlivePeriod = TimeSpan.FromHours(12);

        IHub hub;
        Queue<Notification> buffer;

        public Task Init()
        {
            return TaskDone.Done;
        }

        public override Task ActivateAsync()
        {
            hub = HubGateway.GetLocalHub();
            buffer = new Queue<Notification>();

            RegisterTimer(KeepAlive, null, keepAlivePeriod, keepAlivePeriod);
            RegisterTimer(Flush, null, flushPeriod, flushPeriod);

            return TaskDone.Done;
        }

        Task KeepAlive(object arg)
        {
            DelayDeactivation(keepAlivePeriod);
            return TaskDone.Done;
        }

        Task Flush(object arg)
        {
            if (buffer.Count == 0)
                return TaskDone.Done;

            var notifications = buffer.ToArray();
            buffer.Clear();

            return hub.Publish(notifications);
        }

        public Task Publish(Event @event)
        {
            buffer.Enqueue(new Notification(@event, DateTime.Now));
            return TaskDone.Done;
        }
    }
}