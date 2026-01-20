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
using System.Web.Helpers;
using System.Security.Claims;
using System.Web.Optimization;
using System.Web.Routing;
using System.Data.Entity;
using ControlActividades.Migrations;
using ControlActividades.Models;
using System.Diagnostics;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

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
            // Configure AntiForgery to work with ClaimsIdentity (use NameIdentifier claim)
            try
            {
                AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            }
            catch { }
  
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
                
                // Developer convenience: if debugger is attached and the request is the root 
                // of the application, sign out any existing authentication cookie so the
                // app always starts at the login page. This avoids stale sessions during development.
                try
                {
                    if (Debugger.IsAttached && HttpContext.Current != null && HttpContext.Current.Request != null)
                    {
                        var appPath = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath ?? "";
                        if (appPath == "~/")
                        {
                            try
                            {
                                var auth = HttpContext.Current.GetOwinContext().Authentication;
                                if (auth != null)
                                {
                                    auth.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                }
                            }
                            catch { /* no-op - avoid breaking app start in dev */ }
                        }
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
