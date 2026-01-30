using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Data.Entity;
using ControlActividades.Models.db;
using ControlActividades.Models;

namespace ControlActividades.Controllers.Alumnos
{
    [RoutePrefix("api/Alumnos")]
    public class AlumnoApiController : ApiController
    {
        private ApplicationDbContext _db;
        public ApplicationDbContext Db => _db ?? (_db = new ApplicationDbContext());

        // Compatibilidad: permitir que clientes pidan envíos por alumno usando la ruta /api/Alumnos/ObtenerEnviosActividadesAlumno
        [HttpGet]
        [Route("ObtenerEnviosActividadesAlumno")]
        public async Task<IHttpActionResult> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            try
            {
                var datosAlumnoActividad = await Db.tbEntregaActividadAlumno.FirstOrDefaultAsync(a => a.ActividadId == ActividadId && a.AlumnoId == AlumnoId);
                if (datosAlumnoActividad == null)
                    return Content(HttpStatusCode.NotFound, new { mensaje = "No se encontró registro de entrega para el alumno y la actividad." });

                var entregaActividadId = datosAlumnoActividad.EntregaActividadAlumnoId;
                var fechaEntrega = datosAlumnoActividad?.FechaEntrega;

                var lsEntregas = await Db.tbEntregables.Where(a => a.EntregaActividadAlumnoId == entregaActividadId)
                    .Select(e => new
                    {
                        e.EntregableId,
                        e.TipoEntregaId,
                        e.Contenido,
                        Calificacion = e.Calificacion ?? 0,
                        Comentario = e.Comentario
                    }).ToListAsync();

                var result = new
                {
                    EntregaActividadAlumnoId = entregaActividadId,
                    FechaEntrega = fechaEntrega,
                    EstadoEntregaId = datosAlumnoActividad.EstadoEntregaId,
                    Entregables = lsEntregas
                };

                return Ok(result);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
