using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ControlActividades.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Error/Error403.cshtml"
                };

                filterContext.HttpContext.Response.StatusCode = 403;
                filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}