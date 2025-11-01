using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Microsoft.Owin.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;


namespace ControlActividades
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            string json = System.Configuration.ConfigurationManager.AppSettings["FIREBASE_SERVICE_JSON"];
            if (!string.IsNullOrEmpty(json))
            {
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
                {
                    var credential = GoogleCredential.FromStream(stream);

                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential
                    });
                }
            }
        }
    }
}
