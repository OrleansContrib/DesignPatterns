using System;

namespace ObserverPattern
{
    using ObserverInterfaces;

    /// <summary>
    /// Orleans test silo host
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            // The Orleans silo environment is initialized in its own app domain in order to more
            // closely emulate the distributed situation, when the client and the server cannot
            // pass data via shared memory.
            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup
            {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args,
            });

            Orleans.OrleansClient.Initialize("DevTestClientConfiguration.xml");

            // Observer

            IDoStuff grain = DoStuffFactory.GetGrain(0);

            var theObserver = new TheObserver();
            var obj = ObserveFactory.CreateObjectReference(theObserver).Result; // factory from IObserve

            grain.SubscribeForUpdates(obj);
            

            // close down

            Console.WriteLine("Orleans Silo is running.\nPress Enter to terminate...");
            Console.ReadLine();

            hostDomain.DoCallBack(ShutdownSilo);
        }


        // class for handling updates from grain
        private class TheObserver : IObserve
        {
            // Receive updates 
            public void StuffUpdate(int data)
            {
                Console.WriteLine("New stuff: {0}", data);
            }
        } 



        static void InitSilo(string[] args)
        {
            hostWrapper = new OrleansHostWrapper(args);

            if (!hostWrapper.Run())
            {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        static void ShutdownSilo()
        {
            if (hostWrapper != null)
            {
                hostWrapper.Dispose();
                GC.SuppressFinalize(hostWrapper);
            }
        }

        private static OrleansHostWrapper hostWrapper;
    }
}
