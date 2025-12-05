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
            // Apply EF migrations automatically only in development (compilation debug="true").
            // This avoids applying migrations automatically in production environments.
            try
            {
                var isDebug = HttpContext.Current?.IsDebuggingEnabled ?? false;
                if (isDebug)
                {
                    Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>());
                }
            }
            catch
            {
                // If HttpContext is unavailable during some host scenarios, do not set initializer.
            }

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            // Force request/response encoding to UTF-8 to avoid mojibake
            try
            {
                HttpContext.Current.Request.ContentEncoding = Encoding.UTF8;
                HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
                HttpContext.Current.Response.HeaderEncoding = Encoding.UTF8;

                // Ensure Content-Type header includes charset
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
            }
            catch
            {
                // ignore in case HttpContext is not available
            }
        }
    }
}
