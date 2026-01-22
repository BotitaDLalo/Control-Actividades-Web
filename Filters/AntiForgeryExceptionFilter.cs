using System;
using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Filters
{
    public class AntiForgeryExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null) return;

            var ex = filterContext.Exception;
            if (ex == null) return;

            if (ex.GetType().FullName == "System.Web.Mvc.HttpAntiForgeryException" || ex.GetType().FullName == "System.Web.Helpers.HttpAntiForgeryException")
            {
                try
                {
                    // Remove antiforgery cookie so a fresh token is generated
                    var cookieName = "__RequestVerificationToken";
                    var reqCookie = filterContext.HttpContext.Request.Cookies[cookieName];
                    if (reqCookie != null)
                    {
                        var expired = new HttpCookie(cookieName) { Expires = DateTime.UtcNow.AddDays(-1) };
                        filterContext.HttpContext.Response.Cookies.Add(expired);
                    }
                }
                catch { }

                // Redirect back to the same URL (GET) so the form can be re-rendered with a new token
                var redirectUrl = filterContext.HttpContext.Request.Url?.AbsolutePath ?? "/";
                filterContext.Result = new RedirectResult(redirectUrl);
                filterContext.ExceptionHandled = true;
            }
        }
    }
}
