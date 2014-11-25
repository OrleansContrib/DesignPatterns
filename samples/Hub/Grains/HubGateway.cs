using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Orleans.Providers;

namespace Sample
{
    public class HubGateway : IBootstrapProvider
    {
        static Func<int> nextBuffer;

        public Task Init(string name, IProviderRuntime providerRuntime, IProviderConfiguration config)
        {
            Name = name;
            
            nextBuffer = CreateLoadBalanceStrategy(config.Properties);
            
            return InitPool(config.Properties);
        }

        public string Name
        {
            get; private set;
        }

        static async Task InitPool(IDictionary<string, string> properties)
        {
            await GetLocalHub().Init();

            var poolSize = int.Parse(properties["PoolSize"]);
            foreach (var id in Enumerable.Range(1, poolSize))
                await GetLocalHubBuffer(id).Init();
        }

        static Func<int> CreateLoadBalanceStrategy(IDictionary<string, string> properties)
        {
            var strategy = properties["LoadBalance"];
            var poolSize = int.Parse(properties["PoolSize"]);

            if (strategy == "RoundRobin")
                return new RoundRobinStrategy(1, poolSize).Acquire;

            if (strategy == "Random")
                return new RandomStrategy(1, poolSize).Acquire;

            throw new ApplicationException("Unknown load-balance strategy: " + strategy);
        }

        public static Task Publish(Notification notification)
        {
            return ChooseNextLocalBuffer().Add(notification);
        }

        public static Task Publish(Notification[] notifications)
        {
            return ChooseNextLocalBuffer().Add(notifications);
        }

        static IHubBuffer ChooseNextLocalBuffer()
        {
            return GetLocalHubBuffer(nextBuffer());
        }

        public static IHub GetHub(IPEndPoint endpoint)
        {
            return HubFactory.GetGrain(HubId(endpoint));
        }

        public static IHub GetLocalHub()
        {
            return GetHub(LocalEndPoint);
        }

        static IHubBuffer GetLocalHubBuffer(int id)
        {
            return HubBufferFactory.GetGrain(LocalHubBufferId(id));
        }

        static string LocalHubBufferId(int id)
        {
            return LocalHubId() + ":<-" + id;
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

    public class RandomStrategy
    {
        readonly Random rand = new Random();
        
        readonly int start;
        readonly int end;

        public RandomStrategy(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        public int Acquire()
        {
            return rand.Next(start, end);
        }
    }

    public class RoundRobinStrategy
    {
        int last;

        readonly int start;
        readonly int end;

        public RoundRobinStrategy(int start, int end)
        {
            this.start = start;
            this.end = end;

            last = start - 1;
        }

        public int Acquire()
        {
            int current = last, before;

            do
            {
                before = current;

                var next = current + 1 <= end
                    ? current + 1
                    : start;

                current = Interlocked.CompareExchange(ref last, next, before);
            }
            while (before != current);

            return last;
        }
    }
}
