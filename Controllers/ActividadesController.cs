using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using ControlMaterias.Controllers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;


namespace ControlActividades.Controllers
{
    public class ActividadesController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private ActividadesService _actividadesService;

        public ActividadesController()
        {
        }

        // Controlador que obtiene todo lo de actividades que pertenecen a esa materia
        [HttpGet]
        public async Task<ActionResult> ObtenerActividadesPorMateria(int materiaId)
        {
            try
            {
                var rol = Fg.ObtenerRolUsuario(User);

                var actividades = await ActividadesService.ObtenerActividadesPorMateria(materiaId, rol);

                return Json(actividades, JsonRequestBehavior.AllowGet);
            }
            catch (KeyNotFoundException)
            {
                Response.StatusCode = 404; // Not Found
                return Json(new { mensaje = "No se encontraron actividades para la materia especificada." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener las actividades", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Constructores con dependencias
        public ActividadesController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg)
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

        public ActividadesService ActividadesService
        {
            get
            {
                return _actividadesService ?? (_actividadesService = new ActividadesService());
            }
            private set
            {
                _actividadesService = value;
            }
        }
        #endregion

        //Controlador para obtener los datos de una actividad
        [HttpGet]
        public async Task<ActionResult> ObtenerActividadPorId(int actividadId)
        {
            try
            {
                var actividad = await ActividadesService.ObtenerActividadPorId(actividadId);
                
                return Json(actividad, JsonRequestBehavior.AllowGet);
            }
            catch (KeyNotFoundException ex)
            {
                Response.StatusCode = 404; // Not Found
                return Json(new { mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener la actividad", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Redirige a la vista correspondiente según el rol (Docente -> EvaluarActividades, Alumno -> ActividadDetalle)
        [HttpGet]
        public ActionResult DetallesActividad(int actividadId)
        {
            try
            {
                var actividad = Db.tbActividades.FirstOrDefault(a => a.ActividadId == actividadId);
                if (actividad == null)
                {
                    return HttpNotFound("Actividad no encontrada");
                }

                // Si es docente o administrador, llevar a la vista de evaluación (docente)
                if (User != null && (User.IsInRole("Docente") || User.IsInRole("Administrador")))
                {
                    return RedirectToAction("EvaluarActividades", "Docente", new { actividadId = actividadId, materiaId = actividad.MateriaId });
                }

                // Si es alumno, llevar a su detalle de actividad
                if (User != null && User.IsInRole("Alumno"))
                {
                    return RedirectToAction("ActividadDetalle", "Alumno", new { actividadId = actividadId });
                }

                // Por defecto, redirigir al index de la aplicación
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }


        // Método para obtener la lista de alumnos que están dentro de la materia > se guardan en array para despues comparar.-HAcer busqueda mas eficiente
        [HttpGet]
        public async Task<ActionResult> AlumnosParaCalificarActividades(int materiaId)
        {
            try
            {
                var alumnos = await Db.tbAlumnosMaterias
                    .Where(am => am.MateriaId == materiaId)
                    .Join(Db.tbAlumnos,
                        am => am.AlumnoId,
                        a => a.AlumnoId,
                        (am, a) => new
                        {
                            a.AlumnoId,
                            a.Nombre,
                            a.ApellidoPaterno,
                            a.ApellidoMaterno
                        })
                    .OrderBy(a => a.ApellidoPaterno)
                    .ThenBy(a => a.ApellidoMaterno)
                    .ThenBy(a => a.Nombre)
                    .ToListAsync();

                if (alumnos == null || !alumnos.Any())
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "No se encontraron alumnos para la materia especificada." }, JsonRequestBehavior.AllowGet);
                }

                return Json(alumnos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener los alumnos", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //[HttpPost]
        //public async Task<ActionResult> ObtenerActividadesParaEvaluar(EvaluacionRequest request)
        //{
        //    try
        //    {
        //        if (request == null || request.Alumnos == null || !request.Alumnos.Any() || request.ActividadId <= 0)
        //        {
        //            Response.StatusCode = 400; // Bad Request
        //            return Json(new { error = "Datos inválidos en la solicitud" }, JsonRequestBehavior.AllowGet);
        //        }

        //        // Extraer los ID de los alumnos
        //        var alumnoIds = request.Alumnos.Select(a => a.AlumnoId).ToList();

        //        // Obtener actividades de los alumnos para la actividad específica
        //        //var alumnosActividades = await Db.tbAlumnosActividades
        //        //    .Where(aa => alumnoIds.Contains(aa.AlumnoId) && aa.ActividadId == request.ActividadId)
        //        //    .Include(aa => aa.Alumnos)
        //        //    .ToListAsync();

        //        var alumnosEntregas = await Db.tbEntregaActividadAlumno.Where(a => alumnoIds.Contains(a.AlumnoId) && a.ActividadId == request.ActividadId)
        //            .Include(a=>a.tbAlumnos).ToListAsync();

        //        if (!alumnosEntregas.Any())
        //        {
        //            Response.StatusCode = 404; // Not Found
        //            return Json(new { error = $"No se encontraron registros para la actividadId {request.ActividadId}" }, JsonRequestBehavior.AllowGet);
        //        }

        //        // Separar en no entregados
        //        var noEntregados = alumnosActividades
        //            .Where(aa => !aa.EstatusEntrega)
        //            .Select(aa => new
        //            {
        //                aa.AlumnoActividadId,
        //                aa.Alumnos.AlumnoId,
        //                aa.Alumnos.Nombre,
        //                aa.Alunos.ApellidoPaterno,
        //                aa.Alunos.ApellidoMaterno
        //            })
        //            .ToList();

        //        // Obtener entregas con datos de alumnos

        //        var entregadosIds = alumnosActividades
        //            .Where(aa => aa.EstatusEntrega)
        //            .Select(aa => aa.AlumnoActividadId)
        //            .ToList();

        //        var entregados = await Db.tbEntregablesAlumno
        //            .Where(ea => entregadosIds.Contains(ea.AlumnoActividadId))
        //            .ToListAsync();

        //        var entregadosFormato = entregados
        //            .Select(ea => new
        //            {
        //                AlumnoActividad = alumnosActividades.FirstOrDefault(aa => aa.AlumnoActividadId == ea.AlumnoActividadId),
        //                Entrega = new
        //                {
        //                    ea.EntregaId,
        //                    ea.AlumnoActividadId,
        //                    ea.Respuesta
        //                }
        //            })
        //            .Select(e => new
        //            {
        //                e.AlumnoActividad.AlumnoActividadId,
        //                FechaEntrega = e.AlumnoActividad.FechaEntrega,
        //                EstatusEntrega = true,
        //                e.AlumnoActividad.Alumnos.AlumnoId,
        //                e.AlumnoActividad.Alumnos.Nombre,
        //                e.AlumnoActividad.Alumnos.ApellidoPaterno,
        //                e.AlumnoActividad.Alumnos.ApellidoMaterno,
        //                Entrega = e.Entrega
        //            })
        //            .ToList();

        //        // Retornar resultado en formato JSON
        //        return Json(new
        //        {
        //            //NoEntregados = noEntregados,
        //            Entregados = entregadosFormato
        //        }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.StatusCode = 500; // Internal Server Error
        //        return Json(new { mensaje = "Error al obtener las actividades", error = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        //Si un alumno es agregado a la materia 
        //[HttpPost]
        //public async Task<ActionResult> AsignarActividadesPendientes(int alumnoId)
        //{
        //    try
        //    {
        //        // Verificar si el alumno existe
        //        var alumnoExiste = await Db.tbAlumnos.AnyAsync(a => a.AlumnoId == alumnoId);
        //        if (!alumnoExiste)
        //        {
        //            Response.StatusCode = 400; // Bad Request
        //            return Json(new { mensaje = "El alumno no existe." }, JsonRequestBehavior.AllowGet);
        //        }

        //        // Obtener la materia del alumno
        //        var materiasAlumno = await Db.tbAlumnosMaterias
        //            .Where(am => am.AlumnoId == alumnoId)
        //            .Select(am => am.MateriaId)
        //            .ToListAsync();

        //        if (!materiasAlumno.Any())
        //        {
        //            Response.StatusCode = 400; // Bad Request
        //            return Json(new { mensaje = "El alumno no está inscrito en ninguna materia." }, JsonRequestBehavior.AllowGet);
        //        }

        //        // Buscar actividades que no tiene asignadas en esas materias
        //        var actividadesPendientes = await Db.tbActividades
        //            .Where(a => materiasAlumno.Contains(a.MateriaId) &&
        //                        !Db.tbAlumnosActividades.Any(aa => aa.AlumnoId == alumnoId && aa.ActividadId == a.ActividadId))
        //            .ToListAsync();

        //        // Asignar cada actividad pendiente al alumno
        //        foreach (var actividad in actividadesPendientes)
        //        {
        //            var nuevaRelacion = new tbAlumnosActividades
        //            {
        //                ActividadId = actividad.ActividadId,
        //                AlumnoId = alumnoId,
        //                FechaEntrega = DateTime.Now, // Se actualiza cuando entregue
        //                EstatusEntrega = false
        //            };

        //            Db.tbAlumnosActividades.Add(nuevaRelacion);
        //        }

        //        await Db.SaveChangesAsync();

        //        return Json(new { mensaje = "Actividades asignadas al nuevo alumno." }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.StatusCode = 500; // Internal Server Error
        //        return Json(new { mensaje = "Error al asignar actividades.", error = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}




        // Controlador para registrar o actualizar una calificación
        //[HttpPost]
        //public async Task<ActionResult> RegistrarCalificacion(CalificacionDto calificacionDto)
        //{
        //    if (calificacionDto == null)
        //    {
        //        Response.StatusCode = 400; // Bad Request
        //        return Json(new { mensaje = "Datos inválidos." }, JsonRequestBehavior.AllowGet);
        //    }

        //    // Verificar si la entrega existe
        //    var entregaExiste = await Db.tbEntregablesAlumno.AnyAsync(e => e.EntregaId == calificacionDto.EntregaId);
        //    if (!entregaExiste)
        //    {
        //        Response.StatusCode = 400; // Bad Request
        //        return Json(new { mensaje = "La entrega especificada no existe." }, JsonRequestBehavior.AllowGet);
        //    }

        //    try
        //    {
        //        // Buscar si ya existe una calificación para esta entrega
        //        var calificacionExistente = await Db.tbCalificaciones
        //            .FirstOrDefaultAsync(c => c.EntregaId == calificacionDto.EntregaId);

        //        if (calificacionExistente != null)
        //        {
        //            // Actualizar calificación existente
        //            calificacionExistente.Calificacion = calificacionDto.Calificacion;
        //            calificacionExistente.Comentarios = calificacionDto.Comentario;
        //            calificacionExistente.FechaCalificacionAsignada = DateTime.Now;

        //            // En EF6 no se necesita Update(), solo modificar propiedades y guardar
        //        }
        //        else
        //        {
        //            // Crear nueva calificación
        //            var nuevaCalificacion = new tbCalificaciones
        //            {
        //                EntregaId = calificacionDto.EntregaId,
        //                FechaCalificacionAsignada = DateTime.Now,
        //                Comentarios = calificacionDto.Comentario,
        //                Calificacion = calificacionDto.Calificacion
        //            };

        //            Db.tbCalificaciones.Add(nuevaCalificacion);
        //        }

        //        await Db.SaveChangesAsync();

        //        return Json(new { mensaje = "Calificación guardada correctamente." }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.StatusCode = 500; // Internal Server Error
        //        return Json(new { mensaje = "Error al registrar la calificación.", error = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        // Endpoint to return tipos de actividades for populating select in modal
        [HttpGet]
        public ActionResult ObtenerTiposActividades()
        {
            try
            {
                // Utilizar helper para devolver los tipos disponibles en BD o los valores por defecto del enum
                var tiposDict = EnumHelpers.ObtenerTiposActividad(Db);
                var tipos = tiposDict.Select(kv => new { TipoActividadId = kv.Key, Nombre = kv.Value }).ToList();
                return Json(tipos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al obtener tipos de actividades", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Nuevo: actualizar actividad (compatible con fetch PUT desde JS)
        [HttpPut]
        public async Task<ActionResult> ActualizarActividad(int id, ActividadDTO model)
        {
            if (model == null)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "Datos inválidos." }, JsonRequestBehavior.AllowGet);
            }

            try
            {

                var actividadActualizada = await ActividadesService.ActualizarActividad(id, model);

                return Json(new { mensaje = "Actividad actualizada correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch(KeyNotFoundException ex)
            {
                Response.StatusCode = 404; // Not Found
                return Json(new { mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al actualizar la actividad.", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Nuevo: eliminar actividad (compatible with fetch DELETE desde JS)
        [HttpDelete]
        public async Task<ActionResult> EliminarActividad(int id)
        {
            try
            {
                await ActividadesService.EliminarActividad(id);
            
                return Json(new 
                { 
                    mensaje = "Actividad eliminada correctamente." 
                }, JsonRequestBehavior.AllowGet);
            
            }
            catch (KeyNotFoundException ex)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return Json(new { mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (InvalidOperationException ex)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return Json(new { mensaje = "Error al eliminar la actividad.", error = ex.Message },
                    JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /Actividades/EnviarEntrega (recibe multipart/form-data desde la web)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnviarEntrega()
        {
        try
        {
        var req = System.Web.HttpContext.Current.Request;
        int actividadId =0;
        int alumnoId =0;
        DateTime fechaEntrega = DateTime.Now;
        string respuestaRaw = null;

        int.TryParse(req.Form["ActividadId"], out actividadId);
        int.TryParse(req.Form["AlumnoId"], out alumnoId);
        DateTime.TryParse(req.Form["FechaEntrega"], out fechaEntrega);
        respuestaRaw = req.Form["Respuesta"];

        if (actividadId <=0 || alumnoId <=0)
        {
        Response.StatusCode = (int)HttpStatusCode.BadRequest;
        return Json(new { mensaje = "Actividad o alumno inválido." });
        }

        var actividad = await Db.tbActividades.FindAsync(actividadId);
        if (actividad == null)
        {
        Response.StatusCode = (int)HttpStatusCode.NotFound;
        return Json(new { mensaje = "Actividad no encontrada." });
        }

        // Verificar que el alumno pertenezca a la materia o grupo
        var pertenece = await Db.tbAlumnosMaterias.AnyAsync(am => am.AlumnoId == alumnoId && am.MateriaId == actividad.MateriaId);
        if (!pertenece)
        {
        // también comprobar por grupo (si aplica)
        var gruposIds = await Db.tbGruposMaterias.Where(gm => gm.MateriaId == actividad.MateriaId).Select(gm => gm.GrupoId).ToListAsync();
        if (!(gruposIds != null && gruposIds.Count >0 && await Db.tbAlumnosGrupos.AnyAsync(ag => ag.AlumnoId == alumnoId && gruposIds.Contains(ag.GrupoId))))
        {
        Response.StatusCode = (int)HttpStatusCode.Forbidden;
        return Json(new { mensaje = "No tienes permiso para entregar esta actividad." });
        }
        }

        // Obtener o crear registro de entrega del alumno
        var entrega = await Db.tbEntregaActividadAlumno.FirstOrDefaultAsync(e => e.ActividadId == actividadId && e.AlumnoId == alumnoId);
        if (entrega == null)
        {
        entrega = new tbEntregaActividadAlumno
        {
        ActividadId = actividadId,
        AlumnoId = alumnoId,
        FechaEntrega = fechaEntrega,
        EstadoEntregaId =1
        };
        Db.tbEntregaActividadAlumno.Add(entrega);
        await Db.SaveChangesAsync();
        }

        // No permitir reenvío si ya fue calificada
        if (entrega.Calificacion != null)
        {
        Response.StatusCode = (int)HttpStatusCode.Conflict;
        return Json(new { mensaje = "No puedes volver a entregar porque la entrega ya fue calificada." });
        }

        // Verificar límite de entregas por alumno
        if (actividad.LimiteEntregasPorAlumno >0)
        {
        var existentes = await Db.tbEntregables.CountAsync(t => t.EntregaActividadAlumnoId == entrega.EntregaActividadAlumnoId);
        if (existentes >= actividad.LimiteEntregasPorAlumno)
        {
        Response.StatusCode = (int)HttpStatusCode.Conflict;
        return Json(new { mensaje = $"Has alcanzado el límite de {actividad.LimiteEntregasPorAlumno} envíos para esta actividad." });
        }
        }

        // Procesar archivos subidos y construir lista de URLs
        var archivosUrls = new List<string>();
        try
        {
        if (req.Files != null && req.Files.Count >0)
        {
        var uploadRoot = Server.MapPath($"~/Uploads/Actividades/{actividadId}/{alumnoId}");
        if (!System.IO.Directory.Exists(uploadRoot)) System.IO.Directory.CreateDirectory(uploadRoot);
        for (int i =0; i < req.Files.Count; i++)
        {
        var f = req.Files[i];
        var safeName = System.IO.Path.GetFileName(f.FileName);
        var savePath = System.IO.Path.Combine(uploadRoot, DateTime.Now.Ticks + "_" + safeName);
        f.SaveAs(savePath);
        var publicUrl = Url.Content($"~/Uploads/Actividades/{actividadId}/{alumnoId}/" + System.IO.Path.GetFileName(savePath));
        archivosUrls.Add(publicUrl);
        }
        }
        }
        catch (Exception ex)
        {
        // no bloquear la entrega por errores menores en archivos
        System.Diagnostics.Trace.WriteLine("Error guardando archivos: " + ex.Message);
        }

        // Crear registro tbEntregables con contenido JSON
        var contenidoObj = new
        {
        Respuesta = (respuestaRaw != null && respuestaRaw.StartsWith("{") ? (object)JsonConvert.DeserializeObject(respuestaRaw) : (object)new { Respuesta = respuestaRaw }),
        Archivos = archivosUrls,
        fechaEntrega = DateTime.UtcNow,
        totalArchivos = archivosUrls.Count
        };

        var ent = new tbEntregables
        {
        EntregaActividadAlumnoId = entrega.EntregaActividadAlumnoId,
        TipoEntregaId =1, // texto/archivo
        Contenido = JsonConvert.SerializeObject(contenidoObj)
        };
        Db.tbEntregables.Add(ent);
        await Db.SaveChangesAsync();

        return Json(new { mensaje = "Entrega registrada correctamente." });
        }
        catch (Exception ex)
        {
        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return Json(new { mensaje = "Ocurrió un error al guardar la entrega. Intenta nuevamente más tarde." });
        }
        }

        // POST: /Actividades/QuitarCalificacion
        [HttpPost]
        public async Task<ActionResult> QuitarCalificacion(int entregableId)
        {
        try
        {
        // buscar el entregable y su entrega principal
        var entregable = await Db.tbEntregables.FirstOrDefaultAsync(e => e.EntregableId == entregableId);
        if (entregable == null)
        {
        Response.StatusCode = (int)HttpStatusCode.NotFound;
        return Json(new { mensaje = "Entrega no encontrada." });
        }

        var entregaAlumno = await Db.tbEntregaActividadAlumno.FirstOrDefaultAsync(e => e.EntregaActividadAlumnoId == entregable.EntregaActividadAlumnoId);
        if (entregaAlumno == null)
        {
        Response.StatusCode = (int)HttpStatusCode.NotFound;
        return Json(new { mensaje = "Registro de entrega no encontrado." });
        }

        entregaAlumno.Calificacion = null;
        entregaAlumno.FechaCalificado = null;
        Db.Entry(entregaAlumno).State = EntityState.Modified;
        await Db.SaveChangesAsync();

        return Json(new { mensaje = "Calificación removida." });
        }
        catch (Exception ex)
        {
        Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return Json(new { mensaje = "No se pudo quitar la calificación." });
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
