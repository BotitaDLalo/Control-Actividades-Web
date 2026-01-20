using ControlActividades.Dtos.Migracion;
using ControlActividades.Models;
using ControlActividades.Services;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/migracion")]
    public class MigracionUsuariosController : ApiController
    {

        private readonly IMigracionUsuariosService _migracionService;

        public MigracionUsuariosController()
        {
            var context = HttpContext.Current.GetOwinContext();
            var userManager = context.GetUserManager<ApplicationUserManager>();
            var dbContext = context.Get<ApplicationDbContext>();

            _migracionService = new MigracionUsuariosService(userManager, dbContext);
        }

        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            return Ok("Api funcionando");
        }

        [HttpPost]
        [Route("usuarios")]
        public async Task<IHttpActionResult> MigrarUsuarios(MigracionUsuariosDto dto)
        {
            if(dto?.Usuarios == null || !dto.Usuarios.Any())
            {
                return BadRequest("No hay usuarios.");
            }

            var resultado = await _migracionService.MigrarUsuariosAsync(dto.Usuarios);

            return Ok(resultado);
        }
    }
}