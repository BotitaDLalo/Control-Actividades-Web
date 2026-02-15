using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ControlActividades.Exceptions;
using ControlActividades.Interfaces.Alumnos;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using ControlActividades.Services.Alumno;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Controllers
{
    [Authorize]
    public class AlumnoController : Controller
    {
        #region inicializaciones
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private FCMService _fCMService;
        private AlumnoApiService _alumnoApiService;
        private AlumnoService _alumnoService;


        public AlumnoController()
        {
        }

        public AlumnoController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext DbContext,
            FuncionalidadesGenerales fg,
            FCMService fCMService
            )
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
            _fCMService = fCMService;
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
        public AlumnoApiService AlumnoApiService
        {
            get
            {
                return _alumnoApiService ?? (_alumnoApiService = new AlumnoApiService());
            }
            private set
            {
                _alumnoApiService = value;
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
        
        private AlumnoService AlumnoService
        {
            get
            {
                return _alumnoService ?? (_alumnoService = new AlumnoService());
            }
            set
            {
                _alumnoService = value;
            }
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

        #endregion

        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();

            var alumnoId = Db.tbAlumnos.Where(a => a.UserId == userId).Select(a => a.AlumnoId).FirstOrDefault();

            ViewBag.AlumnoId = alumnoId;

            return View();
        }

        #region grupos y materias

        [HttpGet]
        public async Task<ActionResult> ObtenerClases(int alumnoId)
        {
            var grupos = await Db.tbAlumnosGrupos
                .Where(ag => ag.AlumnoId == alumnoId)
                .Select(ag => new
                {
                    Id = ag.Grupos.GrupoId,
                    Nombre = ag.Grupos.NombreGrupo,
                    esGrupo = true,
                    Materias = Db.tbGruposMaterias
                        .Where(gm => gm.GrupoId == ag.Grupos.GrupoId)
                        .Select(gm => new
                        {
                            Id = gm.MateriaId,
                            Nombre = gm.Materias.NombreMateria
                        })
                })
                .ToListAsync();

            var gruposConMaterias = grupos.Select(g => new
            {
                g.Id,
                g.Nombre,
                g.esGrupo,
                Materias = g.Materias.ToList()
            }).ToList();

            var materias = Db.tbAlumnosMaterias
                .Where(am => am.AlumnoId == alumnoId)
                .Select(am => new
                {
                    Id = am.Materias.MateriaId,
                    Nombre = am.Materias.NombreMateria,
                    esGrupo = false
                })
                .ToList();

            var clases = gruposConMaterias.Cast<object>().Concat(materias.Cast<object>()).ToList();

            return Json(clases, JsonRequestBehavior.AllowGet);
        }


        public async Task<ActionResult> Clase(string tipo, string id)
        {

            int Id = int.Parse(id);
            if (string.IsNullOrEmpty(tipo) || string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(400, "Parámetros inválidos.");
            }

            if (tipo.ToLower() == "grupo")
            {
                var grupo = await Db.tbGrupos.FirstOrDefaultAsync(g => g.GrupoId == Id);
                if (grupo == null) return HttpNotFound("Grupo no encontrado.");
                string userId = User.Identity.GetUserId();

                var alumnoId = Db.tbAlumnos.Where(a => a.UserId == userId).Select(a => a.AlumnoId).FirstOrDefault();

                ViewBag.AlumnoId = alumnoId;


                return View("DetalleGrupo", grupo);
            }
            else if (tipo.ToLower() == "materia")
            {
                var materia = await Db.tbMaterias.FirstOrDefaultAsync(m => m.MateriaId == Id);
                if (materia == null) return HttpNotFound("Materia no encontrada.");
                string userId = User.Identity.GetUserId();

                var alumnoId = Db.tbAlumnos.Where(a => a.UserId == userId).Select(a => a.AlumnoId).FirstOrDefault();

                ViewBag.AlumnoId = alumnoId;
                return View("DetalleMateria", materia);
            }

            return new HttpStatusCodeResult(400, "Tipo de clase no válido.");
        }


        public ActionResult DetalleMateria()
        {
            // Evitar renderizar la vista sin un modelo válido.
            // Esta vista debe cargarse desde la acción Clase(tipo="materia", id=...).
            return RedirectToAction("Index");
        }

        public ActionResult DetalleGrupo()
        {
            // Evitar renderizar la vista sin un modelo válido.
            return RedirectToAction("Index");
        }

        #endregion

        [HttpPost]
        [CustomAuthorize(Roles = "Alumno")]
        public async Task<ActionResult> UnirseAClaseWeb(string CodigoAcceso)
        {
            try
            {
                int alumnoId = Fg.ObtenerCAUsuarioId(User);

                if (alumnoId <= 0)
                {
                    Response.StatusCode = 401;
                    return Json(new { mensaje = "Usuario no válido." });
                }

                if (string.IsNullOrWhiteSpace(CodigoAcceso))
                {
                    Response.StatusCode = 400;
                    return Json(new { mensaje = "Código inválido." });
                }

                var resultado = await AlumnoService.UnirseAClase(alumnoId, CodigoAcceso);
                if (resultado == null)
                {
                    Response.StatusCode = 400;
                    return Json(new { mensaje = "No se pudo procesar la solicitud." });
                }
                return Json(resultado);

            }
            catch (AlumnosException e)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = e.Mensaje});
            }
            catch (Exception)
            {
                Response.StatusCode = 500;

                return Json(new {mensaje = "Error interno del servidor."});
            }
        }





        #region avisos
        public async Task<ActionResult> Avisos(int alumnoId, int? materiaId, int? grupoId)
        {
            ViewBag.AlumnoId = alumnoId;
            IQueryable<tbAvisos> query = Db.tbAvisos;

            // If a specific materia or grupo is provided, prefer scoping to it
            if (materiaId.HasValue && materiaId.Value > 0)
            {
                query = query.Where(a => a.MateriaId == materiaId.Value);
            }
            else if (grupoId.HasValue && grupoId.Value > 0)
            {
                query = query.Where(a => a.GrupoId == grupoId.Value);
            }
            else
            {
                query = query.Where(a => Db.tbAlumnosGrupos.Any(ag => ag.AlumnoId == alumnoId && ag.GrupoId == a.GrupoId)
                             || Db.tbAlumnosMaterias.Any(am => am.AlumnoId == alumnoId && am.MateriaId == a.MateriaId));
            }

            var avisos = await query.ToListAsync();
            return PartialView("_Avisos", avisos);
        }


        [HttpGet]
        public async Task<ActionResult> ObtenerAvisos(int alumnoId, int? materiaId, int? grupoId)
        {
            try
            {
                IQueryable<tbAvisos> query = Db.tbAvisos;
                if (materiaId.HasValue && materiaId.Value > 0)
                {
                    query = query.Where(a => a.MateriaId == materiaId.Value);
                }
                else if (grupoId.HasValue && grupoId.Value > 0)
                {
                    query = query.Where(a => a.GrupoId == grupoId.Value);
                }
                else
                {
                    query = query.Where(a => Db.tbAlumnosGrupos.Any(ag => ag.AlumnoId == alumnoId && ag.GrupoId == a.GrupoId)
                                 || Db.tbAlumnosMaterias.Any(am => am.AlumnoId == alumnoId && am.MateriaId == a.MateriaId));
                }

                var avisosDb = await query.ToListAsync();

                var avisos = avisosDb.Select(a => new
                {
                    a.AvisoId,
                    a.Titulo,
                    a.Descripcion,
                    FechaCreacion = a.FechaCreacion.ToString("dddd, d 'de' MMMM 'de' yyyy HH:mm:ss")
                }).ToList();

                return Json(avisos, JsonRequestBehavior.AllowGet);

                /*
                if (!avisos.Any())
                {
                    return HttpNotFound("No hay avisos para este alumno.");
                }
                */

            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new
                {
                    mensaje = "Error al obtener avisos",
                    detalle = ex.Message,
                    stack = ex.StackTrace
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        public ActionResult Actividades()
        {
            return PartialView("_Actividades");
        }

        [HttpGet]
        public ActionResult ActividadDetalle(int actividadId, int alumnoId = 0)
        {
            try
            {
                List<string> lsEnlaces = new List<string>();
                List<string> lsEnlacesArchivo = new List<string>();
                bool estaCalificado = false;
                bool estaEntregado = false;
                string respuesta = string.Empty;
                decimal calificacion = 0;
                int entregaId = 0;
                EntregaContenidoModel entrega = new EntregaContenidoModel();


                if (User.IsInRole(Roles.ALUMNO))
                {
                    string userId = User.Identity.GetUserId();
                    alumnoId = Db.tbAlumnos.Where(a => a.UserId == userId).Select(a => a.AlumnoId).FirstOrDefault();
                }



                ViewBag.AlumnoId = alumnoId;
                ViewBag.ActividadId = actividadId;

                var datosActividad = Db.tbActividades.Where(a => a.ActividadId == actividadId).FirstOrDefault();
                
                ViewBag.Puntaje = datosActividad.Puntaje;

                var datosEntregable = Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId && a.Estatus).FirstOrDefault();

                if (datosEntregable != null)
                {
                    entregaId = datosEntregable.EntregaActividadAlumnoId;

                    var entregables = Db.tbEntregables.Where(a => a.EntregaActividadAlumnoId == datosEntregable.EntregaActividadAlumnoId).Select(a => a.Contenido).FirstOrDefault();


                    entrega = JsonConvert.DeserializeObject<EntregaContenidoModel>(entregables);


                    respuesta = entrega.Texto;

                    lsEnlaces = entrega.Enlaces;

                    lsEnlacesArchivo = entrega.Archivos.Select(a => a.Ruta).ToList();

                    var fechaEntrega = entrega.FechaEntrega;

                    lsEnlaces = lsEnlaces.Concat(lsEnlacesArchivo).ToList();

                    estaCalificado = (datosEntregable.Calificacion > 0 && datosEntregable.FechaCalificado != null);

                    calificacion = datosEntregable.Calificacion;

                    estaEntregado = true;
                }

                ViewBag.EntregaId = entregaId;

                ActividadDetalleViewModel model = new ActividadDetalleViewModel()
                {
                    NombreActividad = datosActividad.NombreActividad,
                    Descripcion = datosActividad.Descripcion,
                    Respuesta = respuesta,
                    Puntaje = datosActividad.Puntaje,
                    FechaLimite = datosActividad.FechaLimite,
                    Calificacion = calificacion,
                    Enlaces = lsEnlaces,
                    EstaCalificado = estaCalificado,
                    EstaEntregado = estaEntregado,
                };

                if (datosEntregable.FechaCalificado != null)
                {
                    model.FechaCalificado = datosEntregable.FechaCalificado.Value;
                }

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Alumnos()
        {
            return PartialView("_Alumnos");
        }

        public ActionResult Calificaciones()
        {
            return PartialView("_Calificaciones");
        }



        public ActionResult Perfil()
        {
            return View();
        }



        public ActionResult Materia()
        {
            return View();
        }

        // GET: /Alumno/Grupos
        [HttpGet]
        public ActionResult Grupos()
        {
            string userId = User.Identity.GetUserId();
            var alumnoId = Db.tbAlumnos.Where(a => a.UserId == userId).Select(a => a.AlumnoId).FirstOrDefault();
            ViewBag.AlumnoId = alumnoId;

            if (Request.IsAjaxRequest() || (Request.Headers["X-Requested-With"] == "XMLHttpRequest"))
            {
                return PartialView("_GruposPartial");
            }

            return View();
        }

        // GET: /Alumno/MateriasSinGrupo
        [HttpGet]
        public ActionResult MateriasSinGrupo()
        {
            string userId = User.Identity.GetUserId();
            var alumnoId = Db.tbAlumnos.Where(a => a.UserId == userId).Select(a => a.AlumnoId).FirstOrDefault();
            ViewBag.AlumnoId = alumnoId;

            if (Request.IsAjaxRequest() || (Request.Headers["X-Requested-With"] == "XMLHttpRequest"))
            {
                return PartialView("_MateriasSinGrupoPartial");
            }

            return View();
        }

        //public class ModeloNotif
        //{
        //    public string targetToken { get; set; }
        //    public string title { get; set; }
        //    public string body { get; set; }
        //}



        #region SubirEntrega moved from API
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SubirEntrega()
        //{
        //    try
        //    {
        //        var httpRequest = Request;
        //        if (httpRequest == null)
        //        {
        //            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //            return Json(new { mensaje = "No se recibió la solicitud." });
        //        }

        //        string respuesta = httpRequest.Form["Respuesta"] ?? string.Empty;

        //        int.TryParse(httpRequest.Form["ActividadId"], out int actividadId);
        //        int.TryParse(httpRequest.Form["AlumnoId"], out int alumnoId);

        //        if (actividadId == 0 || alumnoId == 0)
        //        {
        //            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //            return Json(new { mensaje = "Faltan datos: AlumnoId o ActividadId." });
        //        }

        //        // Guardar archivos (si los hay)
        //        var savedUrls = new List<string>();
        //        var files = httpRequest.Files;
        //        var uploadRoot = Server.MapPath("~/Uploads/Entregas/");
        //        var destFolder = Path.Combine(uploadRoot, actividadId.ToString(), alumnoId.ToString());
        //        if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

        //        for (int i = 0; i < files.Count; i++)
        //        {
        //            var file = files[i];
        //            if (file == null || file.ContentLength == 0) continue;
        //            var safeName = Path.GetFileName(file.FileName);
        //            var destPath = Path.Combine(destFolder, safeName);
        //            // evitar sobreescribir: agregar timestamp
        //            if (System.IO.File.Exists(destPath))
        //            {
        //                var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        //                destPath = Path.Combine(destFolder, ts + "_" + safeName);
        //            }
        //            file.SaveAs(destPath);
        //            var relative = "/Uploads/Entregas/" + actividadId + "/" + alumnoId + "/" + Path.GetFileName(destPath);
        //            savedUrls.Add(relative);
        //        }

        //        // Preparar contenido: si hay archivos, guardar JSON con respuesta + archivos, si no sólo texto
        //        string contenidoGuardar = respuesta ?? string.Empty;
        //        if (savedUrls.Count > 0)
        //        {
        //            contenidoGuardar = BuildRespuestaWithFiles(respuesta, savedUrls);
        //        }

        //        // Fecha de entrega (usar campo enviado o ahora)
        //        DateTime fechaEnt = DateTime.Now;
        //        DateTime parsedFecha;
        //        if (DateTime.TryParse(httpRequest.Form["FechaEntrega"], out parsedFecha)) fechaEnt = parsedFecha;

        //        // Asegurar que exista un EstadoEntrega válido (evitar violación de FK)
        //        int estadoResolved = await ResolveEstadoEntregaIdAsync(1);

        //        // Verificar permisos: el alumno debe pertenecer a la materia o a un grupo que la contiene
        //        if (!await AlumnoPuedeAccederActividadAsync(alumnoId, actividadId))
        //        {
        //            Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
        //            return Json(new { mensaje = "No tienes permiso para entregar esta actividad." });
        //        }

        //        // Bloquear entregas fuera de la fecha límite si la actividad no permite entregas tarde
        //        var actividad = await Db.tbActividades.FindAsync(actividadId);
        //        if (actividad != null && DateTime.Now > actividad.FechaLimite && !actividad.PermitirEntregasTarde)
        //        {
        //            Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
        //            return Json(new { mensaje = "La fecha límite ya pasó y no se permiten entregas tardías para esta actividad." });
        //        }

        //        int entregaAlumnoId = 0;
        //        tbEntregables entregables = null;
        //        // tbEntregableAlumno entregableLegacy = null; // legacy handling kept minimal here

        //        try
        //        {
        //            var entregaAlumnoExistente = await Db.Set<tbEntregaActividadAlumno>().FirstOrDefaultAsync(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId);
        //            if (entregaAlumnoExistente == null)
        //            {
        //                tbEntregaActividadAlumno entregaAlumno = new tbEntregaActividadAlumno()
        //                {
        //                    ActividadId = actividadId,
        //                    AlumnoId = alumnoId,
        //                    FechaEntrega = fechaEnt,
        //                    EstadoEntregaId = estadoResolved
        //                };

        //                Db.tbEntregaActividadAlumno.Add(entregaAlumno);
        //                await Db.SaveChangesAsync();
        //                entregaAlumnoId = entregaAlumno.EntregaActividadAlumnoId;
        //            }
        //            else
        //            {
        //                int entregaIdExistente = entregaAlumnoExistente.EntregaActividadAlumnoId;
        //                //var lsEntregablesExistentes = Db.tbEntregables.Where(a => a.EntregaActividadAlumnoId == entregaIdExistente).ToList();
        //                //Db.tbEntregables.RemoveRange(lsEntregablesExistentes);


        //                tbEntregaActividadAlumno entregaAlumno = new tbEntregaActividadAlumno()
        //                {
        //                    ActividadId = actividadId,
        //                    AlumnoId = alumnoId,
        //                    FechaEntrega = fechaEnt,
        //                    EstadoEntregaId = 1,
        //                    Estatus = true
        //                };

        //                Db.tbEntregaActividadAlumno.Add(entregaAlumno);
        //                await Db.SaveChangesAsync();
        //                entregaAlumnoId = entregaAlumno.EntregaActividadAlumnoId;
        //            }

        //            int preferido = (savedUrls.Count > 0) ? 2 : 1;
        //            int tipoId = await ResolveTipoEntregaIdAsync(preferido);

        //            entregables = new tbEntregables()
        //            {
        //                EntregaActividadAlumnoId = entregaAlumnoId,
        //                TipoEntregaId = tipoId,
        //                Contenido = contenidoGuardar
        //            };
        //            Db.tbEntregables.Add(entregables);
        //            await Db.SaveChangesAsync();
        //        }
        //        catch (Exception ex)
        //        {
        //            // intentar fallback legacy si es necesario
        //            // Por simplicidad aquí devolvemos error con detalle
        //            Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
        //            return Json(new { mensaje = "Error al guardar entregable.", detalle = ex.Message });
        //        }

        //        // Notificar al docente que un alumno entregó (FCM + persistir notificación)
        //        //try
        //        //{
        //            //var docenteUserId = await Db.tbMaterias.Where(m => m.MateriaId == actividad.MateriaId).Select(m => m.DocenteId).FirstOrDefaultAsync();
        //            //var docenteUid = await Db.tbDocentes.Where(d => d.DocenteId == docenteUserId).Select(d => d.UserId).FirstOrDefaultAsync();
        //            //if (!string.IsNullOrEmpty(docenteUid))
        //            //{
        //            //    var ns = new NotificacionesService(Db, new FCMService());
        //            //    string titulo = "Nueva entrega recibida";
        //            //    string cuerpo = $"El alumno ha entregado la actividad {actividad.NombreActividad}.";
        //            //    var tokens = await Db.tbUsuariosFcmTokens.Where(t => t.UserId == docenteUid).Select(t => new Models.UsuarioFcmToken { UserId = t.UserId, FcmToken = t.Token }).ToListAsync();
        //            //    await ns.ProcesarNotificacion(new List<string> { docenteUid }, tokens, titulo, cuerpo, TiposNotificaciones.ActividadEntregada, actividad.MateriaId);
        //            //}
        //        //}
        //        //catch { }

        //        if (entregables != null)
        //        {
        //            return Json(new
        //            {
        //                EntregaActividadAlumnoId = entregaAlumnoId,
        //                EntregableId = entregables.EntregableId,
        //                Contenido = entregables.Contenido
        //            });
        //        }

        //        return Json(new { EntregaActividadAlumnoId = entregaAlumnoId });
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
        //        return Json(new { mensaje = ex.Message, detalle = ex.ToString() });
        //    }
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> SubirEntrega()
        //{
        //    try
        //    {
        //        var httpRequest = Request;
        //        if (httpRequest == null)
        //        {
        //            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //            return Json(new { mensaje = "No se recibió la solicitud." });
        //        }

        //        string respuestaRaw = httpRequest.Form["Respuesta"] ?? string.Empty;
        //        string enlacesJson = httpRequest.Form["Enlaces"] ?? "[]";
        //        string fechaEntrega = httpRequest.Form["FechaEntrega"] ?? DateTime.Now.ToString("O");


        //        int.TryParse(httpRequest.Form["ActividadId"], out int actividadId);
        //        int.TryParse(httpRequest.Form["AlumnoId"], out int alumnoId);
        //        int.TryParse(httpRequest.Form["TipoEntregaId"], out int tipoEntregaId);

        //        #region Validaciones entrega
        //        var actividad = Db.tbActividades.FirstOrDefault(a => a.ActividadId == actividadId);

        //        var limiteEntrega = actividad.LimiteEntregasPorAlumno;

        //        var tieneLimiteEntregas = actividad.TieneLimiteEntregas;

        //        var entregasAlumno = Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId).ToList();
        //        if (tieneLimiteEntregas)
        //        {
        //            var totalEntregasPorAlumno = entregasAlumno.Count;
        //            if (totalEntregasPorAlumno > limiteEntrega)
        //            {
        //                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //                return Json(new { mensaje = "Has llegado a tu limite de entregas asignado por el docente." });
        //            }

        //        }

        //        #endregion

        //        // PROCESAR RESPUESTA: detectar si viene JSON stringifyado
        //        string textoRespuesta = respuestaRaw;
        //        List<string> enlacesValidos = new List<string>();
        //        List<object> archivosExternos = new List<object>();

        //        try
        //        {
        //            if (!string.IsNullOrEmpty(respuestaRaw) && respuestaRaw.TrimStart().StartsWith("{"))
        //            {
        //                var respuestaObj = JsonConvert.DeserializeObject<dynamic>(respuestaRaw);
        //                textoRespuesta = respuestaObj.texto ?? respuestaObj.Respuesta ?? "";

        //                // Procesar enlaces del JSON interno
        //                if (respuestaObj.enlaces != null)
        //                {
        //                    var enlacesTemp = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(respuestaObj.enlaces));
        //                    foreach (var enlace in enlacesTemp)
        //                    {
        //                        if (_validarURL(enlace))
        //                            enlacesValidos.Add(enlace);
        //                    }
        //                }

        //                // Procesar archivos del JSON interno (URLs de archivos subidos)
        //                if (respuestaObj.archivos != null)
        //                {
        //                    var archivosTemp = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(respuestaObj.archivos));
        //                    foreach (var archivo in archivosTemp)
        //                    {
        //                        if (archivo != null)
        //                            archivosExternos.Add(archivo);
        //                    }
        //                }

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            textoRespuesta = respuestaRaw;
        //        }


        //        if (actividadId == 0 || alumnoId == 0)
        //        {
        //            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //            return Json(new { mensaje = "Faltan datos: AlumnoId o ActividadId." });
        //        }

        //        DateTime fechaEntregaParsed;
        //        try
        //        {
        //            fechaEntregaParsed = DateTime.Parse(fechaEntrega);
        //        }
        //        catch
        //        {
        //            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //            return Json(new { mensaje = "Formato de fecha inválido" });
        //        }

        //        try
        //        {
        //            var enlaces = JsonConvert.DeserializeObject<List<string>>(enlacesJson) ?? new List<string>();
        //            foreach (var enlace in enlaces)
        //            {
        //                if (_validarURL(enlace))
        //                {
        //                    if (!enlacesValidos.Contains(enlace))
        //                    {
        //                        enlacesValidos.Add(enlace);
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //        }


        //        // 4. VERIFICAR SI YA EXISTE ENTREGA

        //        var entregaActiva = entregasAlumno.FirstOrDefault(a => a.Estatus);

        //        var entregaExistente = await Db.tbEntregaActividadAlumno
        //            .FirstOrDefaultAsync(e => e.EntregaActividadAlumnoId == entregaActiva.EntregaActividadAlumnoId);


        //        tbEntregaActividadAlumno entregaActividad;
        //        int entregaActividadAlumnoId = 0;


        //        var fechaLimite = actividad.FechaLimite;
        //        var permiteEntregaTardia = actividad.PermitirEntregasTarde;

        //        if (entregaActiva.FechaEntrega > fechaLimite && !actividad.PermitirEntregasTarde)
        //        {
        //            Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //            return Json(new { mensaje = $"La fecha de entrega es el {fechaEntrega:dd/MM/yyyy} a las {fechaEntrega:HH:mm}" });
        //        }

        //        entregaActividad = new tbEntregaActividadAlumno()
        //        {
        //            ActividadId = actividadId,
        //            AlumnoId = alumnoId,
        //            FechaEntrega = fechaEntregaParsed,
        //            EstadoEntregaId = 1,
        //            EntregaTardia = (entregaActiva.FechaEntrega > fechaLimite)
        //        };


        //        Db.tbEntregaActividadAlumno.Add(entregaActividad);
        //        await Db.SaveChangesAsync();

        //        entregaActividadAlumnoId = entregaActividad.EntregaActividadAlumnoId;

        //        // Guardar archivos (si los hay)
        //        var archivosMetadata = new List<object>();
        //        var savedUrls = new List<string>();
        //        var files = httpRequest.Files;
        //        var uploadRoot = Server.MapPath("~/Uploads/Entregas/");
        //        var destFolder = Path.Combine(uploadRoot, actividadId.ToString(), alumnoId.ToString());
        //        if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);



        //        var extensionesPermitidas = new[]
        //        {
        //            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        //            ".jpg", ".jpeg", ".png", ".gif", ".txt", ".zip", ".rar", ".7z",
        //            ".odt", ".ods", ".odp", ".rtf"
        //        };

        //        const long maxPorArchivo = 50 * 1024 * 1024;
        //        const long maxTotal = 200 * 1024 * 1024;
        //        long tamanoTotal = 0;


        //        for (int i = 0; i < files.Count; i++)
        //        {
        //            var file = files[i];
        //            if (file == null || file.ContentLength == 0)
        //            {
        //                Console.WriteLine($"[LOG] Archivo {i} vacío, se omite");
        //                continue;
        //            }

        //            var extension = Path.GetExtension(file.FileName).ToLower();

        //            // Validar extensión
        //            if (!extensionesPermitidas.Contains(extension))
        //            {
        //                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //                return Json(new { mensaje = $"Extensión no permitida: {extension}" });
        //            }

        //            // Validar tamaño individual
        //            if (file.ContentLength > maxPorArchivo)
        //            {
        //                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //                return Json(new { mensaje = $"Archivo excede 50MB" });
        //            }

        //            tamanoTotal += file.ContentLength;

        //            // Validar tamaño total
        //            if (tamanoTotal > maxTotal)
        //            {
        //                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
        //                return Json(new { mensaje = $"Tamaño total excede 200MB" });

        //            }

        //            // Guardar archivo
        //            var safeName = Path.GetFileName(file.FileName);
        //            var destPath = Path.Combine(destFolder, safeName);

        //            if (System.IO.File.Exists(destPath))
        //            {
        //                var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        //                safeName = $"{ts}_{safeName}";
        //                destPath = Path.Combine(destFolder, safeName);
        //            }


        //            file.SaveAs(destPath);
        //            var ruta = $"/Uploads/Entregas/{actividadId}/{alumnoId}/{safeName}";

        //            archivosMetadata.Add(new
        //            {
        //                nombre = file.FileName,
        //                nombreGuardado = safeName,
        //                size = file.ContentLength,
        //                ruta = ruta,
        //                fechaGuardado = DateTime.Now
        //            });

        //        }
        //        // 6. DETERMINAR TIPO DE ENTREGA
        //        int tipoEntregaDeterminado = _determinarTipoEntrega(textoRespuesta, enlacesValidos, archivosMetadata);

        //        // Combinar archivos subidos directamente con archivos del JSON (URLs)
        //        var todosArchivos = new List<object>();
        //        todosArchivos.AddRange(archivosMetadata);
        //        todosArchivos.AddRange(archivosExternos);

        //        // 7. CREAR ENTREGABLE CON TODO ESTRUCTURADO
        //        var contenidoEstructurado = new
        //        {
        //            texto = textoRespuesta,
        //            enlaces = enlacesValidos,
        //            archivos = todosArchivos,
        //            fechaEntrega = DateTime.Now,
        //            totalArchivos = todosArchivos.Count,
        //            totalEnlaces = enlacesValidos.Count
        //        };

        //        var entregable = new tbEntregables()
        //        {
        //            EntregaActividadAlumnoId = entregaActividadAlumnoId,
        //            TipoEntregaId = tipoEntregaDeterminado,
        //            Contenido = JsonConvert.SerializeObject(contenidoEstructurado),
        //            Calificacion = null
        //        };

        //        Db.tbEntregables.Add(entregable);
        //        await Db.SaveChangesAsync();

        //        Console.WriteLine($"[LOG] Entregable creado: {entregable.EntregableId}");

        //        // 8. LIMPIAR CACHÉ
        //        Db.ChangeTracker.Entries()
        //            .Where(e => e.Entity is tbEntregables || e.Entity is tbEntregaActividadAlumno)
        //            .ToList()
        //            .ForEach(e => e.State = System.Data.Entity.EntityState.Detached);

        //        // 9. RETORNAR RESPUESTA
        //        var datosAlumnoActividad = await Db.tbEntregaActividadAlumno
        //            .FirstOrDefaultAsync(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId);

        //        var lsDatosEntregables = Db.tbEntregables
        //            .Where(a => a.EntregaActividadAlumnoId == datosAlumnoActividad.EntregaActividadAlumnoId)
        //            .ToList();

        //        var lsEnvios = new List<object>();

        //        foreach (var datoEntregable in lsDatosEntregables)
        //        {
        //            lsEnvios.Add(new
        //            {
        //                AlumnoId = alumnoId,
        //                EntregaActividadAlumnoId = datoEntregable.EntregaActividadAlumnoId,
        //                EntregableId = datoEntregable.EntregableId,
        //                ActividadId = datosAlumnoActividad.ActividadId,
        //                FechaEntrega = datosAlumnoActividad.FechaEntrega,
        //                Contenido = datoEntregable.Contenido,
        //                Calificacion = datoEntregable.Calificacion ?? 0,
        //                EstadoEntregaId = datosAlumnoActividad.EstadoEntregaId,
        //                TipoEntrega = tipoEntregaDeterminado
        //            });
        //        }


        //        Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
        //        return Json(new SuccessResponse
        //        {
        //            Mensaje = $"Entrega registrada correctamente ({archivosMetadata.Count} archivo(s), {enlacesValidos.Count} enlace(s))",
        //            Codigo = "EXITO",
        //            Datos = lsEnvios
        //        }, JsonRequestBehavior.AllowGet);

        //    }
        //    catch (Exception ex)
        //    {
        //        Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
        //        return Json(new { mensaje = ex.Message, detalle = ex.ToString() });
        //    }
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubirEntrega()
        {
            try
            {
                var httpRequest = System.Web.HttpContext.Current.Request;

                if (httpRequest == null)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new ErrorResponse
                    {
                        Mensaje = "Solicitud vacía",
                        Codigo = "SOLICITUD_VACIA",
                        Detalles = "El servidor no recibió ninguna solicitud HTTP válida."
                    });
                }

                // 1. EXTRAER PARÁMETROS
                string respuestaRaw = httpRequest.Form["Respuesta"] ?? string.Empty;
                string enlacesJson = httpRequest.Form["Enlaces"] ?? "[]";
                string fechaEntrega = httpRequest.Form["FechaEntrega"] ?? DateTime.Now.ToString("O");

                int.TryParse(httpRequest.Form["ActividadId"], out int actividadId);
                int.TryParse(httpRequest.Form["AlumnoId"], out int alumnoId);
                int.TryParse(httpRequest.Form["TipoEntregaId"], out int tipoEntregaId);

                if (actividadId <= 0 || alumnoId <= 0)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return Json(new ErrorResponse
                    {
                        Mensaje = "Datos inválidos",
                        Codigo = "DATOS_INVALIDOS",
                        Detalles = "ActividadId y AlumnoId son obligatorios"
                    });
                }

                var lsEnvios = await AlumnoApiService.RegistrarEnvioActividadAlumnoConEnlaces(httpRequest, actividadId, alumnoId, tipoEntregaId, fechaEntrega, respuestaRaw, enlacesJson);

                // 3. RESPUESTA EXITOSA
                Response.StatusCode = (int)HttpStatusCode.OK;
                return Json(new SuccessResponse
                {
                    Mensaje = "Entrega registrada correctamente",
                    Codigo = "EXITO",
                    Datos = lsEnvios
                });
            }
            catch (EntregaAlumnoException e)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new ErrorResponse
                {
                    Mensaje = "Error en la entrega",
                    Codigo = "ERROR_ENTREGA",
                    Detalles = e.Message
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new
                {
                    mensaje = "Error interno del servidor",
                    detalle = ex.Message
                });
            }
        }



        //private string BuildRespuestaWithFiles(string respuesta, List<string> files)
        //{
        //    try
        //    {
        //        var obj = new { Respuesta = respuesta ?? string.Empty, Archivos = files ?? new List<string>() };
        //        return JsonConvert.SerializeObject(obj);
        //    }
        //    catch
        //    {
        //        return respuesta ?? string.Empty;
        //    }
        //}

        //private async Task<int> ResolveTipoEntregaIdAsync(int preferido)
        //{
        //    try
        //    {
        //        var existe = await Db.cTipoEntrega.AnyAsync(t => t.TipoActividadId == preferido);
        //        if (existe) return preferido;

        //        var anyId = await Db.cTipoEntrega.Select(t => (int?)t.TipoActividadId).FirstOrDefaultAsync();
        //        if (anyId.HasValue) return anyId.Value;

        //        var texto = new cTipoEntrega { Nombre = "Texto" };
        //        var archivo = new cTipoEntrega { Nombre = "Archivo" };
        //        Db.cTipoEntrega.Add(texto);
        //        Db.cTipoEntrega.Add(archivo);
        //        await Db.SaveChangesAsync();

        //        if (preferido == 1) return texto.TipoActividadId;
        //        if (preferido == 2) return archivo.TipoActividadId;

        //        return texto.TipoActividadId;
        //    }
        //    catch
        //    {
        //        return preferido > 0 ? preferido : 1;
        //    }
        //}

        //private async Task<int> ResolveEstadoEntregaIdAsync(int preferido)
        //{
        //    try
        //    {
        //        var existe = await Db.cEstadoEntrega.AnyAsync(e => e.EstadoEntregaId == preferido);
        //        if (existe) return preferido;

        //        var anyId = await Db.cEstadoEntrega.Select(e => (int?)e.EstadoEntregaId).FirstOrDefaultAsync();
        //        if (anyId.HasValue) return anyId.Value;

        //        var recibido = new cEstadoEntrega { Nombre = "Recibida" };
        //        var pendiente = new cEstadoEntrega { Nombre = "Pendiente" };
        //        Db.cEstadoEntrega.Add(recibido);
        //        Db.cEstadoEntrega.Add(pendiente);
        //        await Db.SaveChangesAsync();

        //        return preferido == 1 ? recibido.EstadoEntregaId : recibido.EstadoEntregaId;
        //    }
        //    catch
        //    {
        //        return preferido > 0 ? preferido : 1;
        //    }
        //}

        //private async Task<bool> AlumnoPuedeAccederActividadAsync(int alumnoId, int actividadId)
        //{
        //    var actividad = await Db.tbActividades.FindAsync(actividadId);
        //    if (actividad == null) return false;

        //    bool bloqueadaPorProgramacion = actividad.Enviado == null && actividad.FechaProgramada.HasValue && actividad.FechaProgramada.Value > DateTime.Now;
        //    if (bloqueadaPorProgramacion) return false;

        //    int materiaId = actividad.MateriaId;
        //    var perteneceMateria = await Db.tbAlumnosMaterias.AnyAsync(am => am.AlumnoId == alumnoId && am.MateriaId == materiaId);
        //    if (perteneceMateria) return true;

        //    var gruposIds = await Db.tbGruposMaterias.Where(gm => gm.MateriaId == materiaId).Select(gm => gm.GrupoId).ToListAsync();
        //    if (gruposIds != null && gruposIds.Count > 0)
        //    {
        //        var perteneceGrupo = await Db.tbAlumnosGrupos.AnyAsync(ag => ag.AlumnoId == alumnoId && gruposIds.Contains(ag.GrupoId));
        //        if (perteneceGrupo) return true;
        //    }

        //    return false;
        //}
        #endregion

        #region ImportarAlumnosExcel
        [HttpPost]
        public async Task<ActionResult> ImportarAlumnosExcel()
        {
            try
            {
                var httpRequest = System.Web.HttpContext.Current.Request;
                if (httpRequest == null || httpRequest.Files.Count ==0)
                {
                    Response.StatusCode =400;
                    return Json(new { mensaje = "No se recibió archivo." });
                }

                var file = httpRequest.Files[0];
                if (file == null || file.ContentLength ==0)
                {
                    Response.StatusCode =400;
                    return Json(new { mensaje = "Archivo vacío." });
                }

                int grupoId =0;
                int materiaId =0;
                int.TryParse(httpRequest.Form["GrupoId"], out grupoId);
                int.TryParse(httpRequest.Form["MateriaId"], out materiaId);

                if (grupoId ==0 && materiaId ==0)
                {
                    Response.StatusCode =400;
                    return Json(new { mensaje = "Debe enviar GrupoId o MateriaId." });
                }

                IWorkbook workbook;
                using (var stream = file.InputStream)
                {
                    if (file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);
                }

                var sheet = workbook.GetSheetAt(0);
                if (sheet == null)
                {
                    Response.StatusCode =400;
                    return Json(new { mensaje = "Hoja no encontrada en el archivo." });
                }

                int startRow = sheet.FirstRowNum;
                var headerRow = sheet.GetRow(startRow);
                bool hasHeader = false;
                var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var formatter = new NPOI.SS.UserModel.DataFormatter();

                if (headerRow != null)
                {
                    var headerCells = headerRow.LastCellNum >0 ? headerRow.LastCellNum :1;
                    for (int hc =0; hc < headerCells; hc++)
                    {
                        var hCell = headerRow.GetCell(hc);
                        var raw = hCell != null ? formatter.FormatCellValue(hCell) : null;
                        if (string.IsNullOrWhiteSpace(raw)) continue;
                        var hText = raw.Trim().ToLowerInvariant();
                        var norm = hText.Replace(" ", "").Replace("_", "");
                        if (norm.Contains("email") || norm.Contains("correo")) headerMap["email"] = hc;
                        else if (norm.Contains("nombre") || norm.Contains("nombres") || norm == "name") headerMap["nombre"] = hc;
                        else if (norm.Contains("apellidopaterno") || norm.Contains("paterno")) headerMap["apellidopaterno"] = hc;
                        else if (norm.Contains("apellidomaterno") || norm.Contains("materno")) headerMap["apellidomaterno"] = hc;
                    }

                    if (headerMap.ContainsKey("email") || headerMap.ContainsKey("nombre")) hasHeader = true;
                }

                var parsedRows = new List<Tuple<string, string, string, string>>(); // email,nombre,apeP,apeM

                for (int r = hasHeader ? startRow +1 : startRow; r <= sheet.LastRowNum; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;

                    string emailFound = null;
                    string nombreVal = null;
                    string apePVal = null;
                    string apeMVal = null;

                    if (hasHeader)
                    {
                        if (headerMap.TryGetValue("email", out int eIdx))
                        {
                            var c = row.GetCell(eIdx);
                            emailFound = c != null ? formatter.FormatCellValue(c)?.Trim() : null;
                        }
                        if (headerMap.TryGetValue("nombre", out int nIdx))
                        {
                            var c = row.GetCell(nIdx);
                            nombreVal = c != null ? formatter.FormatCellValue(c)?.Trim() : null;
                        }
                        if (headerMap.TryGetValue("apellidopaterno", out int pIdx))
                        {
                            var c = row.GetCell(pIdx);
                            apePVal = c != null ? formatter.FormatCellValue(c)?.Trim() : null;
                        }
                        if (headerMap.TryGetValue("apellidomaterno", out int mIdx))
                        {
                            var c = row.GetCell(mIdx);
                            apeMVal = c != null ? formatter.FormatCellValue(c)?.Trim() : null;
                        }

                        if (string.IsNullOrWhiteSpace(emailFound))
                        {
                            var lastCell = row.LastCellNum >0 ? row.LastCellNum :1;
                            for (int c =0; c < lastCell; c++)
                            {
                                var cell = row.GetCell(c);
                                if (cell == null) continue;
                                var cellText = formatter.FormatCellValue(cell)?.Trim();
                                if (string.IsNullOrWhiteSpace(cellText)) continue;
                                if (cellText.Contains("@"))
                                {
                                    emailFound = cellText;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        var lastCell = row.LastCellNum >0 ? row.LastCellNum :1;
                        for (int c =0; c < lastCell; c++)
                        {
                            var cell = row.GetCell(c);
                            if (cell == null) continue;
                            var cellText = formatter.FormatCellValue(cell)?.Trim();
                            if (string.IsNullOrWhiteSpace(cellText)) continue;
                            if (cellText.Contains("@"))
                            {
                                emailFound = cellText;
                                break;
                            }
                            // if no email, try to set nombre from first non-empty cell
                            if (string.IsNullOrWhiteSpace(nombreVal)) nombreVal = cellText;
                            else if (string.IsNullOrWhiteSpace(apePVal)) apePVal = cellText;
                            else if (string.IsNullOrWhiteSpace(apeMVal)) apeMVal = cellText;
                        }
                    }

                    // normalize
                    emailFound = string.IsNullOrWhiteSpace(emailFound) ? null : emailFound.Trim().ToLowerInvariant();
                    if (emailFound != null && !emailFound.Contains("@")) emailFound = null;

                    // accept rows with either an email or at least a name
                    if (string.IsNullOrWhiteSpace(emailFound) && string.IsNullOrWhiteSpace(nombreVal) && string.IsNullOrWhiteSpace(apePVal) && string.IsNullOrWhiteSpace(apeMVal)) continue;

                    parsedRows.Add(Tuple.Create(emailFound ?? string.Empty, nombreVal ?? string.Empty, apePVal ?? string.Empty, apeMVal ?? string.Empty));
                }

                var added = new List<string>();
                var skipped = new List<string>();
                var notFound = new List<string>();
                var ambiguous = new List<string>();
                var lsAlumnosId = new List<int>();
                var processedAlumnoIds = new HashSet<int>();

                foreach (var row in parsedRows)
                {
                    string email = string.IsNullOrWhiteSpace(row.Item1) ? null : row.Item1;
                    string nombre = string.IsNullOrWhiteSpace(row.Item2) ? null : row.Item2;
                    string apeP = string.IsNullOrWhiteSpace(row.Item3) ? null : row.Item3;
                    string apeM = string.IsNullOrWhiteSpace(row.Item4) ? null : row.Item4;

                    int alumnoId =0;

                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        var user = await UserManager.FindByEmailAsync(email);
                        if (user != null)
                        {
                            alumnoId = await Db.tbAlumnos.Where(a => a.UserId == user.Id).Select(a => a.AlumnoId).FirstOrDefaultAsync();
                        }
                    }

                    // if not found by email, attempt lookup by name/apellidos
                    if (alumnoId ==0 && (!string.IsNullOrWhiteSpace(nombre) || !string.IsNullOrWhiteSpace(apeP) || !string.IsNullOrWhiteSpace(apeM)))
                    {
                        // If full name provided, try to match exactly first
                        IQueryable<tbAlumnos> query = Db.tbAlumnos;
                        if (!string.IsNullOrWhiteSpace(nombre))
                        {
                            var nm = nombre.Trim();
                            query = query.Where(a => a.Nombre.ToLower().Contains(nm.ToLower()));
                        }
                        if (!string.IsNullOrWhiteSpace(apeP))
                        {
                            var ap = apeP.Trim();
                            query = query.Where(a => a.ApellidoPaterno.ToLower().Contains(ap.ToLower()));
                        }
                        if (!string.IsNullOrWhiteSpace(apeM))
                        {
                            var am = apeM.Trim();
                            query = query.Where(a => a.ApellidoMaterno.ToLower().Contains(am.ToLower()));
                        }

                        var matches = await query.Take(10).ToListAsync();
                        if (matches.Count ==1)
                        {
                            alumnoId = matches[0].AlumnoId;
                        }
                        else if (matches.Count >1)
                        {
                            // ambiguous: pick first but record ambiguity
                            alumnoId = matches[0].AlumnoId;
                            ambiguous.Add($"{nombre ?? ""} {apeP ?? ""} {apeM ?? ""} (coincidencias: {matches.Count})");
                        }
                    }

                    if (alumnoId ==0)
                    {
                        notFound.Add(email ?? (nombre + " " + apeP + " " + apeM).Trim());
                        continue;
                    }

                    if (processedAlumnoIds.Contains(alumnoId))
                    {
                        skipped.Add(alumnoId.ToString());
                        continue;
                    }

                    lsAlumnosId.Add(alumnoId);
                    processedAlumnoIds.Add(alumnoId);

                    if (grupoId >0)
                    {
                        bool existe = Db.tbAlumnosGrupos.Any(a => a.GrupoId == grupoId && a.AlumnoId == alumnoId);
                        if (!existe)
                        {
                            Db.tbAlumnosGrupos.Add(new tbAlumnosGrupos { AlumnoId = alumnoId, GrupoId = grupoId });
                            added.Add(email ?? (nombre + " " + apeP).Trim());
                        }
                        else skipped.Add(email ?? (nombre + " " + apeP).Trim());
                    }
                    else if (materiaId >0)
                    {
                        bool existe = Db.tbAlumnosMaterias.Any(a => a.MateriaId == materiaId && a.AlumnoId == alumnoId);
                        if (!existe)
                        {
                            Db.tbAlumnosMaterias.Add(new tbAlumnosMaterias { AlumnoId = alumnoId, MateriaId = materiaId });
                            added.Add(email ?? (nombre + " " + apeP).Trim());
                        }
                        else skipped.Add(email ?? (nombre + " " + apeP).Trim());
                    }
                }

                await Db.SaveChangesAsync();

                var alumnos = await (from a in Db.tbAlumnos
                where lsAlumnosId.Contains(a.AlumnoId)
                join u in Db.Users on a.UserId equals u.Id into uj
                from u in uj.DefaultIfEmpty()
                select new EmailVerificadoAlumno
                {
                    Email = u.Email ?? "",
                    UserName = u.UserName ?? "",
                    Nombre = a.Nombre,
                    ApellidoPaterno = a.ApellidoPaterno,
                    ApellidoMaterno = a.ApellidoMaterno
                }).ToListAsync();

                return Json(new
                {
                    TotalLeidos = parsedRows.Count,
                    Agregados = added,
                    Omitidos = skipped,
                    NoEncontrados = notFound,
                    Ambiguos = ambiguous,
                    Alumnos = alumnos
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode =400;
                return Json(new { mensaje = ex.Message });
            }
        }
        #endregion
    }
}
