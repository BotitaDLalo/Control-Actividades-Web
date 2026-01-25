using ControlActividades.Dtos.Migracion;
using ControlActividades.Migracion;
using ControlActividades.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Linq;
using System.Web.Http;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/migracion")]
    public class MigracionUsuariosController : ApiController
    {
        [HttpPost]
        [Route("usuarios")]
        public IHttpActionResult MigrarUsuarios(MigracionUsuariosDto dto)
        {
            if(dto?.Usuarios == null || !dto.Usuarios.Any())
            {
                return BadRequest("No hay usuarios.");
            }

            try
            {
                var migrator = new BulkIdentityMigrator();
                migrator.MigrarUsuarios(dto.Usuarios);
                return Ok("Usuarios migrados correctamente");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            
        }
    }
}