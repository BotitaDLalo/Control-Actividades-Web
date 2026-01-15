using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Controllers
{
    public class EventosAgendaController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private NotificacionesService _notifServ;
        public EventosAgendaController()
        {
        }

        public EventosAgendaController(ApplicationUserManager userManager, 
            ApplicationSignInManager signInManager, 
            RoleManager<IdentityRole> roleManager, 
            ApplicationDbContext DbContext, 
            FuncionalidadesGenerales fg,
            NotificacionesService notificacionesService)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
            Ns = notificacionesService;
        }


        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
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
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
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
                return _roleManager ?? HttpContext.GetOwinContext().Get<RoleManager<IdentityRole>>();
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
                return _notifServ ?? (_notifServ = new NotificacionesService(_db));
            }
            private set
            {
                _notifServ = value;
            }
        }
        public ActionResult IrACalendario()
        {
            if (User.IsInRole("Docente"))
                return RedirectToAction("CalendarioDocentes");

            if(User.IsInRole("Alumno"))
                return RedirectToAction("CalendarioAlumnos");

            return RedirectToAction("Index", "Home");
        }

        [Authorize(Roles = "Docente")]
        public ActionResult CalendarioDocentes()
        {
            string userId = User.Identity.GetUserId();
            var docenteId = Db.tbDocentes
                .Where(d => d.UserId == userId)
                .Select(d => d.DocenteId)
                .FirstOrDefault();

            ViewBag.DocenteId = docenteId;
            return View();
        }

        [Authorize (Roles = "Alumno")]
        public ActionResult CalendarioAlumnos()
        {
            string userId = User.Identity.GetUserId();
            var alumnoId = Db.tbAlumnos
                .Where(a => a.UserId == userId)
                .Select(a => a.AlumnoId)
                .FirstOrDefault();
            ViewBag.AlumnoId = alumnoId;
            return View();
        }

        //  SECCIÓN ALUMNOS
        // Obtener eventos por fecha seleccionada en el calendario
        public ActionResult ObtenerEventosAlumnoFecha(int alumnoId, string fecha)
        {
            try
            {
                if (!DateTime.TryParse(fecha, out DateTime fechaSeleccionada))
                {
                    return new HttpStatusCodeResult(400, "Fecha inválida.");
                }

                var fechaSoloDia = fechaSeleccionada.Date;

                var gruposAlumno = Db.tbAlumnosGrupos
                    .Where(a => a.AlumnoId == alumnoId)
                    .Select(a => a.GrupoId)
                    .ToList();

                var materiasAlumno = Db.tbAlumnosMaterias
                    .Where(a => a.AlumnoId == alumnoId)
                    .Select(a => a.MateriaId)
                    .ToList();

                var eventosPorGrupos = from eg in Db.tbEventosGrupos
                                       join ev in Db.tbEventosAgenda on eg.FechaId equals ev.EventoId
                                       join d in Db.tbDocentes on ev.DocenteId equals d.DocenteId
                                       where gruposAlumno.Contains(eg.GrupoId)
                                             && DbFunctions.TruncateTime(ev.FechaInicio) <= fechaSoloDia
                                             && DbFunctions.TruncateTime(ev.FechaFinal) >= fechaSoloDia
                                       select new
                                       {
                                           ev.EventoId,
                                           ev.Titulo,
                                           ev.Descripcion,
                                           ev.FechaInicio,
                                           ev.FechaFinal,
                                           ev.Color,
                                           Docente = d.Nombre + " " + d.ApellidoPaterno + " " + d.ApellidoMaterno
                                       };

                var eventosPorMaterias = from em in Db.tbEventosMaterias
                                         join ev in Db.tbEventosAgenda on em.FechaId equals ev.EventoId
                                         join d in Db.tbDocentes on ev.DocenteId equals d.DocenteId
                                         where materiasAlumno.Contains(em.MateriaId)
                                               && DbFunctions.TruncateTime(ev.FechaInicio) <= fechaSoloDia
                                                && DbFunctions.TruncateTime(ev.FechaFinal) >= fechaSoloDia
                                         select new
                                         {
                                             ev.EventoId,
                                             ev.Titulo,
                                             ev.Descripcion,
                                             ev.FechaInicio,
                                             ev.FechaFinal,
                                             ev.Color,
                                             Docente = d.Nombre + " " + d.ApellidoPaterno + " " + d.ApellidoMaterno
                                         };

                var todos = eventosPorGrupos
                    .Union(eventosPorMaterias)
                    .Distinct()
                    .ToList();

                return Json(new { ok = true, eventos = todos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Obtener evento individual para mostrar sus detalles
        [HttpGet]
        public ActionResult ObtenerEventoAlumnoId(int eventoId, int alumnoId)
        {
            try
            {
                var evento = Db.tbEventosAgenda.FirstOrDefault(e => e.EventoId == eventoId);
                if (evento == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { ok = false, mensaje = "Evento no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                // Obtener docente
                var docente = Db.tbDocentes
                    .Where(d => d.DocenteId == evento.DocenteId)
                    .Select(d => new
                    {
                        NombreCompleto = d.Nombre + " " + d.ApellidoPaterno + " " + d.ApellidoMaterno
                    })
                    .FirstOrDefault();

                var materiasAlumno = Db.tbAlumnosMaterias
                    .Where(a => a.AlumnoId == alumnoId)
                    .Select(a => a.MateriaId)
                    .ToList();

                var gruposAlumno = Db.tbAlumnosGrupos
                    .Where(a => a.AlumnoId == alumnoId)
                    .Select(a => a.GrupoId)
                    .ToList();


                var materiasEvento = Db.tbEventosMaterias
                    .Where(em => em.FechaId == eventoId)
                    .Select(em => em.MateriaId)
                    .ToList();

                var gruposEvento = Db.tbEventosGrupos
                    .Where(eg => eg.FechaId == eventoId)
                    .Select(eg => eg.GrupoId)
                    .ToList();

  

                bool esEventoPorGrupo = gruposEvento.Any();
                List<int> materiasMostrar = new List<int>();
                string nombreGrupoMostrar = null;

                if (esEventoPorGrupo)
                {
                    // Toma del grupo principal
                    int grupoPrincipal = gruposEvento.First();

                    var materiasDelGrupo = Db.tbGruposMaterias
                        .Where(gm => gm.GrupoId == grupoPrincipal)
                        .Select(gm => gm.MateriaId)
                        .ToList();

                    materiasMostrar = materiasDelGrupo
                        .Intersect(materiasAlumno)
                        .ToList();

                    // Mostar gruppo solo si el alumno pertenece a él
                    if (gruposAlumno.Contains(grupoPrincipal))
                    {
                        nombreGrupoMostrar = Db.tbGrupos
                            .Where(g => g.GrupoId == grupoPrincipal)
                            .Select(g => g.NombreGrupo)
                            .FirstOrDefault();
                    }
                }
                else
                {
                    // Evento por materias sueltas
                    materiasMostrar = materiasEvento
                        .Intersect(materiasAlumno)
                        .ToList();
                }

                // Cargar nombres de materias a mostrar
                var materiasFinal = Db.tbMaterias
                    .Where(m => materiasMostrar.Contains(m.MateriaId))
                    .Select(m => new
                    {
                        m.MateriaId,
                        m.NombreMateria
                    })
                    .ToList();

                return Json(new
                {
                    ok = true,
                    evento = new
                    {
                        evento.EventoId,
                        evento.Titulo,
                        evento.Descripcion,
                        FechaInicio = evento.FechaInicio.ToString("o"),
                        FechaFinal = evento.FechaFinal.ToString("o"),
                        evento.Color,
                        Docente = docente?.NombreCompleto ?? "Docente desconocido"
                    },
                    esPorGrupo = esEventoPorGrupo,
                    grupo = nombreGrupoMostrar,
                    materias = materiasFinal
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        //  SECCIÓN DOCENTES
        [Authorize]
        [HttpGet]
        public ActionResult ObtenerGruposYMaterias()
        {
            try
            {
                string userId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Json(new { grupos = new object[0], materiasSueltas = new object[0] }, JsonRequestBehavior.AllowGet);

                var docenteId = Db.tbDocentes
                                  .Where(d => d.UserId == userId)
                                  .Select(d => d.DocenteId)
                                  .FirstOrDefault();

                if (docenteId == 0)
                    return Json(new { grupos = new object[0], materiasSueltas = new object[0] }, JsonRequestBehavior.AllowGet);

                // Grupos con materias
                var grupos = Db.tbGrupos
                    .Where(g => g.DocenteId == docenteId)
                    .Select(g => new
                    {
                        GrupoId = g.GrupoId,
                        NombreGrupo = g.NombreGrupo,
                        Materias = Db.tbGruposMaterias
                                     .Where(gm => gm.GrupoId == g.GrupoId)
                                     .Join(
                                        Db.tbMaterias,
                                        gm => gm.MateriaId,
                                        m => m.MateriaId,
                                        (gm, m) => new {
                                            MateriaId = m.MateriaId,
                                            NombreMateria = m.NombreMateria
                                        }
                                     )
                                     .ToList()
                    })
                    .ToList();

                // Materias sin grupo
                var materiasEnGrupos = Db.tbGruposMaterias
                    .Select(gm => gm.MateriaId)
                    .Distinct();

                var materiasSueltas = Db.tbMaterias
                    .Where(m => m.DocenteId == docenteId && !materiasEnGrupos.Contains(m.MateriaId))
                    .Select(m => new { MateriaId = m.MateriaId, NombreMateria = m.NombreMateria })
                    .ToList();

                return Json(new { grupos = grupos, materiasSueltas = materiasSueltas }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { error = "Error al obtener datos", detail = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CrearEvento(tbEventosAgenda evento)
        {
            if (evento == null)
                return new HttpStatusCodeResult(400, "Datos inválidos.");

            string userId = User.Identity.GetUserId();

            var docenteId = Db.tbDocentes
                .Where(d => d.UserId == userId)
                .Select(d => d.DocenteId)
                .FirstOrDefault();

            if (docenteId == 0)
                return new HttpStatusCodeResult(400, "No se pudo identificar al docente.");

            evento.DocenteId = docenteId;

            if (evento.FechaFinal < evento.FechaInicio)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = "La fecha final no puede ser anterior a la fecha de inicio" });
            }

            if (evento.Color != "azul" && evento.Color != "gris")
            {
                return new HttpStatusCodeResult(400, "Solo se permiten los colores azul y gris.");
            }

            // Crear evento principal (obligatorio) Evento personal del docente
            Db.tbEventosAgenda.Add(evento);
            await Db.SaveChangesAsync();

            int eventoId = evento.EventoId;

            string gruposString = Request.Form["GruposSeleccionados"];
            string materiasString = Request.Form["MateriasSeleccionadas"];

            List<int> gruposIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(gruposString))
            {
                gruposIds = gruposString
                    .Split(',')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(int.Parse)
                    .ToList();
            }

            List<int> materiasIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(materiasString))
            {
                materiasIds = materiasString
                    .Split(',')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(int.Parse)
                    .ToList();
            }

            //Agregar materias de los grupos seleccionados
            if (gruposIds.Count > 0)
            {
                var materiasDeGrupos = Db.tbGruposMaterias
                    .Where(gm => gruposIds.Contains(gm.GrupoId))
                    .Select(gm => gm.MateriaId)
                    .ToList();

                materiasIds.AddRange(materiasDeGrupos);
            }

            // Quitar duplicados
            materiasIds = materiasIds.Distinct().ToList();

            // guardado de grupos
            foreach (var grupoId in gruposIds)
            {
                Db.tbEventosGrupos.Add(new tbEventosGrupos
                {
                    FechaId = eventoId,
                    GrupoId = grupoId
                });
            }

            // guardado de materias
            foreach (var materiaId in materiasIds)
            {
                Db.tbEventosMaterias.Add(new tbEventosMaterias
                {
                    FechaId = eventoId,
                    MateriaId = materiaId
                });
            }

            await Db.SaveChangesAsync();

            //ENVÍO DE NOTIFICACIONES

            // Notificar a todos los grupos
            foreach (var grupoId in gruposIds)
            {
                await Ns.NotificacionCrearEvento(evento, grupoId, null);
            }

            // Notificar a todas las materias
            foreach (var materiaId in materiasIds)
            {
                await Ns.NotificacionCrearEvento(evento, null, materiaId);
            }

            return Json(new { mensaje = "Evento guardado exitosamente" },
                        JsonRequestBehavior.AllowGet);
        }

        //tbEventosAgenda por docente
        [Authorize]
        [HttpGet]
        public ActionResult ObtenerEventosDocente()
        {
            string userId = User.Identity.GetUserId();
            var docenteId = Db.tbDocentes.Where(d => d.UserId == userId).Select(d => d.DocenteId).FirstOrDefault();

            if (docenteId == 0) return Json(new object[0], JsonRequestBehavior.AllowGet);

            var eventos = Db.tbEventosAgenda
                .Where(e => e.DocenteId == docenteId)
                .Select(e => new
                {
                    eventoId = e.EventoId,
                    titulo = e.Titulo,
                    descripcion = e.Descripcion,
                    fechaInicio = e.FechaInicio,
                    fechaFinal = e.FechaFinal,
                    color = e.Color
                })
                .ToList();

            return Json(eventos, JsonRequestBehavior.AllowGet);
        }

        //Eventos con tablas relacionadas tbMaterias y tbGrupos
        [Authorize]
        [HttpGet]
        public ActionResult ObtenerEventoPorId(int id)
        {
            // Buscar evento principal
            var eventoEntity = Db.tbEventosAgenda
                .FirstOrDefault(e => e.EventoId == id);

            if (eventoEntity == null)
            {
                Response.StatusCode = 404;
                return Json(new { mensaje = "Evento no encontrado" }, JsonRequestBehavior.AllowGet);
            }

            var evento = new
            {
                eventoId = eventoEntity.EventoId,
                titulo = eventoEntity.Titulo,
                descripcion = eventoEntity.Descripcion,
                fechaInicio = eventoEntity.FechaInicio.ToString("o"), // ISO 8601
                fechaFinal = eventoEntity.FechaFinal.ToString("o"),
                color = eventoEntity.Color
            };

            // Materias asociadas al evento (ids)
            var materiasEventoIds = Db.tbEventosMaterias
                .Where(em => em.FechaId == id)
                .Select(em => em.MateriaId)
                .ToList();

            // Grupos asociados al evento (ids)
            var gruposEventoIds = Db.tbEventosGrupos
                .Where(eg => eg.FechaId == id)
                .Select(eg => eg.GrupoId)
                .ToList();

            // Grupos que contienen alguna de las materias del evento
            var gruposPorMateriasIds = Db.tbGruposMaterias
                .Where(gm => materiasEventoIds.Contains(gm.MateriaId))
                .Select(gm => gm.GrupoId)
                .ToList();
            
            // Unión de ambos sin repetir
            var gruposMostrarIds = gruposEventoIds
                .Union(gruposPorMateriasIds)
                .Distinct()
                .ToList();

            // Cargar grupos con todas sus materias
            var gruposConMaterias = Db.tbGrupos
                .Where(g => gruposMostrarIds.Contains(g.GrupoId))
                .Select(g => new
                {
                    grupoId = g.GrupoId,
                    nombre = g.NombreGrupo,
                    materias = Db.tbGruposMaterias
                        .Where(gm => gm.GrupoId == g.GrupoId)
                        .Join(Db.tbMaterias,
                              gm => gm.MateriaId,
                              m => m.MateriaId,
                              (gm, m) => new
                              {
                                  materiaId = m.MateriaId,
                                  nombre = m.NombreMateria,
                                  isSelected = materiasEventoIds.Contains(m.MateriaId)
                              })
                        .ToList()
                })
                .ToList();

            // Materias sin grupos asociadas al evento:
            // materias que están en tbEventosMaterias pero no en tbGruposMaterias (no pertenecen a ningún grupo)
            var materiasSueltas = Db.tbMaterias
                .Where(m => materiasEventoIds.Contains(m.MateriaId)
                            && !Db.tbGruposMaterias.Any(gm => gm.MateriaId == m.MateriaId))
                .Select(m => new { materiaId = m.MateriaId, nombre = m.NombreMateria })
                .ToList();

            bool esPersonal = !gruposConMaterias.Any() && !materiasSueltas.Any();

            return Json(new
            {
                evento = evento,
                esPersonal = esPersonal,
                gruposConMaterias = gruposConMaterias,
                materiasSueltas = materiasSueltas
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpGet]
        public ActionResult ObtenerEventosPorFecha(string fecha)
        {
            if (!DateTime.TryParse(fecha, out DateTime fechaSeleccionada))
            {
                return new HttpStatusCodeResult(400, "Fecha inválida.");
            }

            var eventos = Db.tbEventosAgenda
                .Where(e => DbFunctions.TruncateTime(e.FechaInicio) == fechaSeleccionada.Date)
                .Select(e => new
                {
                    eventoId = e.EventoId,
                    titulo = e.Titulo,
                    descripcion = e.Descripcion,
                    fechaInicio = e.FechaInicio,
                    fechaFinal = e.FechaFinal,
                    color = e.Color
                })
                .ToList();

            if (!eventos.Any())
            {
                return Json(new { mensaje = "No hay eventos para esta fecha." }, JsonRequestBehavior.AllowGet);
            }

            return Json(eventos, JsonRequestBehavior.AllowGet);
        }


        //Obtener Evento para editar
        [Authorize]
        [HttpGet]
        public async Task<ActionResult> GetEvento(int id)
        {
            var evento = await Db.tbEventosAgenda
                .Include(e => e.EventosGrupos)
                .Include(e => e.EventosMaterias)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null)
                return HttpNotFound("Evento no encontrado");

            return Json(new
            {
                eventoId = evento.EventoId,
                titulo = evento.Titulo,
                descripcion = evento.Descripcion,
                fechaInicio = evento.FechaInicio,
                fechaFinal = evento.FechaFinal,
                color = evento.Color,

                gruposSeleccionados = evento.EventosGrupos.Select(g => g.GrupoId).ToList(),
                materiasSeleccionadas = evento.EventosMaterias.Select(m => m.MateriaId).ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditarEvento(EventoEditarDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = 400;
                    return Json(new { mensaje = "Datos inválidos" });
                }

                var evento = Db.tbEventosAgenda.Find(model.EventoId);

                if (evento == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "Evento no encontrado" });
                }

                
                // Actualizar datos del evento
                evento.Titulo = model.Titulo;
                evento.Descripcion = model.Descripcion;
                evento.Color = model.Color;
                evento.FechaInicio = model.FechaInicio;
                evento.FechaFinal = model.FechaFinal;

                
                // Actualizar grupos asignados
                var gruposActuales = Db.tbEventosGrupos
                    .Where(x => x.FechaId == model.EventoId)
                    .ToList();

                foreach (var item in gruposActuales)
                    Db.tbEventosGrupos.Remove(item);

                if (model.GruposSeleccionados != null)
                {
                    foreach (var idGrupo in model.GruposSeleccionados)
                    {
                        Db.tbEventosGrupos.Add(new tbEventosGrupos
                        {
                            FechaId = model.EventoId,        
                            GrupoId = idGrupo
                        });
                    }
                }

                // Actualizar materias asignadas
                var materiasActuales = Db.tbEventosMaterias
                    .Where(x => x.FechaId == model.EventoId)  
                    .ToList();

                foreach (var item in materiasActuales)
                    Db.tbEventosMaterias.Remove(item);

                if (model.MateriasSeleccionadas != null)
                {
                    foreach (var idMat in model.MateriasSeleccionadas)
                    {
                        Db.tbEventosMaterias.Add(new tbEventosMaterias
                        {
                            FechaId = model.EventoId,         
                            MateriaId = idMat
                        });
                    }
                }

                Db.SaveChanges();

                return Json(new { mensaje = "Evento actualizado correctamente" });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al editar evento", error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> EliminarEvento(int id)
        {
            try
            {
                var evento = await Db.tbEventosAgenda.FindAsync(id);
                if (evento == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "Evento no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                // Eliminar grupos relacionados
                var grupos = Db.tbEventosGrupos.Where(g => g.FechaId == id).ToList();
                foreach (var g in grupos)
                    Db.tbEventosGrupos.Remove(g);

                // Eliminar materias relacionadas
                var materias = Db.tbEventosMaterias.Where(m => m.FechaId == id).ToList();
                foreach (var m in materias)
                    Db.tbEventosMaterias.Remove(m);

                // Eliminar evento principal
                Db.tbEventosAgenda.Remove(evento);

                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Evento eliminado correctamente" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al eliminar el evento", error = ex.Message }, JsonRequestBehavior.AllowGet);
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
