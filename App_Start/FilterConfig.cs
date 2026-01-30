using System.Web;
using System.Web.Mvc;
using ControlActividades.Filters;

namespace ControlActividades
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // Global filter to handle antiforgery token mismatches gracefully
            filters.Add(new AntiForgeryExceptionFilter());
            // Require authentication globally; actions/controllers marked with [AllowAnonymous]
            // (e.g., Account.Login/Register) will remain accessible.
            filters.Add(new System.Web.Mvc.AuthorizeAttribute());
        }
    }
}
