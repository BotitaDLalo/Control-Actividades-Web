using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Filters
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Está logueado pero no tiene permiso = 403
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/Error403.cshtml"
                };

                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Error", action = "Forbidden" }
                    )
                );
            }
            else
            {
                // No está logueado = login normal
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}