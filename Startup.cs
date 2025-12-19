using ControlActividades.Recursos;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ControlActividades.Startup))]
namespace ControlActividades
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);

            // Registrar el nuevo proveedor de IDs
            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider),() => new CustomUserIdProvider());
            app.MapSignalR();

        }
    }
}
