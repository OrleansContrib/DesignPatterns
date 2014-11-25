using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using Orleans;
using Orleans.Host;

using Sample.Hubs;

namespace Sample.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ViewResult Observe()
        {
            return View();
        }

        [HttpGet]
        public ViewResult Spawn()
        {
            return View();
        }

        [HttpPost]
        public async Task<ViewResult> Spawn(int publishers)
        {
            if (!OrleansAzureClient.IsInitialized)
            {
                InitializeOrleansClient();
                InitializeHubClient();
            }

            await Init(publishers);

            return View("Observe");
        }

        static void InitializeOrleansClient()
        {
            var clientConfigFile = AzureConfigUtils.ClientConfigFileLocation;

            if (!clientConfigFile.Exists)
                throw new FileNotFoundException(clientConfigFile.FullName);

            OrleansAzureClient.Initialize(clientConfigFile);
        }

        static void InitializeHubClient()
        {
            HubClient.Initialize();
        }

        static Task Init(int publishers)
        {
            var activations = new List<Task>();
            
            foreach (var i in Enumerable.Range(1, publishers))
            {
                var activation = GrainFactory.GetGrain<IPublisher>(i);
                activations.Add(activation.Init());
            }

            return Task.WhenAll(activations.ToArray());
        }
    }
}