using ControlActividades.Models;
using ControlActividades.Models.db;
using System;
using System.Security.Claims;
using System.Web.Mvc;

namespace ControlActividades.Filters
{
    public class AdminAuditLogAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var user = filterContext.HttpContext.User as ClaimsPrincipal;

            if (user == null)
                return;

            // Verificar si está impersonando
            var isImpersonating = user.HasClaim("IsImpersonating", "true");

            if (!isImpersonating)
                return;

            var adminId = user.FindFirst("AdminId")?.Value;
            var adminEmail = user.FindFirst("AdminEmail")?.Value;

            var impersonatedId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var impersonatedEmail = user.FindFirst(ClaimTypes.Email)?.Value;

            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;

            var ip = filterContext.HttpContext.Request.UserHostAddress;

            using (var db = new ApplicationDbContext())
            {
                db.tbAuditoria.Add(new tbAuditoria
                {
                    AdminId = adminId,
                    AdminEmail = adminEmail,
                    UsuarioImpersonadoId = impersonatedId,
                    UsuarioImpersonadoEmail = impersonatedEmail,
                    Accion = action,
                    Controlador = controller,
                    Descripcion = $"El administrador ejecutó {action} en {controller}",
                    DateUtc = DateTime.UtcNow,
                    DireccionIp = ip
                });

                db.SaveChanges();
            }
        }
    }
}