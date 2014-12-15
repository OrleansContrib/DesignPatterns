using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR;

using Orleans.Runtime;

namespace Sample.Hubs
{
    public class Relay : Hub
    {}

    public class HubClient : IHubObserver
    {
        static IHubObserver proxy;
        static IHubObserver client;
        static IHubConnectionContext<dynamic> clients;

        public static void Initialize()
        {
            clients = GlobalHost.ConnectionManager.GetHubContext<Relay>().Clients;

            client = new HubClient();
            proxy  = Task.Run(()=> HubObserverFactory.CreateObjectReference(client)).Result;

            Task.Run(() => Subscribe())
                .Wait();

            Task.Run(() => Resubscribe());
        }

        static async Task Subscribe()
        {
            var orleans = OrleansManagementGrainFactory.GetGrain(0);
            var hosts = await orleans.GetHosts(true);

            foreach (var silo in hosts)
            {
                var address = silo.Key;
                var status = silo.Value;

                if (status != SiloStatus.Active)
                    continue;

                var hub = HubGateway.GetHub(address.Endpoint);

                try
                {
                    await hub.Subscribe(proxy);
                }
                catch (AggregateException e)
                {
                    if (IsAlreadySubscribed(e))
                        continue;

                    throw;
                }
            }
        }

        static bool IsAlreadySubscribed(AggregateException e)
        {
            var ex = e.InnerException as OrleansException;
            return ex != null && ex.Message.Contains("Cannot subscribe already subscribed observer");
        }

        static async Task Resubscribe()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(120));
                await Subscribe();
            }
        }

        public void On(Notification[] notifications, string hub)
        {
            clients.All.receive(notifications.Select(notification => Format(notification, hub)));
        }

        static string Format(Notification notification, string hub)
        {
            var @event = notification.Event;
            var latency = (DateTime.Now - notification.Received).TotalSeconds;

            return string.Format("{0} published {1} on {2} via {4}. Latency: {3} s",
                                 @event.Sender, @event.Id, @event.Time, latency, hub);
        }
    }
}