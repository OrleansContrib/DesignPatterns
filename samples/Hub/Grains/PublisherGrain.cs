using System;
using System.Linq;
using System.Threading.Tasks;

using Orleans;

namespace Sample
{
    public class PublisherGrain : Grain, IPublisher
    {
        Task IPublisher.Init()
        {
            return TaskDone.Done;
        }

        public override Task ActivateAsync()
        {
            RegisterTimer(Publish, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            return TaskDone.Done;
        }

        Task Publish(object arg)
        {
            return HubGateway.Publish(Notification());
        }

        Notification Notification()
        {
            var senderId = ThisId() + "##" + HubGateway.LocalAddress();
            var notificationId = DateTime.Now.Ticks ^ ThisId();
            return new Notification(senderId, notificationId, DateTime.Now);
        }

        long ThisId()
        {
            return this.GetPrimaryKeyLong();
        }
    }
}
