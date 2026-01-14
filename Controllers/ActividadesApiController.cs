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
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
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
        public ActividadesApiController()
        {
        }

        // Compatibilidad: permitir que clientes pidan envíos por alumno usando la ruta /api/Actividades/ObtenerEnviosActividadesAlumno
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

                // Evitar seleccionar columnas que pueden no existir en instalaciones antiguas de la BD
                var lsEntregas = await Db.tbEntregables.Where(a => a.EntregaActividadAlumnoId == entregaActividadId)
                    .Select(e => new
                    {
                        e.EntregableId,
                        e.TipoEntregaId,
                        e.Contenido,
                        // FechaCalificado puede no existir en la BD en algunas instalaciones; omitimos su lectura aquí
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

        public ActividadesApiController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
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
                        fechaLimiteActividad = a.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
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
                // Use RequestContext principal (works in WebApi) to determine roles
                var principal = RequestContext?.Principal;
                bool esDocente = principal != null && (principal.IsInRole("Docente") || principal.IsInRole("Administrador"));

                var q = Db.tbActividades.Where(a => a.MateriaId == materiaId);
                if (!esDocente)
                {
                    // Para alumnos: mostrar únicamente actividades públicas o programadas cuya fecha ya llegó
                    q = q.Where(a => a.Enviado == true || (a.Enviado == null && a.FechaProgramada.HasValue && a.FechaProgramada.Value <= DateTime.Now));
                }

                var actividades = await q.ToListAsync();

                var listaActividades = actividades.Select(a => new
                {
                    ActividadId = a.ActividadId,
                    NombreActividad = a.NombreActividad,
                    Descripcion = a.Descripcion,
                    FechaCreacion = a.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),
                    FechaLimite = a.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
                    Puntaje = a.Puntaje,
                    Enviado = a.Enviado,
                    FechaProgramada = a.FechaProgramada,
                    MateriaId = a.MateriaId
                }).ToList();

                // Determine rolUsuario using the helper
                var rolUsuario = Fg.ObtenerRolUsuario(principal);

                return Ok(new { Actividades = listaActividades, RolUsuario = rolUsuario });
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
                var lsActividades = await ConsultaActividadesPorMateria(materiaId);
                return lsActividades;
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { e.Message });
            }

        }



        // El tipo de retorno debe ser IHttpActionResult<List<object>> porque estamos devolviendo una lista de objetos
        [HttpGet]
        [Route("ObtenerActividades")]
        public async Task<IHttpActionResult> ObtenerActividades()
        {
            try
            {
                var lsActividades = await ConsultaActividades();

                return Ok(lsActividades); // Retorna la lista obtenida de ConsultaActividades
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { e.Message }); // En caso de error, retornamos el mensaje de la excepción
            }
        }



        // Obtener una actividad específica
        [HttpGet]
        [Route("ObtenerActividad")]
        public async Task<IHttpActionResult> ObtenerActividad(int id)
        {
            var activity = await Db.tbActividades.FindAsync(id);
            if (activity == null) return Content(HttpStatusCode.NotFound, "Actividad no encontrada"); // Retorna un mensaje adecuado si no se encuentra la actividad

            return Ok(activity); // Si la actividad se encuentra, la retornamos
        }

        [HttpPost]
        [Route("CrearActividad")]
        public async Task<IHttpActionResult> CrearActividad([FromBody] tbActividades nuevaActividad)
        {
            try
            {
                int materiaId = nuevaActividad.MateriaId;
                // Verificar si la materia existe
                var materia = await Db.tbMaterias.FindAsync(materiaId);
                if (materia == null)
                {
                    return BadRequest("La materia asociada no existe.");
                }

                // Validar campos no nulos o con valores incorrectos
                if (string.IsNullOrWhiteSpace(nuevaActividad.NombreActividad))
                {
                    return BadRequest("El nombre de la actividad es obligatorio.");
                }

                if (nuevaActividad.FechaLimite == default(DateTime))
                {
                    return BadRequest("La fecha límite de la actividad es inválida.");
                }

                // Generar automáticamente la fecha de creación
                nuevaActividad.FechaCreacion = DateTime.Now;


                //nuevaActividad.TipoActividadId = 1;

                // Guardar la actividad en la base de datos
                Db.tbActividades.Add(nuevaActividad);
                await Db.SaveChangesAsync();

                return Ok(new { mensaje = "Actividad creada con éxito", actividadId = nuevaActividad.ActividadId });
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
                //await _ns.NotificacionCrearActividad(nuevaActividad);
            }
        }



        [HttpPut]
        [Route("ActualizarActividad")]
        public async Task<IHttpActionResult> ActualizarActividad(int id, tbActividades updatedActivity)
        {
            var dbActivity = await Db.tbActividades.FindAsync(id);
            if (dbActivity is null) return Content(HttpStatusCode.NotFound, "Actividad no encontrada");

            var prevEnviado = dbActivity.Enviado;

            dbActivity.NombreActividad = updatedActivity.NombreActividad ?? dbActivity.NombreActividad;
            dbActivity.Descripcion = updatedActivity.Descripcion ?? dbActivity.Descripcion;
            dbActivity.FechaLimite = updatedActivity.FechaLimite != default(DateTime) ? updatedActivity.FechaLimite : dbActivity.FechaLimite;
            dbActivity.Puntaje = updatedActivity.Puntaje;

            dbActivity.Enviado = updatedActivity.Enviado ?? dbActivity.Enviado;
            dbActivity.FechaProgramada = updatedActivity.FechaProgramada ?? dbActivity.FechaProgramada;

            await Db.SaveChangesAsync();

            // si cambió a publicado ahora -> asignar alumnos
            bool ahoraPublicado = (prevEnviado != true) && (dbActivity.Enviado == true || (dbActivity.Enviado == null && dbActivity.FechaProgramada.HasValue && dbActivity.FechaProgramada.Value <= DateTime.Now));
            if (ahoraPublicado)
            {
                var alumnosMateria = await Db.tbAlumnosMaterias.Where(am => am.MateriaId == dbActivity.MateriaId).Select(am => am.AlumnoId).ToListAsync();
                foreach (var alumnoId in alumnosMateria)
                {
                    //var existe = await Db.tbAlumnosActividades.AnyAsync(aa => aa.ActividadId == dbActivity.ActividadId && aa.AlumnoId == alumnoId);
                    //if (!existe)
                    //{
                    //    Db.tbAlumnosActividades.Add(new tbAlumnosActividades {
                    //        ActividadId = dbActivity.ActividadId,
                    //        AlumnoId = alumnoId,
                    //        FechaEntrega = DateTime.Now,
                    //        EstatusEntrega = false
                    //    });
                    //}
                }
                //await Db.SaveChangesAsync();
            }

            return Ok(dbActivity);
        }

        [HttpPost]
        [Route("TogglePermitirEntregasTarde")]
        public async Task<IHttpActionResult> TogglePermitirEntregasTarde(int actividadId, bool permitir)
        {
            try
            {
                var activity = await Db.tbActividades.FindAsync(actividadId);
                if (activity == null) return Content(HttpStatusCode.NotFound, new { mensaje = "Actividad no encontrada" });

                activity.PermitirEntregasTarde = permitir;
                await Db.SaveChangesAsync();

                return Ok(new { actividadId = actividadId, permitir = permitir });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { mensaje = ex.Message });
            }
        }


        //[HttpDelete("EliminarActividad/{id}")]
        [HttpDelete]
        [Route("EliminarActividad")]
        public async Task<IHttpActionResult> EliminarActividad(int id)
        {
            try
            {
                var activity = await Db.tbActividades.FirstOrDefaultAsync(a => a.ActividadId == id);

                if (activity is null) return BadRequest("Actividad no encontrada");

                //NO PERMITIR QUE SE ELIMINE LA ACTIVIDAD SI YA TIENE:
                /*
                 ->Entrega del alumno
                -> Calificacion
                 */

                //var alumnoActividad = await Db.tbAlumnosActividades.FirstOrDefaultAsync(a => a.ActividadId == activity.ActividadId);

                //if (alumnoActividad != null)
                //{


                //var entrega = await Db.tbEntregablesAlumno.Where(a => a.AlumnoActividadId == alumnoActividad.AlumnoActividadId).FirstOrDefaultAsync();
                //if (entrega != null)
                //{
                //    var calificacion = await Db.tbCalificaciones.FirstOrDefaultAsync(a => a.EntregaId == entrega.EntregaId);

                //    if (calificacion != null)
                //    {
                //        Db.tbCalificaciones.Remove(calificacion);
                //        Db.tbEntregablesAlumno.Remove(entrega);
                //        Db.tbAlumnosActividades.Remove(alumnoActividad);
                //    }
                //    else
                //    {
                //        Db.tbEntregablesAlumno.Remove(entrega);
                //        Db.tbAlumnosActividades.Remove(alumnoActividad);
                //    }
                //}
                //}



                var existeEntrega = Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == activity.ActividadId).Any();
                if (existeEntrega)
                    return BadRequest();


                Db.tbActividades.Remove(activity);
                await Db.SaveChangesAsync();

                return Ok();
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
                List<AlumnoEntregable> lsEntregables = new List<AlumnoEntregable>();
                RespuestaAlumnosEntregables respuestaAlumnos = new RespuestaAlumnosEntregables();

                //var lsAlumnosActividades = await Db.tbAlumnosActividades
                //    .Where(a => a.ActividadId == actividadId && a.EstatusEntrega == true)
                //    .Include(a => a.EntregablesAlumno)
                //    .Include(a => a.Actividades)
                //    .Include(a => a.Alumnos).ToListAsync();

                var lsAlumnosActividades = await Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == actividadId && a.EstadoEntregaId == 1)
                    .Include(a => a.tbAlumnos)
                    .Include(a => a.tbEntregables)
                    .ToListAsync();

                int puntaje = await Db.tbActividades.Where(a => a.ActividadId == actividadId).Select(a => a.Puntaje).FirstOrDefaultAsync();

                int totalEntregados = lsAlumnosActividades.Count;

                respuestaAlumnos.ActividadId = actividadId;
                respuestaAlumnos.Puntaje = puntaje;
                respuestaAlumnos.TotalEntregados = totalEntregados;

                foreach (var alumnoActividad in lsAlumnosActividades)
                {
                    AlumnoEntregable alumnoEntregable = new AlumnoEntregable();

                    //var alumno = alumnoActividad.Alumnos;
                    //var entregableAlumno = alumnoActividad.EntregablesAlumno;

                    var alumno = alumnoActividad.tbAlumnos;
                    var entregableAlumno = alumnoActividad.tbEntregables;


                    //if (alumno != null && entregableAlumno != null)
                    //{
                    //    var entregaId = entregableAlumno.EntregableId;

                    //    var alumnoId = alumno.AlumnoId;
                    //    var userId = alumno.UserId;
                    //    var nombres = alumno.Nombre;
                    //    var apellidoPaterno = alumno.ApellidoPaterno;
                    //    var apellidoMaterno = alumno.ApellidoMaterno;
                    //    var user = await UserManager.FindByIdAsync(userId ?? "");

                    //    if (user != null)
                    //    {
                    //        var userName = user.UserName;
                    //        alumnoEntregable.AlumnoId = alumnoId;
                    //        alumnoEntregable.NombreUsuario = userName ?? "";
                    //        alumnoEntregable.Nombres = nombres ?? "";
                    //        alumnoEntregable.ApellidoPaterno = apellidoPaterno ?? "";
                    //        alumnoEntregable.ApellidoMaterno = apellidoMaterno ?? "";
                    //    }

                    //    alumnoEntregable.FechaEntrega = alumnoActividad.FechaEntrega;

                    //    alumnoEntregable.EntregaId = entregableAlumno.EntregaId;
                    //    alumnoEntregable.Respuesta = entregableAlumno.Respuesta ?? "";

                    //    var calificacion = await Db.tbCalificaciones.Where(a => a.EntregaId == entregaId).FirstOrDefaultAsync();

                    //    alumnoEntregable.Calificacion = calificacion?.Calificacion ?? -1;

                    //    lsEntregables.Add(alumnoEntregable);
                    //}

                    var alumnoId = alumno.AlumnoId;
                    var userId = alumno.UserId;
                    var nombres = alumno.Nombre;
                    var apellidoPaterno = alumno.ApellidoPaterno;
                    var apellidoMaterno = alumno.ApellidoMaterno;
                    var user = await UserManager.FindByIdAsync(userId ?? "");




                    foreach (var entregable in entregableAlumno.ToList())
                    {

                        var userName = user.UserName;
                        alumnoEntregable.AlumnoId = alumnoId;
                        alumnoEntregable.NombreUsuario = userName ?? "";
                        alumnoEntregable.Nombres = nombres ?? "";
                        alumnoEntregable.ApellidoPaterno = apellidoPaterno ?? "";
                        alumnoEntregable.ApellidoMaterno = apellidoMaterno ?? "";


                        alumnoEntregable.FechaEntrega = alumnoActividad.FechaEntrega;


                        alumnoEntregable.EntregaId = entregable.EntregableId;


                        alumnoEntregable.Respuesta = entregable.Contenido;


                        alumnoEntregable.Calificacion = entregable.Calificacion ?? 0;


                        lsEntregables.Add(alumnoEntregable);
                    }

                }

                respuestaAlumnos.AlumnosEntregables = lsEntregables;


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
                var entregaId = asignarCalificacion.EntregaId;
                var fechaNuevaCalificacion = DateTime.Now;
                var nuevaCalificacion = asignarCalificacion.Calificacion;
                // Intentar actualizar el registro en la tabla tbEntregables (modelo actual)
                int idBuscado = asignarCalificacion.EntregableId != 0 ? asignarCalificacion.EntregableId : entregaId;

                var entregable = await Db.tbEntregables.FindAsync(idBuscado);
                if (entregable != null)
                {
                    entregable.Calificacion = nuevaCalificacion;
                    entregable.FechaCalificado = fechaNuevaCalificacion;
                    // Si se envió comentario (legacy: CalificacionDto), asignarlo
                    try
                    {
                        // intentar obtener comentario desde asignarCalificacion (si existe)
                        var comentarioProp = asignarCalificacion.GetType().GetProperty("Comentario");
                        if (comentarioProp != null)
                        {
                            var comentarioVal = comentarioProp.GetValue(asignarCalificacion) as string;
                            if (!string.IsNullOrEmpty(comentarioVal)) entregable.Comentario = comentarioVal;
                        }
                    }
                    catch { }

                    await Db.SaveChangesAsync();

                    // Enviar notificación al alumno
                    try
                    {
                        // obtener alumno propietario de la entrega
                        var entregaActividad = await Db.tbEntregaActividadAlumno.Where(ea => ea.tbEntregables.Any(t => t.EntregableId == entregable.EntregableId)).FirstOrDefaultAsync();
                        if (entregaActividad != null)
                        {
                            var alumnoUserId = await Db.tbAlumnos.Where(a => a.AlumnoId == entregaActividad.AlumnoId).Select(a => a.UserId).FirstOrDefaultAsync();
                            if (!string.IsNullOrEmpty(alumnoUserId))
                            {
                                var ns = new Services.NotificacionesService(Db, new Services.FCMService());
                                string titulo = "Tu actividad fue calificada";
                                string cuerpo = $"Tu entrega fue calificada con: {nuevaCalificacion}" + (entregable.Comentario != null ? ". Comentario: " + entregable.Comentario : "");
                                var tokens = await Db.tbUsuariosFcmTokens.Where(t => t.UserId == alumnoUserId).Select(t => new Models.UsuarioFcmToken { UserId = t.UserId, FcmToken = t.Token }).ToListAsync();
                                //await ns.ProcesarNotificacion(new List<string> { alumnoUserId }, tokens, titulo, cuerpo, "Calificacion");
                            }
                        }
                    }
                    catch (Exception) { }

                    return Ok();
                }

                // Fallback: si no existe en tbEntregables, intentar en tbCalificaciones (legacy)
                var calificacion = await Db.tbCalificaciones.FirstOrDefaultAsync(a => a.EntregaId == entregaId);
                if (calificacion == null)
                {
                    tbCalificaciones cal = new tbCalificaciones()
                    {
                        Calificacion = nuevaCalificacion,
                        EntregaId = entregaId,
                        FechaCalificacionAsignada = fechaNuevaCalificacion
                    };
                    Db.tbCalificaciones.Add(cal);
                }
                else
                {
                    calificacion.Calificacion = nuevaCalificacion;
                    calificacion.FechaCalificacionAsignada = fechaNuevaCalificacion;
                }
                await Db.SaveChangesAsync();
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
