﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;


namespace ControlActividades.Controllers
{
    public class ActividadesController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;

        public ActividadesController()
        {
        }



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



        //Controlador para obtener los datos de una actividad
        [HttpGet]
        public async Task<ActionResult> ObtenerActividadPorId(int actividadId)
        {
            try
            {
                var actividad = await Db.tbActividades
                    .Where(a => a.ActividadId == actividadId)
                    .Select(a => new
                    {
                        a.ActividadId,
                        a.NombreActividad,
                        a.Descripcion,
                        a.FechaCreacion,
                        a.FechaLimite,
                        a.Puntaje
                    })
                    .FirstOrDefaultAsync();

                if (actividad == null)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "No se encontró la actividad con el ID especificado." }, JsonRequestBehavior.AllowGet);
                }

                return Json(actividad, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener la actividad", error = ex.Message }, JsonRequestBehavior.AllowGet);
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


        [HttpPost]
        public async Task<ActionResult> ObtenerActividadesParaEvaluar(EvaluacionRequest request)
        {
            try
            {
                if (request == null || request.Alumnos == null || !request.Alumnos.Any() || request.ActividadId <= 0)
                {
                    Response.StatusCode = 400; // Bad Request
                    return Json(new { error = "Datos inválidos en la solicitud" }, JsonRequestBehavior.AllowGet);
                }

                // Extraer los ID de los alumnos
                var alumnoIds = request.Alumnos.Select(a => a.AlumnoId).ToList();

                // Obtener actividades de los alumnos para la actividad específica
                var alumnosActividades = await Db.tbAlumnosActividades
                    .Where(aa => alumnoIds.Contains(aa.AlumnoId) && aa.ActividadId == request.ActividadId)
                    .Include(aa => aa.Alumnos) // Incluir datos del alumno directamente
                    .ToListAsync();

                if (!alumnosActividades.Any())
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { error = $"No se encontraron registros para la actividadId {request.ActividadId}" }, JsonRequestBehavior.AllowGet);
                }

                // Separar en no entregados
                var noEntregados = alumnosActividades
                    .Where(aa => !aa.EstatusEntrega)
                    .Select(aa => new
                    {
                        aa.AlumnoActividadId,
                        aa.Alumnos.AlumnoId,
                        aa.Alumnos.Nombre,
                        aa.Alumnos.ApellidoPaterno,
                        aa.Alumnos.ApellidoMaterno
                    })
                    .ToList();

                // Obtener entregas con datos de alumnos
                var entregadosIds = alumnosActividades
                    .Where(aa => aa.EstatusEntrega)
                    .Select(aa => aa.AlumnoActividadId)
                    .ToList();

                var entregados = await Db.tbEntregablesAlumno
                    .Where(ea => entregadosIds.Contains(ea.AlumnoActividadId))
                    .ToListAsync();

                var entregadosFormato = entregados
                    .Select(ea => new
                    {
                        AlumnoActividad = alumnosActividades.FirstOrDefault(aa => aa.AlumnoActividadId == ea.AlumnoActividadId),
                        Entrega = new
                        {
                            ea.EntregaId,
                            ea.AlumnoActividadId,
                            ea.Respuesta
                        }
                    })
                    .Select(e => new
                    {
                        e.AlumnoActividad.AlumnoActividadId,
                        FechaEntrega = e.AlumnoActividad.FechaEntrega,
                        EstatusEntrega = true,
                        e.AlumnoActividad.Alumnos.AlumnoId,
                        e.AlumnoActividad.Alumnos.Nombre,
                        e.AlumnoActividad.Alumnos.ApellidoPaterno,
                        e.AlumnoActividad.Alumnos.ApellidoMaterno,
                        Entrega = e.Entrega
                    })
                    .ToList();

                // Retornar resultado en formato JSON
                return Json(new
                {
                    NoEntregados = noEntregados,
                    Entregados = entregadosFormato
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener las actividades", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        //Si un alumno es agregado a la materia 
        [HttpPost]
        public async Task<ActionResult> AsignarActividadesPendientes(int alumnoId)
        {
            try
            {
                // Verificar si el alumno existe
                var alumnoExiste = await Db.tbAlumnos.AnyAsync(a => a.AlumnoId == alumnoId);
                if (!alumnoExiste)
                {
                    Response.StatusCode = 400; // Bad Request
                    return Json(new { mensaje = "El alumno no existe." }, JsonRequestBehavior.AllowGet);
                }

                // Obtener la materia del alumno
                var materiasAlumno = await Db.tbAlumnosMaterias
                    .Where(am => am.AlumnoId == alumnoId)
                    .Select(am => am.MateriaId)
                    .ToListAsync();

                if (!materiasAlumno.Any())
                {
                    Response.StatusCode = 400; // Bad Request
                    return Json(new { mensaje = "El alumno no está inscrito en ninguna materia." }, JsonRequestBehavior.AllowGet);
                }

                // Buscar actividades que no tiene asignadas en esas materias
                var actividadesPendientes = await Db.tbActividades
                    .Where(a => materiasAlumno.Contains(a.MateriaId) &&
                                !Db.tbAlumnosActividades.Any(aa => aa.AlumnoId == alumnoId && aa.ActividadId == a.ActividadId))
                    .ToListAsync();

                // Asignar cada actividad pendiente al alumno
                foreach (var actividad in actividadesPendientes)
                {
                    var nuevaRelacion = new tbAlumnosActividades
                    {
                        ActividadId = actividad.ActividadId,
                        AlumnoId = alumnoId,
                        FechaEntrega = DateTime.Now, // Se actualiza cuando entregue
                        EstatusEntrega = false
                    };

                    Db.tbAlumnosActividades.Add(nuevaRelacion);
                }

                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Actividades asignadas al nuevo alumno." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al asignar actividades.", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        // Controlador para registrar o actualizar una calificación
        [HttpPost]
        public async Task<ActionResult> RegistrarCalificacion(CalificacionDto calificacionDto)
        {
            if (calificacionDto == null)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "Datos inválidos." }, JsonRequestBehavior.AllowGet);
            }

            // Verificar si la entrega existe
            var entregaExiste = await Db.tbEntregablesAlumno.AnyAsync(e => e.EntregaId == calificacionDto.EntregaId);
            if (!entregaExiste)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "La entrega especificada no existe." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // Buscar si ya existe una calificación para esta entrega
                var calificacionExistente = await Db.tbCalificaciones
                    .FirstOrDefaultAsync(c => c.EntregaId == calificacionDto.EntregaId);

                if (calificacionExistente != null)
                {
                    // Actualizar calificación existente
                    calificacionExistente.Calificacion = calificacionDto.Calificacion;
                    calificacionExistente.Comentarios = calificacionDto.Comentario;
                    calificacionExistente.FechaCalificacionAsignada = DateTime.Now;

                    // En EF6 no se necesita Update(), solo modificar propiedades y guardar
                }
                else
                {
                    // Crear nueva calificación
                    var nuevaCalificacion = new tbCalificaciones
                    {
                        EntregaId = calificacionDto.EntregaId,
                        FechaCalificacionAsignada = DateTime.Now,
                        Comentarios = calificacionDto.Comentario,
                        Calificacion = calificacionDto.Calificacion
                    };

                    Db.tbCalificaciones.Add(nuevaCalificacion);
                }

                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Calificación guardada correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al registrar la calificación.", error = ex.Message }, JsonRequestBehavior.AllowGet);
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
