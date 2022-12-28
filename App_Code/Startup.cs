using Owin;
using Microsoft.Owin;
[assembly: OwinStartup(typeof(Muinternight.Startup))]

namespace Muinternight {
    public class Startup {
        public void Configuration(IAppBuilder app) {
            app.MapSignalR();
        }
    }
}