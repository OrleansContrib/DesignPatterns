using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Sample
{
    public class PublisherGrain : Grain, IPublisher
    {
        static readonly Random rand = new Random();

        Task IPublisher.Init()
        {
            return TaskDone.Done;
        }

        public override Task ActivateAsync()
        {
            RegisterTimer(Publish, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(rand.Next(3, 10)));
            return TaskDone.Done;
        }

        Task Publish(object arg)
        {
            return HubGateway.Publish(Event());
        }

        Event Event()
        {
            var senderId = ThisId() + "##" + HubGateway.LocalAddress();
            var eventId = DateTime.Now.Ticks ^ ThisId();
            return new Event(senderId, eventId, DateTime.Now);
        }

        long ThisId()
        {
            return this.GetPrimaryKeyLong();
        }
    }
}
