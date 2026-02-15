using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ControlActividades.Exceptions;
using ControlActividades.Interfaces.Actividades;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using ControlActividades.Services.Actividades;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;



namespace ControlActividades.Controllers
{
    [RoutePrefix("api/Actividades")]
    public class ActividadesApiController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private ActividadesApiService _actividadesApiService;
        private NotificacionesService _notifServ;
        public ActividadesApiController()
        {
        }

        public ActividadesApiController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg, NotificacionesService notifServ)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Ns = notifServ;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.Current.GetOwinContext().Get<RoleManager<IdentityRole>>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public ApplicationDbContext Db
        {
            get
            {
                return _db ?? (_db = new ApplicationDbContext());
            }
            private set
            {
                _db = value;
            }
        }

        public FuncionalidadesGenerales Fg
        {
            get
            {
                return _fg ?? (_fg = new FuncionalidadesGenerales());
            }
            set
            {
                _fg = value;
            }
        }
        public NotificacionesService Ns
        {
            get
            {
                return _notifServ ?? (_notifServ = new NotificacionesService(Db, new FCMService()));
            }
            private set
            {
                _notifServ = value;
            }
        }


        public ActividadesApiService ActividadesApiService
        {
            get
            {
                return _actividadesApiService ?? (_actividadesApiService = new ActividadesApiService());
            }
            private set
            {
                _actividadesApiService = value;
            }
        }

        // Compatibilidad: permitir que clientes pidan envíos por alumno usando la ruta /api/Actividades/ObtenerEnviosActividadesAlumno
        [HttpGet]
        [Route("ObtenerEnviosActividadesAlumno")]
        public async Task<IHttpActionResult> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            try
            {
                var datosAlumnoActividad = await ActividadesApiService.ObtenerEnviosActividadesAlumno(ActividadId, AlumnoId);

                var result = new
                {
                    EntregaActividadAlumnoId = datosAlumnoActividad.EntregaActividadAlumnoId,
                    FechaEntrega = datosAlumnoActividad.FechaEntrega,
                    EstadoEntregaId = datosAlumnoActividad.EstadoEntregaId,
                    Entregables = datosAlumnoActividad.Entregables
                };

                return Ok(result);
            }
            catch (EntregableNoEncontradoException)
            {
                return Content(HttpStatusCode.NotFound, new { mensaje = "No se encontró registro de entrega para el alumno y la actividad." });

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public async Task<List<object>> ConsultaActividades()
        {
            try
            {
                var listaActividades = await Db.tbActividades
                    .Select(a => new
                    {
                        actividadId = a.ActividadId,
                        nombreActividad = a.NombreActividad,
                        descripcionActividad = a.Descripcion,
                        fechaCreacionActividad = a.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),
                        //fechaLimiteActividad = a.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
                        fechaLimiteActividad = a.FechaLimite.HasValue
                            ? a.FechaLimite.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                            : null,
                        //tipoActividadId = a.TipoActividadId,
                        puntaje = a.Puntaje,
                        materiaId = a.MateriaId
                    })
                    .Cast<object>()
                    .ToListAsync();

                return listaActividades;
            }
            catch (Exception)
            {
                return new List<object>();
            }
        }

        // Cambiar el tipo de retorno a IHttpActionResult<List<object>> para ser consistente
        public async Task<IHttpActionResult> ConsultarActividadesCreadas()
        {
            try
            {
                var lsActividades = await Db.tbActividades.Select(a => new
                {
                    a.ActividadId,
                    a.NombreActividad
                }).ToListAsync();

                return Ok(lsActividades); // Retorna la lista de actividades creadas
            }
            catch (Exception)
            {
                return BadRequest("Ocurrió un error al obtener las actividades creadas.");
            }
        }

        public async Task<IHttpActionResult> ConsultaActividadesPorMateria(int materiaId)
        {
            try
            {
                var q = Db.tbActividades.Where(a => a.MateriaId == materiaId);

                var actividades = await q.ToListAsync();

                var listaActividades = actividades.Select(a => new
                {
                    ActividadId = a.ActividadId,
                    NombreActividad = a.NombreActividad,
                    DescripcionActividad = a.Descripcion,
                    FechaCreacionActividad = a.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),
                    //FechaLimiteActividad = a.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
                    FechaLimiteActividad = a.FechaLimite.HasValue
                        ? a.FechaLimite.Value.ToString("yyyy-MM-ddTHH:mm:ss")
                        : null,
                    Puntaje = a.Puntaje,
                    Enviado = a.Enviado,
                    FechaProgramada = a.FechaProgramada,
                    MateriaId = a.MateriaId
                }).ToList();


                return Ok(listaActividades);
            }
            catch (Exception ex)
            {
                // Return server error with details to help debugging from client
                return Content(HttpStatusCode.InternalServerError, new { mensaje = $"Error al obtener actividades: {ex.Message}", detalle = ex.ToString() });
            }
        }


        [HttpGet]
        [Route("ObtenerActividadesPorMateria")]
        public async Task<IHttpActionResult> ObtenerActividadesPorMateria(int materiaId)
        {

            try
            {
                //var lsActividades = await ConsultaActividadesPorMateria(materiaId);

                var lsActividades = await ActividadesApiService.ObtenerActividadesPorMateria(materiaId);

                return Ok();
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { e.Message });
            }

        }



        // El tipo de retorno debe ser IHttpActionResult<List<object>> porque estamos devolviendo una lista de objetos
        //[HttpGet]
        //[Route("ObtenerActividades")]
        //public async Task<IHttpActionResult> ObtenerActividades()
        //{
        //    try
        //    {
        //        var lsActividades = await ConsultaActividades();

        //        return Ok(lsActividades); // Retorna la lista obtenida de ConsultaActividades
        //    }
        //    catch (Exception e)
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { e.Message }); // En caso de error, retornamos el mensaje de la excepción
        //    }
        //}



        // Obtener una actividad específica
        //[HttpGet]
        //[Route("ObtenerActividad")]
        //public async Task<IHttpActionResult> ObtenerActividad(int id)
        //{
        //    var activity = await Db.tbActividades.FindAsync(id);
        //    if (activity == null) return Content(HttpStatusCode.NotFound, "Actividad no encontrada"); // Retorna un mensaje adecuado si no se encuentra la actividad

        //    return Ok(activity); // Si la actividad se encuentra, la retornamos
        //}

        [HttpPost]
        [Route("CrearActividad")]
        public async Task<IHttpActionResult> CrearActividad([FromBody] tbActividades nuevaActividad)
        {
            try
            {
                await ActividadesApiService.CrearActividad(nuevaActividad);

                return Ok(new { mensaje = "Actividad creada con éxito", actividadId = nuevaActividad.ActividadId });
            }
            catch (ActividadException e)
            {
                var mensaje = e.Message;
                return BadRequest(mensaje);
            }
            catch (DbUpdateException dbEx)
            {
                var mensaje = $"Error al actualizar la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return Content(HttpStatusCode.InternalServerError, mensaje);
            }
            catch (Exception ex)
            {
                var mensaje = $"Error inesperado: {ex.Message}";
                return Content(HttpStatusCode.InternalServerError, mensaje);
            }

            finally
            {
                await Ns.NotificacionCrearActividad(nuevaActividad);
            }
        }



        [HttpPut]
        [Route("ActualizarActividad")]
        public async Task<IHttpActionResult> ActualizarActividad(int id, tbActividades updatedActivity)
        {
            try
            {
                var actividad = await ActividadesApiService.ActualizarActividad(id, updatedActivity);

                return Ok(actividad);
            }
            catch (ActividadException e)
            {
                var mensaje = e.Message;
                return Content(HttpStatusCode.NotFound, mensaje);
            }
        }

        //[HttpPost]
        //[Route("TogglePermitirEntregasTarde")]
        //public async Task<IHttpActionResult> TogglePermitirEntregasTarde(int actividadId, bool permitir)
        //{
        //    try
        //    {
        //        var activity = await Db.tbActividades.FindAsync(actividadId);
        //        if (activity == null) return Content(HttpStatusCode.NotFound, new { mensaje = "Actividad no encontrada" });

        //        activity.PermitirEntregasTarde = permitir;
        //        await Db.SaveChangesAsync();

        //        return Ok(new { actividadId = actividadId, permitir = permitir });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(HttpStatusCode.InternalServerError, new { mensaje = ex.Message });
        //    }
        //} 


        //[HttpDelete("EliminarActividad/{id}")]
        [HttpDelete]
        [Route("EliminarActividad")]
        public async Task<IHttpActionResult> EliminarActividad(int id)
        {
            try
            {
                await ActividadesApiService.EliminarActividad(id);

                return Ok();
            }
            catch (ActividadException e)
            {
                var mensaje = e.Message;
                return BadRequest(mensaje);
            }
            catch (DbUpdateException dbEx)
            {
                var mensaje = $"Error al actualizar la base de datos: {dbEx.InnerException?.Message ?? dbEx.Message}";
                return Content(HttpStatusCode.InternalServerError, mensaje);
            }
            catch (Exception ex)
            {
                var mensaje = $"Error inesperado: {ex.Message}";
                return Content(HttpStatusCode.InternalServerError, mensaje);
            }
        }



        [HttpGet]
        [Route("ObtenerAlumnosEntregables")]
        public async Task<IHttpActionResult> ObtenerAlumnosEntregables(int actividadId)
        {
            try
            {
                var respuestaAlumnos = await ActividadesApiService.ObtenerAlumnosEntregables(actividadId);

                return Ok(respuestaAlumnos);
            }
            catch (Exception e)
            {
                return BadRequest($"Error: {e.Message}");
            }
        }

        [HttpPost]
        [Route("AsignarCalificacion")]
        public async Task<IHttpActionResult> AsignarCalificacion([FromBody] AsignarCalificacionPeticion asignarCalificacion)
        {
            try
            {
                var entregableId = asignarCalificacion.EntregableId;
                var calificacion = asignarCalificacion.Calificacion;


                //var entregable = Db.tbEntregables.FirstOrDefault(a => a.EntregableId == entregableId);

                //if (entregable == null) return BadRequest();
                await ActividadesApiService.AsignarCalificacion(entregableId, calificacion);

                return Ok();

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("QuitarCalificacion")]
        public async Task<IHttpActionResult> QuitarCalificacion([FromBody] QuitarCalificacionPeticion peticion)
        {
            try
            {
                await ActividadesApiService.QuitarCalificacion(peticion.EntregableId);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }

                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
