using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sample
{
    public interface IPublisher : Orleans.IGrainWithIntegerKey
    {
        Task Init();
    }
}
