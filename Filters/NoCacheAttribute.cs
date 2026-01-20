using System;
using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Filters
{
    public class NoCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {

            var response = context.HttpContext.Response;

            //Evita caché del navegador/del proxy. Obliga a pedir siempre la vista al servidor
            response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            response.Cache.SetValidUntilExpires(false);
            response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.SetNoStore();

            base.OnResultExecuting(context);
        }
    }
}