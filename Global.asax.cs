using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using ControlActividades.Migrations;
using ControlActividades.Models;

namespace ControlActividades
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            try
            {

                Database.SetInitializer<ApplicationDbContext>(null);
            }
            catch
            {

            }

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

  
            try
            {
                ControlActividades.Services.ScheduledPublishingService.Start();
            }
            catch { }
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {

            try
            {
                HttpContext.Current.Request.ContentEncoding = Encoding.UTF8;
                HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                HttpContext.Current.Response.HeaderEncoding = Encoding.UTF8;


                var resp = HttpContext.Current.Response;
                if (!string.IsNullOrEmpty(resp.ContentType))
                {
                    if (!resp.ContentType.ToLower().Contains("charset"))
                    {
                        resp.ContentType = resp.ContentType.Split(';')[0] + "; charset=utf-8";
                    }
                }
                else
                {
                    resp.ContentType = "text/html; charset=utf-8";
                }

                // During development disable client-side caching so changes to scripts/styles/views
                // are reflected immediately when debugging (prevents stale cached assets in browser)
                try
                {
                    // Only apply when a debugger is attached (development) to avoid forcing no-cache in production
                    if (System.Diagnostics.Debugger.IsAttached && HttpContext.Current != null)
                    {
                        var cache = HttpContext.Current.Response.Cache;
                        cache.SetCacheability(System.Web.HttpCacheability.NoCache);
                        cache.SetNoStore();
                        cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                        cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                    }
                }
                catch { }
            }
            catch
            {

            }
        }
    }
}
