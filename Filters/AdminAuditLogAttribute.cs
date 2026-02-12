using ControlActividades.Models;
using ControlActividades.Models.db;
using NPOI.SS.Formula.Functions;
using System;
using System.Linq;
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
            string impersonatedEmail = null;
            
            using (var db = new ApplicationDbContext())
            {
                var usuario = db.Users.FirstOrDefault(u => u.Id == impersonatedId);
                if (usuario != null)
                    impersonatedEmail = usuario.Email;
            }

            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var action = filterContext.ActionDescriptor.ActionName;

            var request = filterContext.HttpContext.Request;
            string ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = request.ServerVariables["REMOTE_ADDR"];
            }

            // Si aún viene null, evitar guardar null
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "No disponible";
            }

            if (!string.IsNullOrEmpty(ipAddress) && ipAddress.Contains(","))
            {
                ipAddress = ipAddress.Split(',')[0];
            }

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
                    DireccionIp = ipAddress
                });

                db.SaveChanges();
            }
        }
    }
}