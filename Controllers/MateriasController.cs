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
using System;
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

namespace ControlMaterias.Controllers
{
    public class MateriasController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private NotificacionesService _notifServ;

        public MateriasController()
        {
        }

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

                enlace.Estatus = Estatus;
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Estatus actualizado." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al actualizar estatus.", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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


        [HttpGet]
        public async Task<ActionResult> ObtenerDetallesMateria(int materiaId, int docenteId)
        {
            try
            {
                // Consulta la base de datos usando Entity Framework para obtener los detalles de la materia
                var materiaDetalles = await Db.tbMaterias
                    .Where(m => m.MateriaId == materiaId && m.DocenteId == docenteId)
                    .Select(m => new
                    {
                        NombreMateria = m.NombreMateria,
                        CodigoAcceso = m.CodigoAcceso,
                        CodigoColor = m.CodigoColor,
                        DocenteId = m.DocenteId
                    })
                    .FirstOrDefaultAsync();

                // Verifica si no se encontraron detalles de la materia
                if (materiaDetalles == null)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "Materia No Encontrada O Sin Permiso" }, JsonRequestBehavior.AllowGet);
                }

                return Json(materiaDetalles, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener los detalles de la materia", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



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

                var usuarios = await Db.Users
                    .Where(u => u.Email.Contains(query))
                    .Select(u => new { u.Id, u.Email })
                    .ToListAsync();

                var usuarioIds = usuarios.Select(u => u.Id).ToList();

                var alumnosConCorreo = await Db.tbAlumnos
                    .Where(a => a.Nombre.Contains(query) ||
                                a.ApellidoPaterno.Contains(query) ||
                                a.ApellidoMaterno.Contains(query) ||
                                usuarioIds.Contains(a.UserId)) 
                    .Select(a => new
                    {
                        a.IdentityUser.Email,
                        a.Nombre,
                        a.ApellidoPaterno,
                        a.ApellidoMaterno
                    })
                    .ToListAsync();

                return Json(alumnosConCorreo, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al buscar alumnos", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        //controlador para unir materia con alumno

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

                // Buscar el alumno por correo
                var alumno = await Db.tbAlumnos
                    .Where(a => a.IdentityUser.Email == correo)
                    .Select(a => new { a.AlumnoId })
                    .FirstOrDefaultAsync();

                if (alumno == null)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "Alumno no encontrado con el correo proporcionado." }, JsonRequestBehavior.AllowGet);
                }

                // Verificar si ya existe la relación en la tabla alumnosMaterias
                var relacionExistente = await Db.tbAlumnosMaterias
                    .Where(am => am.AlumnoId == alumno.AlumnoId && am.MateriaId == materiaId)
                    .FirstOrDefaultAsync();

                if (relacionExistente != null)
                {
                    Response.StatusCode = 400; // Bad Request
                    return Json(new { mensaje = "El alumno ya está asignado a esta materia." }, JsonRequestBehavior.AllowGet);
                }

                // Agregar nueva relación
                var nuevaRelacion = new tbAlumnosMaterias
                {
                    AlumnoId = alumno.AlumnoId,
                    MateriaId = materiaId
                };

                Db.tbAlumnosMaterias.Add(nuevaRelacion);
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Alumno asignado a la materia exitosamente." }, JsonRequestBehavior.AllowGet);
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
                            Estatus = x.a.Estatus ?? "Activo"
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



        // Controlador api que crea actividades y asigna a los alumnos
        [HttpPost]
        public async Task<ActionResult> CrearActividad(tbActividades actividadDto)
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

            // Verificar que la materia exista en la base de datos
            var materiaExiste = await Db.tbMaterias.AnyAsync(m => m.MateriaId == actividadDto.MateriaId);
            if (!materiaExiste)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "La materia especificada no existe." }, JsonRequestBehavior.AllowGet);
            }

            // Verificar que el tipo de actividad exista en la base de datos
            var tipoActividadExiste = await Db.cTiposActividades.AnyAsync(t => t.TipoActividadId == actividadDto.TipoActividadId);
            if (!tipoActividadExiste)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "El tipo de actividad especificado no existe." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // Crear la nueva actividad
                var nuevaActividad = new tbActividades
                {
                    NombreActividad = actividadDto.NombreActividad,
                    Descripcion = actividadDto.Descripcion,
                    FechaCreacion = DateTime.Now,
                    FechaLimite = actividadDto.FechaLimite,
                    TipoActividadId = actividadDto.TipoActividadId,
                    Puntaje = actividadDto.Puntaje,
                    MateriaId = actividadDto.MateriaId
                };

                Db.tbActividades.Add(nuevaActividad);
                await Db.SaveChangesAsync(); // Guarda la actividad y genera el ID

                // Obtener los alumnos que pertenecen a la materia
                var alumnosMateria = await Db.tbAlumnosMaterias
                    .Where(am => am.MateriaId == actividadDto.MateriaId)
                    .Select(am => am.AlumnoId)
                    .ToListAsync();

                // Crear registros en la tabla AlumnoActividad para cada alumno
                foreach (var alumnoId in alumnosMateria)
                {
                    var alumnoActividad = new tbAlumnosActividades
                    {
                        ActividadId = nuevaActividad.ActividadId,
                        AlumnoId = alumnoId,
                        FechaEntrega = DateTime.Now, // Inicialmente la fecha de creación
                        EstatusEntrega = false
                    };

                    Db.tbAlumnosActividades.Add(alumnoActividad);
                }

                // Guardar los cambios en la tabla AlumnoActividad
                await Db.SaveChangesAsync();

                //Envío de notificación a los alumnos dentro de la materia
                await Ns.NotificacionCrearActividad(
                    nuevaActividad,
                    nuevaActividad.MateriaId
                    );

                return Json(new { mensaje = "Actividad creada y asignada a los alumnos con éxito", actividadId = nuevaActividad.ActividadId }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al crear la actividad", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        //Controlador que obtiene  todo lo de actividades que pertecenen a esa materia
        [HttpGet]
        public async Task<ActionResult> ObtenerActividadesPorMateria(int materiaId)
        {
            try
            {
                // Load activities into memory first to avoid EF translation issues with DateTime.ToString(format)
                var actividadesEntities = await Db.tbActividades
                    .Where(a => a.MateriaId == materiaId)
                    .ToListAsync();

                if (actividadesEntities == null || actividadesEntities.Count == 0)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "No hay actividades registradas para esta materia." }, JsonRequestBehavior.AllowGet);
                }

                var resultado = actividadesEntities.Select(a => new
                {
                    a.ActividadId,
                    a.NombreActividad,
                    a.Descripcion,
                    FechaCreacion = a.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),
                    FechaLimite = a.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
                    a.Puntaje
                }).ToList();

                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener las actividades", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpDelete]
        [Route("api/Actividades/EliminarActividad/{id}")]
        public async Task<ActionResult> EliminarActividad(int id)
        {
            try
            {
                // Buscar el registro en la tabla tbActividades
                var actividad = await Db.tbActividades
                    .FirstOrDefaultAsync(a => a.ActividadId == id);

                if (actividad == null)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "No se encontró el registro en Actividades." }, JsonRequestBehavior.AllowGet);
                }

                // Eliminar el registro
                Db.tbActividades.Remove(actividad);
                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Actividad eliminada correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al eliminar la actividad.", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        //Controlador para crear un aviso funciona desde dentro de la materia
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CrearAviso(tbAvisos avisos)
        {
            if (avisos == null)
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { mensaje = "Datos inválidos." }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                var nuevoAviso = new tbAvisos
                {
                    DocenteId = avisos.DocenteId,
                    Titulo = avisos.Titulo,
                    Descripcion = avisos.Descripcion,
                    GrupoId = avisos.GrupoId == 0 ? null : avisos.GrupoId,
                    MateriaId = avisos.MateriaId,
                    FechaCreacion = DateTime.Now
                };

                Db.tbAvisos.Add(nuevoAviso);
                await Db.SaveChangesAsync();

                await Ns.NotificacionCrearAviso(
                    nuevoAviso,
                    nuevoAviso.GrupoId,
                    nuevoAviso.MateriaId
                );

                return Json(new { mensaje = "Aviso creado con éxito" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al crear el aviso", error = ex.Message }, JsonRequestBehavior.AllowGet);
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
                var avisosDb = await Db.tbAvisos
                    .Where(a => a.MateriaId == IdMateria)
                    .ToListAsync();

                var avisos = avisosDb.Select(a => new
                {
                    a.AvisoId,
                    a.Titulo,
                    a.Descripcion,
                    FechaCreacion = a.FechaCreacion.ToString("dddd, d 'de' MMMM 'de' yyyy HH:mm:ss")
                });
                return Json(avisos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener los avisos", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        //Controlador para obtener informacion de un aviso para despeus editar
        [HttpGet]
        public async Task<ActionResult> ObtenerAvisoPorId(int avisoId)
        {
            try
            {
                var aviso = await Db.tbAvisos
                    .Where(a => a.AvisoId == avisoId)
                    .Select(a => new
                    {
                        a.AvisoId,
                        a.Titulo,
                        a.Descripcion
                    })
                    .FirstOrDefaultAsync();

                if (aviso == null)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "Aviso no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                return Json(aviso, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al obtener el aviso", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        //Editar aviso
        [HttpPut]
        public async Task<ActionResult> EditarAviso(tbAvisos model)
        {
            try
            {
                var aviso = await Db.tbAvisos.FindAsync(model.AvisoId);
                if (aviso == null)
                {
                    Response.StatusCode = 404; // Not Found
                    return Json(new { mensaje = "Aviso no encontrado" }, JsonRequestBehavior.AllowGet);
                }

                aviso.Titulo = model.Titulo;
                aviso.Descripcion = model.Descripcion;

                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Aviso actualizado correctamente" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500; // Internal Server Error
                return Json(new { mensaje = "Error al actualizar el aviso", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


       
        [HttpPost]
        public async Task<ActionResult> CrearMateria(tbMaterias materia)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { error = "Datos de materia inválidos." }, JsonRequestBehavior.AllowGet);
            }

            materia.CodigoAcceso = ObtenerClaveMateria();
            Db.tbMaterias.Add(materia);
            await Db.SaveChangesAsync();

            return Json(new
            {
                mensaje = "Materia creada con éxito.",
                materiaId = materia.MateriaId
            }, JsonRequestBehavior.AllowGet);
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

        [HttpGet]
        public async Task<ActionResult> ObtenerMateriasSinGrupo(int docenteId)
        {
            try
            {
                var materiasSinGrupo = await Db.tbMaterias
                    .Where(m => m.DocenteId == docenteId &&
                        !Db.tbGruposMaterias.Any(gm => gm.MateriaId == m.MateriaId))
                    .ToListAsync();

                var resultado = new List<object>();

                foreach (var materia in materiasSinGrupo)
                {
                    var actividadesRecientes = await Db.tbActividades
                        .Where(a => a.MateriaId == materia.MateriaId)
                        .OrderByDescending(a => a.FechaCreacion)
                        .Take(2)
                        .Select(a => new
                        {
                            a.ActividadId,
                            a.NombreActividad,
                            a.FechaCreacion
                        })
                        .ToListAsync();

                    resultado.Add(new
                    {
                        materia.MateriaId,
                        materia.NombreMateria,
                        materia.Descripcion,
                        materia.DocenteId,
                        DocenteNombre = Db.tbDocentes.Where(d => d.DocenteId == materia.DocenteId).Select(d => d.Nombre + " " + d.ApellidoPaterno + " " + d.ApellidoMaterno).FirstOrDefault(),
                        materia.CodigoColor,
                        materia.CodigoAcceso,
                        // materia.DocenteId already included above
                        ActividadesRecientes = actividadesRecientes
                    });
                }

                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al obtener las materias", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
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

                var actividades = Db.tbActividades.Where(a => a.MateriaId == id);
                Db.tbActividades.RemoveRange(actividades);

                var avisos = Db.tbAvisos.Where(a => a.MateriaId == id);
                Db.tbAvisos.RemoveRange(avisos);

                var relacionesAlumnos = Db.tbAlumnosMaterias.Where(am => am.MateriaId == id);
                Db.tbAlumnosMaterias.RemoveRange(relacionesAlumnos);

                var relacionMateriaConGrupo = Db.tbGruposMaterias.Where(mg => mg.MateriaId == id);
                Db.tbGruposMaterias.RemoveRange(relacionMateriaConGrupo);

                Db.tbMaterias.Remove(materia);

                await Db.SaveChangesAsync();

                return Json(new { mensaje = "Materia y sus relaciones eliminadas correctamente." }, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al eliminar la materia", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public async Task<ActionResult> ActualizarMateria(int materiaId, tbMaterias materiaDto)
        {
            try
            {
                var materiaExistente = await Db.tbMaterias.FindAsync(materiaId);
                if (materiaExistente == null)
                {
                    Response.StatusCode = 404;
                    return Json(new { mensaje = "Materia no encontrada." }, JsonRequestBehavior.AllowGet);
                }

                if (string.IsNullOrWhiteSpace(materiaDto.NombreMateria))
                {
                    Response.StatusCode = 400;
                    return Json(new { mensaje = "El nombre de la materia no puede estar vacío." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    materiaExistente.NombreMateria = materiaDto.NombreMateria;
                }
                
                materiaExistente.Descripcion = string.IsNullOrWhiteSpace(materiaDto.Descripcion)
                    ? null
                :materiaDto.Descripcion;


                await Db.SaveChangesAsync();
                if (materiaDto == null)
                {
                    return Json(new { mensaje = "El objeto materiaDto llegó nulo." }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    materiaExistente.MateriaId,
                    materiaExistente.NombreMateria,
                    materiaExistente.Descripcion
                }, JsonRequestBehavior.AllowGet);

                //return Json(materiaExistente, JsonRequestBehavior.AllowGet);
            }
            catch (System.Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al actualizar la materia", error = ex.Message }, JsonRequestBehavior.AllowGet);
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
