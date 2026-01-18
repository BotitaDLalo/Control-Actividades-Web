using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ControlActividades.Dtos.Migracion;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/migracion")]
    public class MigracionUsuariosController : ApiController
    {
        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            return Ok("Api funcionando");
        }

        [HttpPost]
        [Route("usuarios")]
        public IHttpActionResult MigrarUsuarios(MigracionUsuariosDto dto)
        {
            if(dto == null || dto.Usuarios == null || !dto.Usuarios.Any())
            {
                return BadRequest("El objeto DTO no puede ser nulo.");
            }

            return Ok(new
            {
                TotalRecibidos = dto.Usuarios.Count
            });
        }
    }
}