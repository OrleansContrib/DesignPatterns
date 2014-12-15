using System;
using System.Linq;

using Owin;
using Microsoft.Owin;
[assembly: OwinStartup(typeof(Sample.Startup))]

namespace Sample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}