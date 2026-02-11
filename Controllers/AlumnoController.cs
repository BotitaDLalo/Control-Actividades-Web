using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

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
        public ActionResult ActividadDetalle(int actividadId)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var alumnoId = Db.tbAlumnos.Where(a => a.UserId == userId).Select(a => a.AlumnoId).FirstOrDefault();
                ViewBag.AlumnoId = alumnoId;
                ViewBag.ActividadId = actividadId;
                return View();
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

        public class ModeloNotif
        {
            public string targetToken { get; set; }
            public string title { get; set; }
            public string body { get; set; }
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

        #region SubirEntrega moved from API
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubirEntrega()
        {
            try
            {
                var httpRequest = Request;
                if (httpRequest == null)
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return Json(new { mensaje = "No se recibió la solicitud." });
                }

                int actividadId = 0;
                int alumnoId = 0;
                string respuesta = httpRequest.Form["Respuesta"] ?? string.Empty;

                int.TryParse(httpRequest.Form["ActividadId"], out actividadId);
                int.TryParse(httpRequest.Form["AlumnoId"], out alumnoId);

                if (actividadId == 0 || alumnoId == 0)
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                    return Json(new { mensaje = "Faltan datos: AlumnoId o ActividadId." });
                }

                // Guardar archivos (si los hay)
                var savedUrls = new List<string>();
                var files = httpRequest.Files;
                var uploadRoot = Server.MapPath("~/Uploads/Entregas/");
                var destFolder = Path.Combine(uploadRoot, actividadId.ToString(), alumnoId.ToString());
                if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    if (file == null || file.ContentLength == 0) continue;
                    var safeName = Path.GetFileName(file.FileName);
                    var destPath = Path.Combine(destFolder, safeName);
                    // evitar sobreescribir: agregar timestamp
                    if (System.IO.File.Exists(destPath))
                    {
                        var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        destPath = Path.Combine(destFolder, ts + "_" + safeName);
                    }
                    file.SaveAs(destPath);
                    var relative = "/Uploads/Entregas/" + actividadId + "/" + alumnoId + "/" + Path.GetFileName(destPath);
                    savedUrls.Add(relative);
                }

                // Preparar contenido: si hay archivos, guardar JSON con respuesta + archivos, si no sólo texto
                string contenidoGuardar = respuesta ?? string.Empty;
                if (savedUrls.Count > 0)
                {
                    contenidoGuardar = BuildRespuestaWithFiles(respuesta, savedUrls);
                }

                // Fecha de entrega (usar campo enviado o ahora)
                DateTime fechaEnt = DateTime.Now;
                DateTime parsedFecha;
                if (DateTime.TryParse(httpRequest.Form["FechaEntrega"], out parsedFecha)) fechaEnt = parsedFecha;

                // Asegurar que exista un EstadoEntrega válido (evitar violación de FK)
                int estadoResolved = await ResolveEstadoEntregaIdAsync(1);

                // Verificar permisos: el alumno debe pertenecer a la materia o a un grupo que la contiene
                if (!await AlumnoPuedeAccederActividadAsync(alumnoId, actividadId))
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return Json(new { mensaje = "No tienes permiso para entregar esta actividad." });
                }

                // Bloquear entregas fuera de la fecha límite si la actividad no permite entregas tarde
                var actividad = await Db.tbActividades.FindAsync(actividadId);
                if (actividad != null && DateTime.Now > actividad.FechaLimite && !actividad.PermitirEntregasTarde)
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return Json(new { mensaje = "La fecha límite ya pasó y no se permiten entregas tardías para esta actividad." });
                }

                int entregaAlumnoId = 0;
                tbEntregables entregables = null;
                // tbEntregableAlumno entregableLegacy = null; // legacy handling kept minimal here

                try
                {
                    var entregaAlumnoExistente = await Db.Set<tbEntregaActividadAlumno>().FirstOrDefaultAsync(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId);
                    if (entregaAlumnoExistente == null)
                    {
                        tbEntregaActividadAlumno entregaAlumno = new tbEntregaActividadAlumno()
                        {
                            ActividadId = actividadId,
                            AlumnoId = alumnoId,
                            FechaEntrega = fechaEnt,
                            EstadoEntregaId = estadoResolved
                        };

                        Db.Set<tbEntregaActividadAlumno>().Add(entregaAlumno);
                        await Db.SaveChangesAsync();
                        entregaAlumnoId = entregaAlumno.EntregaActividadAlumnoId;
                    }
                    else
                    {
                        entregaAlumnoExistente.FechaEntrega = fechaEnt;
                        entregaAlumnoExistente.EstadoEntregaId = estadoResolved;
                        await Db.SaveChangesAsync();
                        entregaAlumnoId = entregaAlumnoExistente.EntregaActividadAlumnoId;
                    }

                    int preferido = (savedUrls.Count > 0) ? 2 : 1;
                    int tipoId = await ResolveTipoEntregaIdAsync(preferido);

                    entregables = new tbEntregables()
                    {
                        EntregaActividadAlumnoId = entregaAlumnoId,
                        TipoEntregaId = tipoId,
                        Contenido = contenidoGuardar
                    };
                    Db.tbEntregables.Add(entregables);
                    await Db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // intentar fallback legacy si es necesario
                    // Por simplicidad aquí devolvemos error con detalle
                    Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                    return Json(new { mensaje = "Error al guardar entregable.", detalle = ex.Message });
                }

                // Notificar al docente que un alumno entregó (FCM + persistir notificación)
                try
                {
                    var docenteUserId = await Db.tbMaterias.Where(m => m.MateriaId == actividad.MateriaId).Select(m => m.DocenteId).FirstOrDefaultAsync();
                    var docenteUid = await Db.tbDocentes.Where(d => d.DocenteId == docenteUserId).Select(d => d.UserId).FirstOrDefaultAsync();
                    if (!string.IsNullOrEmpty(docenteUid))
                    {
                        var ns = new NotificacionesService(Db, new FCMService());
                        string titulo = "Nueva entrega recibida";
                        string cuerpo = $"El alumno ha entregado la actividad {actividad.NombreActividad}.";
                        var tokens = await Db.tbUsuariosFcmTokens.Where(t => t.UserId == docenteUid).Select(t => new Models.UsuarioFcmToken { UserId = t.UserId, FcmToken = t.Token }).ToListAsync();
                        await ns.ProcesarNotificacion(new List<string> { docenteUid }, tokens, titulo, cuerpo, TiposNotificaciones.ActividadEntregada, actividad.MateriaId);
                    }
                }
                catch { }

                if (entregables != null)
                {
                    return Json(new
                    {
                        EntregaActividadAlumnoId = entregaAlumnoId,
                        EntregableId = entregables.EntregableId,
                        Contenido = entregables.Contenido
                    });
                }

                return Json(new { EntregaActividadAlumnoId = entregaAlumnoId });
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                return Json(new { mensaje = ex.Message, detalle = ex.ToString() });
            }
        }

        private string BuildRespuestaWithFiles(string respuesta, List<string> files)
        {
            try
            {
                var obj = new { Respuesta = respuesta ?? string.Empty, Archivos = files ?? new List<string>() };
                return JsonConvert.SerializeObject(obj);
            }
            catch
            {
                return respuesta ?? string.Empty;
            }
        }

        private async Task<int> ResolveTipoEntregaIdAsync(int preferido)
        {
            try
            {
                var existe = await Db.cTipoEntrega.AnyAsync(t => t.TipoActividadId == preferido);
                if (existe) return preferido;

                var anyId = await Db.cTipoEntrega.Select(t => (int?)t.TipoActividadId).FirstOrDefaultAsync();
                if (anyId.HasValue) return anyId.Value;

                var texto = new cTipoEntrega { Nombre = "Texto" };
                var archivo = new cTipoEntrega { Nombre = "Archivo" };
                Db.cTipoEntrega.Add(texto);
                Db.cTipoEntrega.Add(archivo);
                await Db.SaveChangesAsync();

                if (preferido == 1) return texto.TipoActividadId;
                if (preferido == 2) return archivo.TipoActividadId;

                return texto.TipoActividadId;
            }
            catch
            {
                return preferido > 0 ? preferido : 1;
            }
        }

        private async Task<int> ResolveEstadoEntregaIdAsync(int preferido)
        {
            try
            {
                var existe = await Db.cEstadoEntrega.AnyAsync(e => e.EstadoEntregaId == preferido);
                if (existe) return preferido;

                var anyId = await Db.cEstadoEntrega.Select(e => (int?)e.EstadoEntregaId).FirstOrDefaultAsync();
                if (anyId.HasValue) return anyId.Value;

                var recibido = new cEstadoEntrega { Nombre = "Recibida" };
                var pendiente = new cEstadoEntrega { Nombre = "Pendiente" };
                Db.cEstadoEntrega.Add(recibido);
                Db.cEstadoEntrega.Add(pendiente);
                await Db.SaveChangesAsync();

                return preferido == 1 ? recibido.EstadoEntregaId : recibido.EstadoEntregaId;
            }
            catch
            {
                return preferido > 0 ? preferido : 1;
            }
        }

        private async Task<bool> AlumnoPuedeAccederActividadAsync(int alumnoId, int actividadId)
        {
            var actividad = await Db.tbActividades.FindAsync(actividadId);
            if (actividad == null) return false;

            bool bloqueadaPorProgramacion = actividad.Enviado == null && actividad.FechaProgramada.HasValue && actividad.FechaProgramada.Value > DateTime.Now;
            if (bloqueadaPorProgramacion) return false;

            int materiaId = actividad.MateriaId;
            var perteneceMateria = await Db.tbAlumnosMaterias.AnyAsync(am => am.AlumnoId == alumnoId && am.MateriaId == materiaId);
            if (perteneceMateria) return true;

            var gruposIds = await Db.tbGruposMaterias.Where(gm => gm.MateriaId == materiaId).Select(gm => gm.GrupoId).ToListAsync();
            if (gruposIds != null && gruposIds.Count > 0)
            {
                var perteneceGrupo = await Db.tbAlumnosGrupos.AnyAsync(ag => ag.AlumnoId == alumnoId && gruposIds.Contains(ag.GrupoId));
                if (perteneceGrupo) return true;
            }

            return false;
        }
        #endregion

        #region ImportarAlumnosExcel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ImportarAlumnosExcel()
        {
            try
            {
                var httpRequest = Request;
                if (httpRequest == null || httpRequest.Files.Count ==0)
                    return Json(new { mensaje = "No se recibió archivo." }, JsonRequestBehavior.AllowGet);

                var file = httpRequest.Files[0];
                if (file == null || file.ContentLength ==0)
                    return Json(new { mensaje = "Archivo vacío." }, JsonRequestBehavior.AllowGet);

                int grupoId =0;
                int materiaId =0;
                int.TryParse(httpRequest.Form["GrupoId"], out grupoId);
                int.TryParse(httpRequest.Form["MateriaId"], out materiaId);

                if (grupoId ==0 && materiaId ==0)
                    return Json(new { mensaje = "Debe enviar GrupoId o MateriaId." }, JsonRequestBehavior.AllowGet);

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
                    return Json(new { mensaje = "Hoja no encontrada en el archivo." }, JsonRequestBehavior.AllowGet);

                int startRow = sheet.FirstRowNum;
                var headerRow = sheet.GetRow(startRow);
                bool hasHeader = false;
                if (headerRow != null)
                {
                    var headerCells = headerRow.LastCellNum >0 ? headerRow.LastCellNum :1;
                    for (int hc =0; hc < headerCells; hc++)
                    {
                        var hCell = headerRow.GetCell(hc);
                        var hText = hCell != null ? new DataFormatter().FormatCellValue(hCell)?.ToString()?.ToLower() : null;
                        if (!string.IsNullOrEmpty(hText) && hText.Contains("email"))
                        {
                            hasHeader = true;
                            break;
                        }
                    }
                }

                var emails = new List<string>();
                var formatter = new DataFormatter();
                for (int r = hasHeader ? startRow +1 : startRow; r <= sheet.LastRowNum; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;

                    string found = null;
                    var lastCell = row.LastCellNum >0 ? row.LastCellNum :1;
                    for (int c =0; c < lastCell; c++)
                    {
                        var cell = row.GetCell(c);
                        if (cell == null) continue;
                        var cellText = formatter.FormatCellValue(cell)?.Trim();
                        if (string.IsNullOrWhiteSpace(cellText)) continue;
                        if (cellText.Contains("@"))
                        {
                            found = cellText;
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(found)) continue;
                    var emailNormalized = found.Trim().ToLowerInvariant();
                    if (!emailNormalized.Contains("@")) continue;
                    emails.Add(emailNormalized);
                }

                if (!emails.Any())
                    return Json(new { mensaje = "No se encontraron emails en el archivo." }, JsonRequestBehavior.AllowGet);

                var added = new List<string>();
                var skipped = new List<string>();
                var notFound = new List<string>();
                var lsAlumnosId = new List<int>();

                foreach (var email in emails.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var user = await UserManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        try
                        {
                            var password = "Tmp#" + Guid.NewGuid().ToString("N").Substring(0,8);
                            var newUser = new ApplicationUser { UserName = email, Email = email };
                            var createResult = await UserManager.CreateAsync(newUser, password);
                            if (createResult.Succeeded)
                            {
                                var roleName = ControlActividades.Models.Role.Alumno.ToString();
                                if (!await RoleManager.RoleExistsAsync(roleName))
                                {
                                    await RoleManager.CreateAsync(new IdentityRole(roleName));
                                }
                                await UserManager.AddToRoleAsync(newUser.Id, roleName);
                                user = await UserManager.FindByEmailAsync(email);
                            }
                            else
                            {
                                // log errors and treat as not found
                                Console.WriteLine($"Crear usuario falló para {email}: {string.Join(";", createResult.Errors)}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error creando usuario para email " + email + ": " + ex.Message);
                        }
                    }

                    if (user == null)
                    {
                        notFound.Add(email);
                        continue;
                    }

                    var alumnoId = await Db.tbAlumnos.Where(a => a.UserId == user.Id).Select(a => a.AlumnoId).FirstOrDefaultAsync();
                    if (alumnoId ==0)
                    {
                        try
                        {
                            var nombrePart = (user.Email ?? "Alumno").Split('@')[0];
                            var nuevoAlumno = new tbAlumnos
                            {
                                UserId = user.Id,
                                Nombre = string.IsNullOrWhiteSpace(nombrePart) ? "Alumno" : nombrePart,
                                ApellidoPaterno = "N/A",
                                ApellidoMaterno = "N/D",
                                Matricula = user.Email ?? Guid.NewGuid().ToString()
                            };
                            Db.tbAlumnos.Add(nuevoAlumno);
                            try
                            {
                                await Db.SaveChangesAsync();
                                alumnoId = nuevoAlumno.AlumnoId;
                            }
                            catch (System.Data.Entity.Validation.DbEntityValidationException dbValEx)
                            {
                                foreach (var eve in dbValEx.EntityValidationErrors)
                                {
                                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:", eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                    foreach (var ve in eve.ValidationErrors)
                                    {
                                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                                    }
                                }
                                alumnoId =0;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error creando tbAlumnos para " + email + ": " + ex.Message);
                        }
                    }

                    if (alumnoId ==0)
                    {
                        notFound.Add(email);
                        continue;
                    }

                    lsAlumnosId.Add(alumnoId);

                    if (grupoId >0)
                    {
                        bool existe = Db.tbAlumnosGrupos.Any(a => a.GrupoId == grupoId && a.AlumnoId == alumnoId);
                        if (!existe)
                        {
                            Db.tbAlumnosGrupos.Add(new tbAlumnosGrupos { AlumnoId = alumnoId, GrupoId = grupoId });
                            added.Add(email);
                        }
                        else
                        {
                            skipped.Add(email);
                        }
                    }
                    else if (materiaId >0)
                    {
                        bool existe = Db.tbAlumnosMaterias.Any(a => a.MateriaId == materiaId && a.AlumnoId == alumnoId);
                        if (!existe)
                        {
                            Db.tbAlumnosMaterias.Add(new tbAlumnosMaterias { AlumnoId = alumnoId, MateriaId = materiaId });
                            added.Add(email);
                        }
                        else
                        {
                            skipped.Add(email);
                        }
                    }
                }

                try
                {
                    await Db.SaveChangesAsync();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbValEx)
                {
                    var detalles = new List<string>();
                    foreach (var eve in dbValEx.EntityValidationErrors)
                    {
                        foreach (var ve in eve.ValidationErrors)
                        {
                            detalles.Add($"{eve.Entry.Entity.GetType().Name}.{ve.PropertyName}: {ve.ErrorMessage}");
                        }
                    }
                    return Json(new { mensaje = "Validation failed", detalles }, JsonRequestBehavior.AllowGet);
                }

                var alumnos = (from a in Db.tbAlumnos
                                where lsAlumnosId.Contains(a.AlumnoId)
                                join u in Db.Users on a.UserId equals u.Id into uj
                                from u in uj.DefaultIfEmpty()
                                select new EmailVerificadoAlumno
                                {
                                    AlumnoId = a.AlumnoId,
                                    Email = u.Email ?? "",
                                    UserName = u.UserName ?? "",
                                    Nombre = a.Nombre,
                                    ApellidoPaterno = a.ApellidoPaterno,
                                    ApellidoMaterno = a.ApellidoMaterno
                                }).ToList();

                return Json(new
                {
                    TotalLeidos = emails.Count,
                    Agregados = added,
                    Omitidos = skipped,
                    NoEncontrados = notFound,
                    Alumnos = alumnos
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ImportarAlumnosExcel MVC error: " + ex.Message + "\n" + ex.StackTrace);
                return Json(new { mensaje = "Error al importar: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}
