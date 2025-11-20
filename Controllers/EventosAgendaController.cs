using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ControlActividades.Controllers
{
    public class EventosAgendaController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        public EventosAgendaController()
        {
        }

        public EventosAgendaController(ApplicationUserManager userManager, 
            ApplicationSignInManager signInManager, 
            RoleManager<IdentityRole> roleManager, 
            ApplicationDbContext DbContext, 
            FuncionalidadesGenerales fg)
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

        public ActionResult Calendario()
        {
            string userId = User.Identity.GetUserId();
            var docenteId = Db.tbDocentes
                .Where(d => d.UserId == userId)
                .Select(d => d.DocenteId)
                .FirstOrDefault();

            ViewBag.DocenteId = docenteId;
            return View();
        }

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

            return Json(new { mensaje = "Evento guardado exitosamente" },
                        JsonRequestBehavior.AllowGet);
        }

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

        [Authorize]
        [HttpPut]
        public async Task<ActionResult> EditarEvento(EventoEditarDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    Response.StatusCode = 400; // Bad Request
                    return Json(new { mensaje = "Datos inválidos" });
                }

                var eventoEditar = await Db.tbEventosAgenda.FindAsync(model.EventoId);
                if (eventoEditar == null) {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "Evento no encontrado" });
                }

                if (model.FechaFinal < model.FechaInicio)
                {
                    Response.StatusCode = 400;
                    return Json(new { mensaje = "La fecha final no puede ser anterior a la fecha de inicio" });
                }

                eventoEditar.Titulo = model.Titulo;
                eventoEditar.Descripcion = model.Descripcion;
                eventoEditar.FechaInicio = model.FechaInicio;
                eventoEditar.FechaFinal = model.FechaFinal;
                eventoEditar.Color = model.Color;
                
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Evento actualizado correctamente" });

            }
            catch(Exception ex) 
            {
                Response.StatusCode = 500; //Internal Server Error
                return Json(new { mensaje = "Error al actualizar el evento ", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpDelete]
        public async Task<ActionResult> EliminarEvento(int id)
        {
            try
            {
                var eventoEliminar = await Db.tbEventosAgenda.FindAsync(id);
                if (eventoEliminar == null)
                {
                    Response.StatusCode = 400; // Bad Request
                    return Json(new { mensaje = "Evento no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                Db.tbEventosAgenda.Remove(eventoEliminar);
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Evento eliminado" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; //Internal Server Error
                return Json(new { mensaje = "Error al eliminar el evento ", error = ex.Message }, JsonRequestBehavior.AllowGet);
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
