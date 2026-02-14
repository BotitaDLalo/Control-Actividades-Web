using ControlActividades.Recursos;
using Hangfire;
using Hangfire.SqlServer;
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

            // Usar cadena de conexión actual
            GlobalConfiguration.Configuration
                .UseSqlServerStorage("DefaultConnection");

            // Activar servidor
            app.UseHangfireServer();

            // Activar dashboard (panel visual)
            app.UseHangfireDashboard("/hangfire");

        }
    }
}
