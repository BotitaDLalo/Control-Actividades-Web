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
using ControlActividades.Filters;

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

        #endregion

        [CustomAuthorize(Roles = "Alumno")]
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
                    //EntregaTardia = datosEntregable.EntregaTardia
                };

                if (datosEntregable != null && datosEntregable.FechaCalificado != null)
                {
                    model.FechaCalificado = datosEntregable.FechaCalificado.Value;
                }

                if (datosEntregable != null && datosEntregable.EntregaTardia)
                {
                    model.EntregaTardia = datosEntregable.EntregaTardia;
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
