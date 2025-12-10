using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/EventosAgenda")]
    public class EventosAgendaApiController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        public EventosAgendaApiController()
        {
        }

        public EventosAgendaApiController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
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


        public async Task<List<object>> ConsultaEventos()
        {
            try
            {
                // Consultamos los eventos de la agenda
                var eventos = await Db.tbEventosAgenda
                    .Include(e => e.EventosGrupos)  // Incluimos los eventos de grupos
                    .Include(e => e.EventosMaterias) // Incluimos los eventos de materias
                    .ToListAsync();

                // Formateamos los eventos para devolver solo la información necesaria
                var listaEventos = eventos.Select(e => new
                {
                    e.EventoId,
                    e.DocenteId,
                    e.Titulo,
                    e.Descripcion,
                    e.Color,
                    FechaInicio = e.FechaInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                    FechaFinal = e.FechaFinal.ToString("yyyy-MM-ddTHH:mm:ss"),
                    e.EventosGrupos?.FirstOrDefault()?.GrupoId,
                    e.EventosMaterias?.FirstOrDefault()?.MateriaId,
                }).ToList();

                return listaEventos.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                // En caso de error, lanzamos la excepción para ser manejada en el controlador
                throw new Exception("Hubo un problema al consultar los eventos", ex);
            }
        }



        [HttpGet]
        [Route("ObtenerEventos")]
        public async Task<IHttpActionResult> ObtenerEventos(int docenteId)
        {
            try
            {
                // Consulta eventos y convierte la lista a dynamic
                var lsEventos = await ConsultaEventos();
                var eventosDinamicos = lsEventos.Cast<dynamic>();

                // Filtrar los eventos por docenteId
                var eventosFiltrados = eventosDinamicos
                    .Where(e => e.DocenteId == docenteId)
                    .ToList();

                // Devolver los eventos filtrados
                return Ok(eventosFiltrados);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest,new { e.Message });
            }
        }

        // Nuevo metodo para consultar los evetos para el Alumno
        [HttpGet]
        [Route("ObtenerEventosAlumno")]
        public async Task<IHttpActionResult> ObtenerEventosAlumno(int alumnoId)
        {
            try
            {
                // Obtener todos los eventos
                var lsEventos = await ConsultaEventos();
                var eventosDinamicos = lsEventos.Cast<dynamic>();

                // Obtener los grupos del alumno
                var gruposAlumno = await Db.tbAlumnosGrupos
                    .Where(ag => ag.AlumnoId == alumnoId)
                    .Select(ag => ag.GrupoId)
                    .ToListAsync();

                // Obtener las materias relacionadas con esos grupos (si es necesario)
                var materiasAlumno = await Db.tbGruposMaterias
                    .Where(gm => gruposAlumno.Contains(gm.GrupoId))
                    .Select(gm => gm.MateriaId)
                    .ToListAsync();

                // Filtrar eventos que pertenezcan a los grupos o materias del alumno
                var eventosFiltrados = eventosDinamicos
                    .Where(e =>
                        (e.GrupoId != null && gruposAlumno.Contains((int)e.GrupoId)) ||
                        (e.MateriaId != null && materiasAlumno.Contains((int)e.MateriaId))
                    )
                    .ToList();

                return Ok(eventosFiltrados);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { e.Message });
            }
        }



        [HttpPost]
        [Route("CrearEventos")]
        public async Task<IHttpActionResult> CrearEventos([FromBody] tbEventosAgenda nuevoEvento)
        {

            try
            {
                // Primero, guardar el evento en la tabla EventosAgenda
                Db.tbEventosAgenda.Add(nuevoEvento);
                await Db.SaveChangesAsync();  // Esto genera automáticamente el EventoId (FechaId)

                // Después de guardar, asignamos el EventoId generado a EventosGrupos y EventosMaterias
                if (nuevoEvento.EventosGrupos != null)
                {
                    var grupos = nuevoEvento.EventosGrupos.Select(grupo => new tbEventosGrupos
                    {
                        FechaId = nuevoEvento.EventoId,
                        GrupoId = grupo.GrupoId
                    }).ToList();

                    Db.tbEventosGrupos.AddRange(grupos);

                }

                if (nuevoEvento.EventosMaterias != null)
                {
                    var materias = nuevoEvento.EventosMaterias.Select(materia => new tbEventosMaterias
                    {
                        FechaId = nuevoEvento.EventoId,
                        MateriaId = materia.MateriaId,
                    }).ToList();

                    Db.tbEventosMaterias.AddRange(materias);
                }

                await Db.SaveChangesAsync(); // Guardamos las relaciones en EventosGrupos y EventosMaterias

                // Retornamos el evento creado, o el ID generado para confirmación
                return Ok(new { Message = "Evento creado correctamente." });
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.InternalServerError, "Ocurrió un error al procesar la solicitud.");
            }

        }


        [HttpPatch]
        [Route("ActualizarEvento/{id}")]
        public async Task<IHttpActionResult> ActualizarEvento(int id, [FromBody] tbEventosAgenda eventoActualizado)
        {
            try
            {
                var eventoExistente = await Db.tbEventosAgenda
                    .Include(e => e.EventosGrupos)
                    .Include(e => e.EventosMaterias)
                    .FirstOrDefaultAsync(e => e.EventoId == id);

                if (eventoExistente == null)
                {
                    return Content(HttpStatusCode.NotFound,new { Message = "Evento no encontrado." });
                }

                // Actualizar los campos básicos del evento
                eventoExistente.Titulo = eventoActualizado.Titulo;
                eventoExistente.Descripcion = eventoActualizado.Descripcion;
                eventoExistente.Color = eventoActualizado.Color;
                eventoExistente.FechaInicio = eventoActualizado.FechaInicio;
                eventoExistente.FechaFinal = eventoActualizado.FechaFinal;

                // Eliminar relaciones anteriores solo si se enviaron nuevas
                if ((eventoActualizado.EventosGrupos != null && eventoActualizado.EventosGrupos.Any()) ||
                    (eventoActualizado.EventosMaterias != null && eventoActualizado.EventosMaterias.Any()))
                {
                    Db.tbEventosGrupos.RemoveRange(eventoExistente.EventosGrupos);
                    Db.tbEventosMaterias.RemoveRange(eventoExistente.EventosMaterias);
                }

                // Verificar que solo uno de los dos (Grupos o Materias) sea asignado
                if ((eventoActualizado.EventosGrupos != null && eventoActualizado.EventosGrupos.Any()) &&
                    (eventoActualizado.EventosMaterias != null && eventoActualizado.EventosMaterias.Any()))
                {
                    return Content(HttpStatusCode.BadRequest,new { Message = "El evento no puede estar asignado a un grupo y una materia al mismo tiempo." });
                }

                // Validar que los grupos existan antes de insertarlos
                if (eventoActualizado.EventosGrupos != null && eventoActualizado.EventosGrupos.Any())
                {
                    var grupoIds = eventoActualizado.EventosGrupos.Select(g => g.GrupoId).ToList();
                    var gruposExistentes = await Db.tbGrupos
                        .Where(g => grupoIds.Contains(g.GrupoId))
                        .Select(g => g.GrupoId)
                        .ToListAsync();

                    if (gruposExistentes.Count != grupoIds.Count)
                    {
                        return Content(HttpStatusCode.BadRequest, new { Message = "Uno o más GrupoId no existen en la base de datos." });
                    }

                    eventoExistente.EventosGrupos = eventoActualizado.EventosGrupos.Select(grupo => new tbEventosGrupos
                    {
                        FechaId = eventoExistente.EventoId,
                        GrupoId = grupo.GrupoId
                    }).ToList();
                }

                // Validar que las materias existan antes de insertarlas
                if (eventoActualizado.EventosMaterias != null && eventoActualizado.EventosMaterias.Any())
                {
                    var materiaIds = eventoActualizado.EventosMaterias.Select(m => m.MateriaId).ToList();
                    var materiasExistentes = await Db.tbMaterias
                        .Where(m => materiaIds.Contains(m.MateriaId))
                        .Select(m => m.MateriaId)
                        .ToListAsync();

                    if (materiasExistentes.Count != materiaIds.Count)
                    {
                        return Content(HttpStatusCode.BadRequest, new { Message = "Uno o más MateriaId no existen en la base de datos." });
                    }

                    eventoExistente.EventosMaterias = eventoActualizado.EventosMaterias.Select(materia => new tbEventosMaterias
                    {
                        FechaId = eventoExistente.EventoId,
                        MateriaId = materia.MateriaId
                    }).ToList();
                }

                // Si no hay grupos ni materias, lanzar un error
                if ((eventoActualizado.EventosGrupos == null || !eventoActualizado.EventosGrupos.Any()) &&
                    (eventoActualizado.EventosMaterias == null || !eventoActualizado.EventosMaterias.Any()))
                {
                    return Content(HttpStatusCode.BadRequest, new { Message = "El evento debe estar asignado a al menos un grupo o materia." });
                }

                await Db.SaveChangesAsync();

                return Ok(new { Message = "Evento actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { Message = "Ocurrió un error al actualizar el evento.", Error = ex.Message, StackTrace = ex.StackTrace });
            }
        }


        [HttpDelete]
        [Route("EliminarEvento")]
        public async Task<IHttpActionResult> EliminarEvento(int eventoId, int docenteId)
        {
            try
            {
                // Buscar el evento en la tabla EventosAgenda
                var evento = await Db.tbEventosAgenda
                    .Include(e => e.EventosGrupos)
                    .Include(e => e.EventosMaterias)
                    .FirstOrDefaultAsync(e => e.EventoId == eventoId);

                if (evento == null)
                {
                    return Content(HttpStatusCode.InternalServerError,new { Message = "El evento no fue encontrado." });
                }

                // Eliminar las relaciones en EventosGrupos si existen
                if (evento.EventosGrupos != null && evento.EventosGrupos.Any())
                {
                    Db.tbEventosGrupos.RemoveRange(evento.EventosGrupos);
                }

                // Eliminar las relaciones en EventosMaterias si existen
                if (evento.EventosMaterias != null && evento.EventosMaterias.Any())
                {
                    Db.tbEventosMaterias.RemoveRange(evento.EventosMaterias);
                }

                // Eliminar el evento de la tabla EventosAgenda
                Db.tbEventosAgenda.Remove(evento);

                // Guardar los cambios en la base de datos
                await Db.SaveChangesAsync();

                return Ok(new { Message = "Evento eliminado correctamente." });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { Message = "Ocurrió un error al eliminar el evento.", Error = ex.Message });
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
