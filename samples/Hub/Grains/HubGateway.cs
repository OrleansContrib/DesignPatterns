using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Orleans.Providers;

namespace Sample
{
    public class HubGateway : IBootstrapProvider
    {
        public async Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;           
            await GetLocalHub().Init();
        }

        public string Name
        {
            get; private set;
        }

        public static Task Publish(Event e)
        {
            return HubBufferFactory.GetGrain(0).Publish(e);
        }

        public static IHub GetHub(IPEndPoint endpoint)
        {
            return HubFactory.GetGrain(HubId(endpoint));
        }

        public static IHub GetLocalHub()
        {
            return GetHub(LocalEndPoint);
        }

        static string HubId(IPEndPoint endpoint)
        {
            Debug.Assert(endpoint != null);
            return "HUB" + endpoint.Address;
        }

        public static string LocalHubId()
        {
            return HubId(LocalEndPoint);
        }

        public static IPAddress LocalAddress()
        {
            return LocalEndPoint.Address;
        }

        public static IPEndPoint LocalEndPoint
        {
            get; set;
        }
    }
}
