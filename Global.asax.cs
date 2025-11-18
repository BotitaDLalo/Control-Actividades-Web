using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
