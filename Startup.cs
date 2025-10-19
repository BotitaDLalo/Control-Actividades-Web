using Microsoft.Owin;
using Owin;
using System;

[assembly: OwinStartupAttribute(typeof(ControlActividades.Startup))]
namespace ControlActividades
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
