using ControlActividades;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using NPOI.SS.Formula.Eval;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ControlMaterias.Controllers
{
    public class CopiarActividadesRequest
    {
        public int origenMateriaId { get; set; }
        public int nuevoMateriaId { get; set; }
    }

    public class MateriasController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private NotificacionesService _notifServ;
        private MateriasService _materiasService;

        public MateriasController()
        {
        }
        public MateriasController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg, NotificacionesService notificacionesService)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
            Ns = notificacionesService;
        }

        #region Propiedades
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

        private MateriasService MateriasService
        {
            get
            {
                return _materiasService ?? (_materiasService = new MateriasService());
            }
            set
            {
                _materiasService = value;
            }
        }

        #endregion


        #region Materia
        public async Task<ActionResult> Index()
        {
            //var lsMaterias = ObtenerMateriasSinGrupoPorUsuario();
            int usuarioId = Fg.ObtenerCAUsuarioId(User);
            string role = Fg.ObtenerRolUsuario(User);
            int st_usuarioId = 0;

            List< MateriaCARes> lsMaterias = await MateriasService.ObtenerMateriasSinGrupoPorUsuario(usuarioId, st_usuarioId, role);

            return View(lsMaterias);
        }


        public async Task<ActionResult> MateriaDetalles(int? materiaId, int? grupoId)
        {
            // Redirigir a la vista docente centralizada `MateriasDetalles` para evitar mantener dos vistas casi idénticas.
            if (!materiaId.HasValue)
            {
                return RedirectToAction("Index");
            }



            // Si el usuario es alumno, redirigir a la vista de alumno correspondiente
            //try
            //{
            //    var rolUsuario = Fg.ObtenerRolUsuario(User);
            //    if (!string.IsNullOrEmpty(rolUsuario) && rolUsuario == Roles.ALUMNO)
            //    {
            //        // Preferir abrir la materia cuando se proporciona materiaId (aunque venga dentro de un grupo)
            //        if (materiaId.HasValue && materiaId.Value > 0)
            //        {
            //            return RedirectToAction("Clase", "Alumno", new { tipo = "materia", id = materiaId.Value });
            //        }

            //        if (grupoId.HasValue && grupoId.Value > 0)
            //        {
            //            return RedirectToAction("Clase", "Alumno", new { tipo = "grupo", id = grupoId.Value });
            //        }
            //    }
            //}
            //catch { /* si falla la obtención del rol seguir mostrando vista docente por defecto */ }

            //// asegurar que ViewBag tenga los ids necesarios para las vistas/JS (docente u otros roles)
            //try
            //{
            //    string userId = User != null ? User.Identity.GetUserId() : null;
            //    var docenteId = 0;
            //    if (!string.IsNullOrEmpty(userId))
            //        docenteId = Db.tbDocentes.Where(a => a.UserId == userId).Select(a => a.DocenteId).FirstOrDefault();

            //    ViewBag.DocenteId = docenteId;
            //    ViewBag.MateriaId = materiaId.HasValue ? materiaId.Value : 0;
            //    ViewBag.GrupoId = grupoId ?? 0;
            //}
            //catch { ViewBag.DocenteId = 0; ViewBag.MateriaId = materiaId ?? 0; ViewBag.GrupoId = grupoId ?? 0; }

            //var nombreMateria = Db.tbMaterias.Where(a => a.MateriaId == materiaId).Select(a => a.NombreMateria).FirstOrDefault();
            //ViewBag.NombreMateria = nombreMateria;

            int ca_usuarioId = Fg.ObtenerCAUsuarioId(User);
            int st_usuarioId = Fg.ObtenerSTUsuarioId(User);

            string role = Fg.ObtenerRolUsuario(User);

            var materiaDetalles = await MateriasService.ObtenerMateriaDetalles(materiaId.Value, grupoId ?? 0, role, ca_usuarioId, st_usuarioId);
            if (materiaDetalles == null)
                return RedirectToAction("Index");

            ViewBag.MateriaId = materiaId.HasValue ? materiaId.Value : 0;
            ViewBag.GrupoId = grupoId ?? 0;
            return View("MateriaDetalles");
        }


        [HttpPost]
        public async Task<ActionResult> CrearMateria(MateriasP materia)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { error = "Datos de materia inválidos." });
            }
            
            var usuarioId = Fg.ObtenerCAUsuarioId(User);
            var codigoAcceso = ObtenerClaveMateria();

            var materiadb = new tbMaterias
            {
                NombreMateria = materia.NombreMateria,
                Descripcion = materia.Descripcion,
                CodigoColor = materia.Color,
                CodigoAcceso = codigoAcceso,
                DocenteId = usuarioId,
            };

            Db.tbMaterias.Add(materiadb);
            await Db.SaveChangesAsync();

            return Json(new
            {
                mensaje = "Materia creada con éxito.",
                materiaId = materiadb.MateriaId
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        private async Task<tbMaterias>CrearMateriaInterna(string nombre, string descripcion, string color, int docenteId)
        {
            var codigoAcceso = ObtenerClaveMateria();

            var materiaDb = new tbMaterias
            {
                NombreMateria = nombre,
                Descripcion = descripcion,
                CodigoColor = color,
                CodigoAcceso = codigoAcceso,
                DocenteId = docenteId,
            };

            Db.tbMaterias.Add(materiaDb);
            await Db.SaveChangesAsync();

            return materiaDb;
        }

        [HttpPost]
        public async Task<ActionResult> CrearMateriaConGrupo(CrearMateriaConGrupoRequest request)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = "Datos inválidos." });
            }

            var docenteId = Fg.ObtenerCAUsuarioId(User);

            // Validar que el grupo exista y pertenezca al docente
            var grupo = await Db.tbGrupos
                .FirstOrDefaultAsync(g => g.GrupoId == request.GrupoId && g.DocenteId == docenteId);

            if (grupo == null)
            {
                Response.StatusCode = 404;
                return Json(new { mensaje = "Grupo no encontrado o no autorizado." });
            }

            // Crear materia reutilizando lógica
            var nuevaMateria = await CrearMateriaInterna(
                request.NombreMateria,
                request.Descripcion,
                request.Color,
                docenteId
            );

            // Crear relación
            var relacion = new tbGruposMaterias
            {
                GrupoId = request.GrupoId,
                MateriaId = nuevaMateria.MateriaId
            };

            Db.tbGruposMaterias.Add(relacion);
            await Db.SaveChangesAsync();

            return Json(new
            {
                mensaje = "Materia creada y asociada correctamente.",
                materiaId = nuevaMateria.MateriaId
            });
        }
        private string ObtenerClaveMateria()
        {
            var random = new Random();
            return new string(
                Enumerable.Range(0, 10)
                          .Select(_ => (char)random.Next('A', 'Z'))
                          .ToArray()
            );
        }

        #endregion

        #region Avisos
        //Controlador para crear un aviso funciona desde dentro de la materia
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CrearAviso(CrearAvisoRequest avisos)
        {
            if (avisos == null)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "Datos inválidos." });
            }

            if (avisos.FechaFin < avisos.FechaInicio)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = "La fecha de fin debe ser mayor a la fecha de inicio." });
            }

            if (avisos.FrecuenciaDias < 1)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = "Frecuencia inválida." });
            }

            try
            {
                var usuarioId = Fg.ObtenerCAUsuarioId(User);

                var nuevoAviso = new tbAvisos
                {
                    DocenteId = usuarioId,
                    Titulo = avisos.Titulo,
                    Descripcion = avisos.Descripcion,
                    Enlaces = string.IsNullOrWhiteSpace(avisos.Enlaces)
                                ? null
                                : avisos.Enlaces.Trim(),
                    GrupoId = avisos.GrupoId == 0 ? null : avisos.GrupoId,
                    MateriaId = avisos.MateriaId,
                    FechaCreacion = DateTime.Now,
                    FechaInicio = avisos.FechaInicio,
                    FechaFin = avisos.FechaFin,
                    FrecuenciaDias = avisos.FrecuenciaDias
                };

                Db.tbAvisos.Add(nuevoAviso);
                await Db.SaveChangesAsync();


                /*YA NO SE ENVÍA NOTIFICACIÓN INMEDIATA*/
                /*SE ENVIARÁ CON HANGFIRE*/

                return Json(new { mensaje = "Aviso creado con éxito" });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al crear el aviso", error = ex.Message });
            }
        }


        //Crea aviso cuando pues se crea un aviso desde el grupo
        [HttpPost]
        public async Task<ActionResult> CrearAvisoPorGrupo(tbAvisos datos)
        {
            if (datos == null || datos.GrupoId == null || string.IsNullOrWhiteSpace(datos.Titulo) || string.IsNullOrWhiteSpace(datos.Descripcion))
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "Datos inválidos." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                int? grupoId = datos.GrupoId;
                string titulo = datos.Titulo;
                string descripcion = datos.Descripcion;

                // Buscar todas las materias asociadas a ese GrupoId en la tabla tbGruposMaterias
                var materiasRelacionadas = await Db.tbGruposMaterias
                    .Where(gm => gm.GrupoId == grupoId)
                    .Select(gm => gm.MateriaId)
                    .ToListAsync();

                if (!materiasRelacionadas.Any())
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "No se encontraron materias asociadas a este grupo." }, JsonRequestBehavior.AllowGet);
                }

                // Crear un aviso para cada materia relacionada con el grupo
                var avisos = materiasRelacionadas.Select(materiaId => new tbAvisos
                {
                    DocenteId = datos.DocenteId, // Asegurar que venga en los datos
                    Titulo = titulo,
                    Descripcion = descripcion,
                    GrupoId = grupoId,
                    MateriaId = materiaId,
                    FechaCreacion = DateTime.Now
                }).ToList();

                Db.tbAvisos.AddRange(avisos);
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Avisos creados con éxito", cantidad = avisos.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al crear los avisos", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //Controlador para eliminar un aviso
        [HttpDelete]
        public async Task<ActionResult> EliminarAviso(int id)
        {
            if (id <= 0)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "ID de aviso inválido." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // Buscar el aviso por su ID
                var aviso = await Db.tbAvisos.FindAsync(id);

                // Si no se encuentra el aviso
                if (aviso == null)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "Aviso no encontrado." }, JsonRequestBehavior.AllowGet);
                }

                // Eliminar el aviso
                Db.tbAvisos.Remove(aviso);
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Aviso eliminado con éxito" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al eliminar el aviso", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //Controlador para obtener avisos para la vista
        [HttpGet]
        [Route("api/Avisos/ObtenerAvisos")]
        public async Task<ActionResult> ObtenerAvisos(int IdMateria)
        {
            try
            {
                var ahora = DateTime.Now;
                var rolUsuario = Fg.ObtenerRolUsuario(User);

                var query = Db.tbAvisos
                    .Where(a => a.MateriaId == IdMateria);

                //Si es alumno solo activos
                if (rolUsuario == "Alumno")
                {
                    query = query.Where(a =>
                        ahora >= a.FechaInicio &&
                        ahora <= a.FechaFin);
                }

                var avisosDb = await query
                    .OrderByDescending(a => a.FechaCreacion)
                    .ToListAsync();

                var avisos = avisosDb.Select(a => new
                {
                    a.AvisoId,
                    a.Titulo,
                    a.Descripcion,
                    a.Enlaces,
                    a.FrecuenciaDias,

                    FechaCreacion = a.FechaCreacion.ToString("dddd, d 'de' MMMM 'de' yyyy HH:mm:ss"),
                    FechaCreacionIso = a.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),

                    FechaInicio = a.FechaInicio.ToString("dd/MM/yyyy"),
                    FechaFin = a.FechaFin.ToString("dd/MM/yyyy"),

                    Estado = ahora < a.FechaInicio
                                ? "Programado"
                                : (ahora > a.FechaFin
                                    ? "Finalizado"
                                    : "Activo")
                });

                return Json(new
                {
                    avisos,
                    RolUsuario = rolUsuario
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new
                {
                    mensaje = "Error al obtener los avisos",
                    error = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //Método para obtener informacion de un aviso para despeus editar
        [HttpGet]
        public async Task<ActionResult> ObtenerAvisoPorId(int avisoId)
        {
            try
            {
                var avisoDb = await Db.tbAvisos
                    .FirstOrDefaultAsync(a => a.AvisoId == avisoId);

                if (avisoDb == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "Aviso no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                var aviso = new
                {
                    avisoDb.AvisoId,
                    avisoDb.Titulo,
                    avisoDb.Descripcion,
                    avisoDb.Enlaces,
                    FechaInicio = avisoDb.FechaInicio.ToString("yyyy-MM-dd"),
                    FechaFin = avisoDb.FechaFin.ToString("yyyy-MM-dd"),
                    avisoDb.FrecuenciaDias
                };

                return Json(aviso, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al obtener el aviso", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //Editar aviso
        [HttpPut]
        public async Task<ActionResult> EditarAviso(CrearAvisoRequest model)
        {
            try
            {
                var aviso = await Db.tbAvisos.FindAsync(model.AvisoId);

                if (aviso == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "Aviso no encontrado" });
                }

                aviso.Titulo = model.Titulo;
                aviso.Descripcion = model.Descripcion;
                aviso.Enlaces = string.IsNullOrWhiteSpace(model.Enlaces)
                                    ? null
                                    : model.Enlaces.Trim();

                aviso.FechaInicio = model.FechaInicio;
                aviso.FechaFin = model.FechaFin;
                aviso.FrecuenciaDias = model.FrecuenciaDias;

                if (model.FechaFin < model.FechaInicio)
                {
                    Response.StatusCode = 400;
                    return Json(new { mensaje = "La fecha de fin no puede ser menor que la fecha de inicio" });
                }

                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Aviso actualizado correctamente" });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al actualizar el aviso", error = ex.Message });
            }
        }
        #endregion

        #region Actividades
        // Controlador api que crea actividades y asigna a los alumnos
        [HttpPost]
        public async Task<ActionResult> CrearActividad(ActividadDTO actividadDto)
        {
            if (actividadDto == null)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "Datos inválidos." }, JsonRequestBehavior.AllowGet);
            }

            // Validar que la fecha límite sea en el futuro
            if (actividadDto.FechaLimite <= DateTime.Now)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "La fecha límite debe ser en el futuro." }, JsonRequestBehavior.AllowGet);
            }

            try
            {

                var actividad = await MateriasService.CrearActividadAsync(actividadDto);
                //Envío de notificación a los alumnos dentro de la materia
                /*
                await Ns.NotificacionCreaActividad(
                    actividadDto
                );*/
                
                return Json(new
                {   mensaje = "Actividad creada y asignada a los alumnos con éxito",
                    actividadId = actividad.ActividadId
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        //Controlador que obtiene  todo lo de actividades que pertecenen a esa materia
        [HttpGet]
        public async Task<ActionResult> ObtenerActividadesPorMateria(int materiaId)
        {
            try
            {
                // Load activities into memory first to avoid EF translation issues with DateTime.ToString(format)
                //bool esDocente = User != null && (User.IsInRole("Docente") || User.IsInRole("Administrador"));
                var query = Db.tbActividades.Where(a => a.MateriaId == materiaId).ToList();
                if (User.IsInRole(Roles.ALUMNO))
                {
                    // para alumnos mostrar actividades publicadas o programadas cuyo horario ya se cumplió
                    query = query.Where(a => a.Enviado == true || (a.Enviado == null && a.FechaProgramada.HasValue && a.FechaProgramada.Value <= DateTime.Now)).ToList();
                }
                var actividadesEntities = query;

                //if (actividadesEntities == null || actividadesEntities.Count == 0)
                //{
                //    Response.StatusCode = 404; // Not Found
                //    return Json(new { mensaje = "No hay actividades registradas para esta materia." }, JsonRequestBehavior.AllowGet);
                //}
                var rolUsuario = Fg.ObtenerRolUsuario(User);

                var resultado = actividadesEntities.Select(a => new
                {
                    a.ActividadId,
                    a.NombreActividad,
                    a.Descripcion,
                    FechaCreacion = a.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),
                    FechaLimite = a.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
                    a.Puntaje,
                    Enviado = a.Enviado,
                    FechaProgramada = a.FechaProgramada,
                    Rol = rolUsuario
                }).ToList();



                return Json(new
                {
                    Actividades = resultado,
                    RolUsuario = rolUsuario
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener las actividades", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Avisos
        //Controlador para crear un aviso funciona desde dentro de la materia
        [HttpPost]
        public async Task<ActionResult> CopiarActividades(CopiarActividadesRequest req)
        {
            if (req == null || req.origenMateriaId <= 0 || req.nuevoMateriaId <= 0)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = "Parámetros inválidos" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var actividades = await Db.tbActividades.Where(a => a.MateriaId == req.origenMateriaId).ToListAsync();
                if (actividades == null || actividades.Count == 0)
                {
                    return Json(new { mensaje = "No hay actividades para copiar" }, JsonRequestBehavior.AllowGet);
                }

                foreach (var a in actividades)
                {
                    var nueva = new tbActividades
                    {
                        NombreActividad = a.NombreActividad,
                        Descripcion = a.Descripcion,
                        FechaCreacion = DateTime.Now,
                        FechaLimite = a.FechaLimite,
                        //TipoActividadId = a.TipoActividadId,
                        Puntaje = a.Puntaje,
                        MateriaId = req.nuevoMateriaId
                    };
                    Db.tbActividades.Add(nueva);
                }

                await Db.SaveChangesAsync();
                return Json(new { mensaje = "Actividades copiadas" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al copiar actividades", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Entregables
        #endregion

        #region Alumnos
        [HttpGet]
        public async Task<ActionResult> BuscarAlumnosPorCorreo(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    Response.StatusCode = 400; // Bad Request
                    return Json(new { mensaje = "El criterio de búsqueda no puede estar vacío." }, JsonRequestBehavior.AllowGet);
                }

                // Trim query and limit results to avoid large payloads
                var q = query.Trim();

                var alumnosPorCorreo = await MateriasService.BuscarAlumnosPorCorreo(q);

                return Json(alumnosPorCorreo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al buscar alumnos", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Método para buscar el alumno por correo y asignarlo a la materia si no está asignado
        [HttpPost]
        public async Task<ActionResult> AsignarAlumnoMateria(string correo, int materiaId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(correo))
                {
                    Response.StatusCode = 400; // Bad Request
                    return Json(new { mensaje = "El correo no puede estar vacío." }, JsonRequestBehavior.AllowGet);
                }

                // Buscar el alumno por correo usando join a Users para evitar problemas de navegación
                var alumnoId = await (from a in Db.tbAlumnos
                                      join u in Db.Users on a.UserId equals u.Id
                                      where u.Email == correo
                                      select (int?)a.AlumnoId).FirstOrDefaultAsync();

                if (alumnoId == null || alumnoId == 0)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "Alumno no encontrado con el correo proporcionado." }, JsonRequestBehavior.AllowGet);
                }

                // Verificar si ya existe la relación en la tabla alumnosMaterias
                var relacionExistente = await Db.tbAlumnosMaterias
                    .Where(am => am.AlumnoId == alumnoId && am.MateriaId == materiaId)
                    .FirstOrDefaultAsync();

                if (relacionExistente != null)
                {
                    Response.StatusCode = 400; // Bad Request
                    return Json(new { mensaje = "El alumno ya está asignado a esta materia." }, JsonRequestBehavior.AllowGet);
                }

                // Agregar nueva relación
                var nuevaRelacion = new tbAlumnosMaterias
                {
                    AlumnoId = alumnoId.Value,
                    MateriaId = materiaId
                };

                Db.tbAlumnosMaterias.Add(nuevaRelacion);
                await Db.SaveChangesAsync();

                //ENVÍO DE NOTIFICACIÓN
                await Ns.NotificacionRegistrarAlumnoClase(
                        new List<int> { alumnoId.Value },
                        docenteId: 0,
                        materiaId: materiaId
                    );

                return Json(new { mensaje = "Alumno asignado a la materia exitosamente.", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al asignar el alumno a la materia.", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        // Método para obtener la lista de alumnos que están dentro de la materia
        [HttpGet]
        public async Task<ActionResult> ObtenerAlumnosPorMateria(int materiaId)
        {
            try
            {
                // incluir correo del usuario relacionado (Identity User) para mostrar en la vista
                var alumnos = await Db.tbAlumnosMaterias
                    .Where(am => am.MateriaId == materiaId)
                    .Join(Db.tbAlumnos,
                        am => am.AlumnoId,
                        a => a.AlumnoId,
                        (am, a) => new { am, a })
                    .Join(Db.Users,
                        x => x.a.UserId,
                        u => u.Id,
                        (x, u) => new
                        {
                            x.am.AlumnoMateriaId,
                            x.a.AlumnoId,
                            x.a.Nombre,
                            x.a.ApellidoPaterno,
                            x.a.ApellidoMaterno,
                            Email = u.Email,
                            //Estatus = x.a.Estatus ?? "Activo"
                        })
                    .OrderBy(a => a.ApellidoPaterno)
                    .ThenBy(a => a.ApellidoMaterno)
                    .ThenBy(a => a.Nombre)
                    .ToListAsync();


                // Devolver lista junto con mensaje de OK
                return Json(new { alumnos = alumnos }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener los alumnos", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        //Eliminar a un alumno de la materia.
        [HttpDelete]
        public async Task<ActionResult> EliminarAlumnoDeMateria(int idEnlace)
        {
            try
            {
                // Buscar el registro en la base de datos
                var alumnoMateria = await Db.tbAlumnosMaterias
                    .FirstOrDefaultAsync(am => am.AlumnoMateriaId == idEnlace);

                // Si no se encuentra se retorna un error
                if (alumnoMateria == null)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "No se encontró el alumno en la materia" }, JsonRequestBehavior.AllowGet);
                }

                // Eliminar el registro de la base de datos
                Db.tbAlumnosMaterias.Remove(alumnoMateria);
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Alumno eliminado de la materia correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al eliminar al alumno.", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region Configuracion

        [HttpGet]
        public async Task<ActionResult> ObtenerMateriaEditar(int materiaId)
        {
            var materia = await Db.tbMaterias.FindAsync(materiaId);

            if (materia == null)
            {
                Response.StatusCode = 404;
                return Json(new { mensaje = "Materia no encontrada." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                MateriaId = materia.MateriaId,
                NombreMateria = materia.NombreMateria,
                Descripcion = materia.Descripcion
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public async Task<ActionResult> EliminarMateria(int id)
        {
            try
            {
                var materia = await Db.tbMaterias.FindAsync(id);
                if (materia == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "La materia no existe" }, JsonRequestBehavior.AllowGet);
                }

                var existenAlumnos = Db.tbAlumnosMaterias.Where(a => a.MateriaId == id).Any();
                if (existenAlumnos)
                    return Json(new { mensaje = "Ya existen alumnos inscritos a la materia", success = false }, JsonRequestBehavior.AllowGet);


                var existenActividades = Db.tbActividades.Where(a => a.MateriaId == id).Any();
                if (existenActividades)
                    return Json(new { mensaje = "Ya existen actividades creadas.", success = false }, JsonRequestBehavior.AllowGet);


                var existenAvisos = Db.tbAvisos.Where(a => a.MateriaId == id).Any();
                if (existenAvisos)
                    return Json(new { mensaje = "Ya existen avisos creados.", success = false }, JsonRequestBehavior.AllowGet);


                var relacionMateriaConGrupo = Db.tbGruposMaterias.Where(mg => mg.MateriaId == id);
                Db.tbGruposMaterias.RemoveRange(relacionMateriaConGrupo);

                Db.tbMaterias.Remove(materia);



                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Materia y sus relaciones eliminadas correctamente.", success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al eliminar la materia", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> ActualizarMateria(int materiaId, MateriasP materiaDto)
        {
            try
            {
                if (materiaDto == null)
                {
                    return Json(new { mensaje = "Datos no envidados correctamente." });
                }

                if (!ModelState.IsValid)
                {
                    Response.StatusCode = 400;
                    return Json(new { mensaje = "Datos inválidos" });
                }

                var materiaExistente = await Db.tbMaterias.FindAsync(materiaId);
                if (materiaExistente == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "Materia no encontrada." });
                }

                if (!string.IsNullOrWhiteSpace(materiaDto.NombreMateria))
                {
                    materiaExistente.NombreMateria = materiaDto.NombreMateria;
                }

                materiaExistente.Descripcion = materiaDto.Descripcion;
                if (!string.IsNullOrWhiteSpace(materiaDto.Descripcion))
                {
                    materiaExistente.Descripcion = materiaDto.Descripcion;
                }

                await Db.SaveChangesAsync();

                return Json(new
                {
                    MateriaId = materiaExistente.MateriaId,
                    NombreMateria = materiaExistente.NombreMateria,
                    Descripcion = materiaExistente.Descripcion
                });

            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new 
                { 
                    mensaje = "Error al actualizar la materia", 
                    error = ex.Message 
                });
            }
        }

        public async Task<ActionResult> CambiarCodigoAuto(int materiaId)
        {
            try
            {
                var materia = await Db.tbMaterias.FindAsync(materiaId);
                if (materia == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "Materia no encontrada" }, JsonRequestBehavior.AllowGet);
                }

                // Generar código único simple
                string nuevo;
                var rnd = new Random();
                do
                {
                    nuevo = new string(Enumerable.Range(0, 8).Select(_ => (char)rnd.Next('A', 'Z')).ToArray());
                } while (Db.tbMaterias.Any(m => m.CodigoAcceso == nuevo));

                materia.CodigoAcceso = nuevo;
                await Db.SaveChangesAsync();

                return Json(new { CodigoAcceso = nuevo }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al actualizar código", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<ActionResult> CambiarCodigo(int materiaId, tbMaterias dto)
        {
            try
            {
                var materia = await Db.tbMaterias.FindAsync(materiaId);
                if (materia == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "Materia no encontrada" }, JsonRequestBehavior.AllowGet);
                }

                if (dto == null || string.IsNullOrWhiteSpace(dto.CodigoAcceso))
                {
                    Response.StatusCode = 400;
                    return Json(new { mensaje = "Código inválido" }, JsonRequestBehavior.AllowGet);
                }

                materia.CodigoAcceso = dto.CodigoAcceso.Trim();
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Código actualizado" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al actualizar código", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public async Task<ActionResult> AsociarMateriasAGrupo(AsociarMateriasRequest request)
        {

            if (request == null || request.MateriaIds == null || !request.MateriaIds.Any())
            {
                return new HttpStatusCodeResult(400, "No se enviaron materias para asociar.");
            }

            try
            {
                foreach (var materiaId in request.MateriaIds)
                {
                    // Evita duplicados en la tabla intermedia
                    var existeAsociacion = await Db.tbGruposMaterias
                        .AnyAsync(gm => gm.GrupoId == request.GrupoId && gm.MateriaId == materiaId);

                    if (!existeAsociacion)
                    {
                        Db.tbGruposMaterias.Add(new tbGruposMaterias
                        {
                            GrupoId = request.GrupoId,
                            MateriaId = materiaId
                        });
                    }
                }

                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Materias asociadas correctamente al grupo." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al asociar materias: {ex.Message}");
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error interno al asociar materias al grupo." });
            }
        }

        #endregion



        #region PartialViews
        public ActionResult AvisosPartialView()
        {
            return PartialView("_Avisos");
        }

        public ActionResult ActividadesPartialView()
        {
            return PartialView("_Actividades");
        }

        public ActionResult EntregablesPartialView()
        {
            return PartialView("_Entregables");
        }

        public ActionResult AlumnosPartialView()
        {
            return PartialView("_Alumnos");
        }

        public ActionResult ConfiguracionPartialView()
        {

            ViewBag.NombreMateria = "";
            ViewBag.Descripcion = "";
            return PartialView("_Configuracion");
        }
        #endregion



        [HttpPost]
        public async Task<ActionResult> ActualizarEstatusAlumno(int AlumnoId, int MateriaId, string Estatus)
        {
            try
            {
                // buscar la relación específica alumno-materia
                var enlace = await Db.tbAlumnosMaterias.FirstOrDefaultAsync(a => a.AlumnoId == AlumnoId && a.MateriaId == MateriaId);
                if (enlace == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "No se encontró relación alumno-materia." }, JsonRequestBehavior.AllowGet);
                }

                //enlace.Estatus = Estatus;
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Estatus actualizado." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al actualizar estatus.", error = ex.Message }, JsonRequestBehavior.AllowGet);
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
