using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace ControlActividades
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuración y servicios de Web API

            // Rutas de Web API
            config.MapHttpAttributeRoutes();

            // Support routes that include the action name: /api/{controller}/{action}
            config.Routes.MapHttpRoute(
                name: "ApiWithAction",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Fallback route for IA controller specifically (keeps compatibility)
            config.Routes.MapHttpRoute(
                name: "IAApi",
                routeTemplate: "api/IA/{action}",
                defaults: new { controller = "IA", action = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
