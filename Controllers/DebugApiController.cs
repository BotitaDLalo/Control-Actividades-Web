using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using ControlActividades;
using ControlActividades.Models;
using ControlActividades.Models.db;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/Debug")]
    public class DebugApiController : ApiController
    {
        private ApplicationDbContext _db;
        public ApplicationDbContext Db => _db ?? (_db = new ApplicationDbContext());

        [HttpGet]
        [Route("EntregasActividad")]
        public async Task<IHttpActionResult> EntregasActividad(int actividadId)
        {
            try
            {
                var entregas = await Db.tbEntregaActividadAlumno
                    .Where(e => e.ActividadId == actividadId)
                    .Include(e => e.tbEntregables)
                    .ToListAsync();

                var lista = new List<object>();
                foreach (var e in entregas)
                {
                    foreach (var ent in e.tbEntregables ?? new List<tbEntregables>())
                    {
                        lista.Add(new
                        {
                            EntregaActividadAlumnoId = e.EntregaActividadAlumnoId,
                            e.ActividadId,
                            e.AlumnoId,
                            e.FechaEntrega,
                            e.EstadoEntregaId,
                            EntregableId = ent.EntregableId,
                            ent.TipoEntregaId,
                            ent.Contenido,
                            ent.Calificacion
                        });
                    }
                }

                return Ok(lista);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { mensaje = ex.Message });
            }
        }
    }
}
