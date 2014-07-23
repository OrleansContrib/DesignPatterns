using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace ObserverInterfaces
{
    /// <summary>
    /// Orleans grain communication interface IMyGrain1
    /// </summary>
    public interface IDoStuff : Orleans.IGrain
    {
        Task SubscribeForUpdates(IObserve subscriber);

    }


    public interface IObserve : Orleans.IGrainObserver
    {
        void StuffUpdate(int data);
    }
}
