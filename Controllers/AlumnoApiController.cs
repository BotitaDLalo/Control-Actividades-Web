using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ControlActividades.Exceptions;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using ControlActividades.Services.Actividades;
using ControlActividades.Services.Alumno;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Google;
using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Owin;
using static ControlActividades.Controllers.AlumnoController;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/Alumnos")]
    public class AlumnoApiController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private NotificacionesService _notifServ;
        private AlumnoApiService _alumnoApiService;
        public AlumnoApiController() { }

        #region Propiedades
        public AlumnoApiController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext DbContext,
            FuncionalidadesGenerales fg,
            NotificacionesService notifServ
            )
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
            Ns = notifServ;
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

        public NotificacionesService Ns
        {
            get
            {
                return _notifServ ?? (_notifServ = new NotificacionesService(Db, new FCMService()));
            }
            private set
            {
                _notifServ = value;
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

        #endregion


        // Asegura que exista al menos un registro en cEstadoEntrega y devuelve un EstadoEntregaId válido
        //private async Task<int> ResolveEstadoEntregaIdAsync(int preferido)
        //{
        //    try
        //    {
        //        var existe = await Db.cEstadoEntrega.AnyAsync(e => e.EstadoEntregaId == preferido);
        //        if (existe) return preferido;

        //        var anyId = await Db.cEstadoEntrega.Select(e => (int?)e.EstadoEntregaId).FirstOrDefaultAsync();
        //        if (anyId.HasValue) return anyId.Value;

        //        // crear valores por defecto
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

        // Helper: verifica si existe una tabla en la base de datos (SQL Server)
        //private bool TableExists(string tableName)
        //{
        //    try
        //    {
        //        var sql = @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @p0";
        //        var count = Db.Database.SqlQuery<int>(sql, tableName).FirstOrDefault();
        //        return count > 0;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        // Verifica que el alumno pueda acceder/entregar la actividad:
        // - la actividad existe y está publicada (o programada y la fecha ya pasó)
        // - el alumno está inscrito en la materia OR pertenece a algún grupo que contiene la materia
        //private async Task<bool> AlumnoPuedeAccederActividadAsync(int alumnoId, int actividadId)
        //{
        //    var actividad = await Db.tbActividades.FindAsync(actividadId);
        //    if (actividad == null) return false;

        //    // Permitir entrega si la actividad está publicada, es borrador, o es programada y la fecha ya pasó.
        //    // (Se bloquean sólo las actividades programadas cuya fecha aún no llegó)
        //    bool bloqueadaPorProgramacion = actividad.Enviado == null && actividad.FechaProgramada.HasValue && actividad.FechaProgramada.Value > DateTime.Now;
        //    if (bloqueadaPorProgramacion) return false;

        //    int materiaId = actividad.MateriaId;

        //    // Si está inscrito en la materia
        //    var perteneceMateria = await Db.tbAlumnosMaterias.AnyAsync(am => am.AlumnoId == alumnoId && am.MateriaId == materiaId);
        //    if (perteneceMateria) return true;

        //    // Si pertenece a algún grupo que contiene la materia
        //    var gruposIds = await Db.tbGruposMaterias.Where(gm => gm.MateriaId == materiaId).Select(gm => gm.GrupoId).ToListAsync();
        //    if (gruposIds != null && gruposIds.Count > 0)
        //    {
        //        var perteneceGrupo = await Db.tbAlumnosGrupos.AnyAsync(ag => ag.AlumnoId == alumnoId && gruposIds.Contains(ag.GrupoId));
        //        if (perteneceGrupo) return true;
        //    }

        //    return false;
        //}


        //private string BuildRespuestaWithFiles(string respuesta, List<string> files)
        //{
        //    // Guardar texto y enlaces a archivos en un JSON sencillo
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

        // Intenta resolver el TipoEntregaId solicitado; si no existe en la tabla cTipoEntrega
        // devuelve el primer Id disponible o crea registros por defecto (1=TEXTO,2=ARCHIVO) cuando sea necesario.
        //private async Task<int> ResolveTipoEntregaIdAsync(int preferido)
        //{
        //    try
        //    {
        //        // Verificar existencia del preferido
        //        var existe = await Db.cTipoEntrega.AnyAsync(t => t.TipoActividadId == preferido);
        //        if (existe) return preferido;

        //        // Si no existe, intentar obtener cualquier id existente
        //        var anyId = await Db.cTipoEntrega.Select(t => (int?)t.TipoActividadId).FirstOrDefaultAsync();
        //        if (anyId.HasValue) return anyId.Value;

        //        // Si la tabla está vacía, insertar valores por defecto
        //        var texto = new cTipoEntrega { Nombre = "Texto" };
        //        var archivo = new cTipoEntrega { Nombre = "Archivo" };
        //        Db.cTipoEntrega.Add(texto);
        //        Db.cTipoEntrega.Add(archivo);
        //        await Db.SaveChangesAsync();

        //        // Intentar devolver el preferido si coincide con los que acabamos de crear
        //        if (preferido == 1) return texto.TipoActividadId;
        //        if (preferido == 2) return archivo.TipoActividadId;

        //        // Por defecto devolver el primero creado
        //        return texto.TipoActividadId;
        //    }
        //    catch
        //    {
        //        // En caso de error, devolver 1 como fallback razonable
        //        return preferido > 0 ? preferido : 1;
        //    }
        //}




        // Endpoint para unirse a una clase con código de acceso
        //[HttpPost]
        //[Route("UnirseAClase")]
        //public async Task<IHttpActionResult> UnirseAClase([FromBody] UnirseAClaseRequest request)
        //{
        //    if (string.IsNullOrEmpty(request.CodigoAcceso))
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { mensaje = "El código de acceso es obligatorio" });
        //    }

        //    // Buscar si el código pertenece a un Grupo
        //    var grupo = await Db.tbGrupos.FirstOrDefaultAsync(g => g.CodigoAcceso == request.CodigoAcceso);

        //    if (grupo != null)
        //    {
        //        // Verificar si el alumno ya está inscrito en el grupo
        //        var existeRelacion = await Db.tbAlumnosGrupos
        //            .AnyAsync(ag => ag.AlumnoId == request.AlumnoId && ag.GrupoId == grupo.GrupoId);

        //        if (!existeRelacion)
        //        {
        //            // Agregar el alumno al grupo
        //            var nuevaRelacion = new tbAlumnosGrupos
        //            {
        //                AlumnoId = request.AlumnoId,
        //                GrupoId = grupo.GrupoId
        //            };
        //            Db.tbAlumnosGrupos.Add(nuevaRelacion);
        //            await Db.SaveChangesAsync();
        //            return Ok(new { mensaje = "Te has unido al grupo", nombre = grupo.NombreGrupo, esGrupo = true });
        //        }

        //    }

        //    // Buscar si el código pertenece a una Materia
        //    var materia = await Db.tbMaterias.FirstOrDefaultAsync(m => m.CodigoAcceso == request.CodigoAcceso);

        //    if (materia != null)
        //    {
        //        // Verificar si el alumno ya está inscrito en la materia
        //        var existeRelacion = await Db.tbAlumnosMaterias
        //            .AnyAsync(am => am.AlumnoId == request.AlumnoId && am.MateriaId == materia.MateriaId);

        //        if (!existeRelacion)
        //        {
        //            // Agregar el alumno a la materia
        //            var nuevaRelacion = new tbAlumnosMaterias
        //            {
        //                AlumnoId = request.AlumnoId,
        //                MateriaId = materia.MateriaId
        //            };
        //            Db.tbAlumnosMaterias.Add(nuevaRelacion);
        //            await Db.SaveChangesAsync();
        //            return Ok(new { mensaje = "Te has unido a la materia", nombre = materia.NombreMateria, esGrupo = false });
        //        }

        //    }

        //    return Content(HttpStatusCode.NotFound, new { mensaje = "Código de acceso no válido" });
        //}

        /*
        [HttpPost]
        [Route("UnirseAClaseM")]
        public async Task<IHttpActionResult> UnirseAClaseM([FromBody] UnirseAClaseRequest request)
        {
            try
            {
                var codigo = request.CodigoAcceso;

                var grupo = await Db.tbGrupos.FirstOrDefaultAsync(g => g.CodigoAcceso == request.CodigoAcceso);

                if (grupo != null)
                {
                    int docenteId = grupo.DocenteId;
                    var docente = await Db.tbDocentes.Where(a => a.DocenteId == docenteId).FirstOrDefaultAsync();

                    if (docente == null) return BadRequest();

                    var existeRelacion = await Db.tbAlumnosGrupos
                        .AnyAsync(ag => ag.AlumnoId == request.AlumnoId && ag.GrupoId == grupo.GrupoId);

                    if (!existeRelacion)
                    {
                        var lsMateriasId = await Db.tbGruposMaterias.Where(a => a.GrupoId == grupo.GrupoId).Select(a => a.MateriaId).ToListAsync();

                        var lsMaterias = await Db.tbMaterias.Where(a => lsMateriasId.Contains(a.MateriaId)).Select(m => new MateriaRes
                        {
                            MateriaId = m.MateriaId,
                            NombreMateria = m.NombreMateria,
                            Descripcion = m.Descripcion,
                            //m.CodigoColor,
                            Actividades = Db.tbActividades.Where(a => a.MateriaId == m.MateriaId).ToList()
                        }).ToListAsync();


                        GrupoRes grupoRes = new GrupoRes()
                        {
                            GrupoId = grupo.GrupoId,
                            NombreGrupo = grupo.NombreGrupo,
                            Descripcion = grupo.Descripcion,
                            CodigoAcceso = grupo.CodigoAcceso,
                            CodigoColor = grupo.CodigoColor,
                            Materias = lsMaterias
                        };

                        var nuevaRelacion = new tbAlumnosGrupos
                        {
                            AlumnoId = request.AlumnoId,
                            GrupoId = grupo.GrupoId
                        };
                        Db.tbAlumnosGrupos.Add(nuevaRelacion);
                        await Db.SaveChangesAsync();


                        UnirseAClaseMRespuesta respuesta = new UnirseAClaseMRespuesta()
                        {
                            Grupo = grupoRes,
                            EsGrupo = true
                        };


                        return Ok(respuesta);
                    }
                    return BadRequest();

                }

                var materia = await Db.tbMaterias.FirstOrDefaultAsync(m => m.CodigoAcceso == request.CodigoAcceso);

                if (materia != null)
                {
                    int docenteId = materia.DocenteId;
                    var docente = await Db.tbDocentes.Where(a => a.DocenteId == docenteId).FirstOrDefaultAsync();

                    if (docente == null) return BadRequest();
                    var existeRelacion = await Db.tbAlumnosMaterias
                         .AnyAsync(am => am.AlumnoId == request.AlumnoId && am.MateriaId == materia.MateriaId);

                    if (!existeRelacion)
                    {
                        MateriaRes materiaRes = new MateriaRes()
                        {
                            MateriaId = materia.MateriaId,
                            NombreMateria = materia.NombreMateria,
                            Descripcion = materia.Descripcion,
                            Actividades = await Db.tbActividades.Where(a => a.MateriaId == materia.MateriaId).ToListAsync()
                        };

                        var nuevaRelacion = new tbAlumnosMaterias
                        {
                            AlumnoId = request.AlumnoId,
                            MateriaId = materia.MateriaId
                        };
                        Db.tbAlumnosMaterias.Add(nuevaRelacion);
                        await Db.SaveChangesAsync();

                        UnirseAClaseMRespuesta respuesta = new UnirseAClaseMRespuesta()
                        {
                            Materia = materiaRes,
                            EsGrupo = false
                        };

                        return Ok(respuesta);
                    }
                    return BadRequest();
                }

                return Content(HttpStatusCode.NotFound, new { mensaje = "No existe la clase." });
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        */

        // Nuevo metodo para registrarse mediante codigo de clase
        [HttpPost]
        [Route("UnirseAClaseM")]
        public async Task<IHttpActionResult> UnirseAClaseM([FromBody] UnirseAClaseRequest request)
        {
            try
            {
                // 1. Validación del request
                if (request == null || string.IsNullOrEmpty(request.CodigoAcceso) || request.AlumnoId <= 0)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        mensaje = "Datos de solicitud inválidos: AlumnoId y CodigoAcceso son obligatorios."
                    });
                }

                // 2. Normalizar código a mayúsculas para comparación case-insensitive
                var codigoNormalizado = request.CodigoAcceso.Trim().ToUpper();


                var unirseClaseRes = await AlumnoApiService.UnirseAClase(request.AlumnoId, request.CodigoAcceso);

                return Ok(unirseClaseRes);

                // 16. Código no encontrado - ni en grupos ni en materias
                //return Content(HttpStatusCode.NotFound, new
                //{
                //    mensaje = "Código de acceso inválido o inexistente. Verifica que el código sea correcto."
                //});
            }
            catch (AlumnosException e)
            {
                var mensaje = e.Mensaje;
                return Content(HttpStatusCode.BadRequest, new
                {
                    mensaje = mensaje
                });
            }
            catch (Exception ex)
            {
                // 17. Logging del error
                // _logger.LogError(ex, "Error al unirse a clase para AlumnoId: {AlumnoId}, Codigo: {Codigo}", 
                //     request?.AlumnoId, request?.CodigoAcceso);

                // 18. Retornar error 500 con mensaje genérico
                return Content(HttpStatusCode.InternalServerError, new
                {
                    mensaje = "Error interno del servidor. Inténtalo de nuevo más tarde."
                });
            }
        }





        //[HttpPost]
        //[Route("RegistrarEnvioActividadAlumno")]
        //public async Task<IHttpActionResult> RegistrarEnvioActividadAlumno([FromBody] EntregableAlumno entregable)
        //{
        //    try
        //    {
        //        var actividadId = entregable.ActividadId;
        //        var alumnoId = entregable.AlumnoId;
        //        var respuesta = entregable.Respuesta;
        //        var fechaEntrega = entregable.FechaEntrega;
        //        var tipoEntregaId = entregable.TipoEntregaId;

        //        if (entregable.ActividadId <= 0 || entregable.AlumnoId <= 0)
        //        {
        //            return Content(HttpStatusCode.BadRequest, new
        //            {
        //                mensaje = "ActividadId y AlumnoId deben ser mayores a 0.",
        //                ActividadId = entregable.ActividadId,
        //                AlumnoId = entregable.AlumnoId
        //            });
        //        }

        //        // Verificar permisos y existencia de actividad
        //        if (!await AlumnoPuedeAccederActividadAsync(alumnoId, actividadId))
        //            return Content(HttpStatusCode.Forbidden, new { mensaje = "No tienes permiso para entregar esta actividad." });

        //        DateTime fechaEntParsed;
        //        if (!DateTime.TryParse(fechaEntrega, out fechaEntParsed)) fechaEntParsed = DateTime.Now;

        //        // Reutilizar entrega existente si ya existe
        //        var entregaExist = await Db.tbEntregaActividadAlumno.FirstOrDefaultAsync(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId);
        //        int entregaAlumnoId;
        //        if (entregaExist == null)
        //        {
        //            tbEntregaActividadAlumno entregaAlumno = new tbEntregaActividadAlumno()
        //            {
        //                ActividadId = actividadId,
        //                AlumnoId = alumnoId,
        //                FechaEntrega = fechaEntParsed,
        //                EstadoEntregaId = 1
        //            };

        //            Db.tbEntregaActividadAlumno.Add(entregaAlumno);
        //            await Db.SaveChangesAsync();
        //            entregaAlumnoId = entregaAlumno.EntregaActividadAlumnoId;
        //        }
        //        else
        //        {
        //            entregaExist.FechaEntrega = fechaEntParsed;
        //            entregaExist.EstadoEntregaId = 1;
        //            await Db.SaveChangesAsync();
        //            entregaAlumnoId = entregaExist.EntregaActividadAlumnoId;
        //        }

        //        // Asegurar que el TipoEntregaId exista antes de insertar
        //        int tipoPreferido = 1;
        //        int tipoResolved = await ResolveTipoEntregaIdAsync(tipoPreferido);
        //        tbEntregables entregables = new tbEntregables()
        //        {
        //            EntregaActividadAlumnoId = entregaAlumnoId,
        //            TipoEntregaId = tipoEntregaId,
        //            Contenido = respuesta,
        //        };
        //        Db.tbEntregables.Add(entregables);
        //        await Db.SaveChangesAsync();



        //        var datosAlumnoActividad = await Db.tbEntregaActividadAlumno.FirstOrDefaultAsync(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId);

        //        //var datosEntregable = await Db.tbEntregablesAlumno.Where(a => a.AlumnoActividadId == alumnoActividadId).FirstOrDefaultAsync();

        //        if (datosAlumnoActividad == null)
        //        {
        //            // No existe registro de entrega en la tabla nueva; retornar BadRequest para que el cliente use el flujo legacy o muestre formulario
        //            return BadRequest();
        //        }

        //        var lsDatosEntregables = await Db.tbEntregables.Where(a => a.EntregaActividadAlumnoId == datosAlumnoActividad.EntregaActividadAlumnoId).ToListAsync();

        //        if (lsDatosEntregables.Count > 0)
        //        {
        //            //int entregaId = datosEntregable.EntregaId;

        //            //var calificacion = await Db.tbCalificaciones.Where(a => a.EntregaId == entregaId).Select(a => a.Calificacion).FirstOrDefaultAsync();

        //            var lsEnvios = new List<EnvioActividadAlumnoResponse>();

        //            foreach (var datoEntregable in lsDatosEntregables)
        //            {
        //                var estadoEntregaId = datosAlumnoActividad.EstadoEntregaId;

        //                var envio = new EnvioActividadAlumnoResponse()
        //                {
        //                    AlumnoId = alumnoId,
        //                    EntregaActividadAlumnoId = datoEntregable.EntregaActividadAlumnoId,
        //                    EntregableId = datoEntregable.EntregableId,
        //                    ActividadId = datosAlumnoActividad.ActividadId,
        //                    FechaEntrega = datosAlumnoActividad.FechaEntrega,
        //                    Contenido = datoEntregable.Contenido,
        //                    Calificacion = datoEntregable.Calificacion ?? 0,
        //                    EstadoEntregaId = estadoEntregaId
        //                };

        //                lsEnvios.Add(envio);
        //            }


        //            return Ok(lsEnvios);
        //        }

        //        return Content(HttpStatusCode.InternalServerError, new
        //        {
        //            mensaje = "Error: No se pudo guardar completamente la entrega."
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(HttpStatusCode.InternalServerError, new
        //        {
        //            mensaje = "Error al registrar el envío de la actividad.",
        //            error = ex.Message,
        //            innerError = ex.InnerException?.Message,
        //            innerInner = ex.InnerException?.InnerException?.Message
        //        });
        //    }
        //}

        /// <summary>
        /// ✅ NUEVO: Registra entrega con TEXTO, ENLACES y ARCHIVOS
        /// Recibe: ActividadId, AlumnoId, Respuesta, Enlaces (JSON), FechaEntrega, TipoEntregaId, files (multipart)
        /// </summary>
        [HttpPost]
        [Route("RegistrarEnvioActividadAlumnoConEnlaces")]
        public async Task<IHttpActionResult> RegistrarEnvioActividadAlumnoConEnlaces()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest == null)
                {
                    return Content(HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Mensaje = "Solicitud vacía",
                        Codigo = "SOLICITUD_VACIA",
                        Detalles = "El servidor no recibió ninguna solicitud HTTP válida."
                    });
                }

                // 1. EXTRAER PARÁMETROS
                int actividadId = 0;
                int alumnoId = 0;
                string respuestaRaw = httpRequest.Form["Respuesta"] ?? string.Empty;
                string enlacesJson = httpRequest.Form["Enlaces"] ?? "[]";
                string fechaEntrega = httpRequest.Form["FechaEntrega"] ?? DateTime.Now.ToString("O");
                int tipoEntregaId = 0;

                int.TryParse(httpRequest.Form["ActividadId"], out actividadId);
                int.TryParse(httpRequest.Form["AlumnoId"], out alumnoId);
                int.TryParse(httpRequest.Form["TipoEntregaId"], out tipoEntregaId);

                Console.WriteLine($"[LOG] Registrando entrega - ActividadId: {actividadId}, AlumnoId: {alumnoId}");




                //#region Validaciones entrega
                //var actividad = Db.tbActividades.FirstOrDefault(a => a.ActividadId == actividadId);

                //var limiteEntrega = actividad.LimiteEntregasPorAlumno;

                //var tieneLimiteEntregas = actividad.TieneLimiteEntregas;

                //var entregasAlumno = Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId).ToList();
                //if (tieneLimiteEntregas)
                //{
                //    var totalEntregasPorAlumno = entregasAlumno.Count;
                //    if (totalEntregasPorAlumno > limiteEntrega)
                //    {
                //        return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //        {
                //            Mensaje = "Limite de entregas",
                //            Codigo = "LIMITE_ENTREGA_ACTIVIDAD_ALUMNO",
                //            Detalles = "Has llegado a tu limite de entregas asignado por el docente."
                //        });
                //    }
                //}



                //#endregion



                //#region PROCESAR RESPUESTA: detectar si viene JSON stringifyado
                //string textoRespuesta = respuestaRaw;
                //List<string> enlacesValidos = new List<string>();
                //List<object> archivosExternos = new List<object>();
                //try
                //{
                //    if (!string.IsNullOrEmpty(respuestaRaw) && respuestaRaw.TrimStart().StartsWith("{"))
                //    {
                //        var respuestaObj = JsonConvert.DeserializeObject<dynamic>(respuestaRaw);
                //        textoRespuesta = respuestaObj.texto ?? respuestaObj.Respuesta ?? "";

                //        // Procesar enlaces del JSON interno
                //        if (respuestaObj.enlaces != null)
                //        {
                //            var enlacesTemp = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(respuestaObj.enlaces));
                //            foreach (var enlace in enlacesTemp)
                //            {
                //                if (_validarURL(enlace))
                //                    enlacesValidos.Add(enlace);
                //            }
                //        }

                //        // Procesar archivos del JSON interno (URLs de archivos subidos)
                //        if (respuestaObj.archivos != null)
                //        {
                //            var archivosTemp = JsonConvert.DeserializeObject<List<object>>(JsonConvert.SerializeObject(respuestaObj.archivos));
                //            foreach (var archivo in archivosTemp)
                //            {
                //                if (archivo != null)
                //                    archivosExternos.Add(archivo);
                //            }
                //        }

                //        Console.WriteLine($"[LOG] Respuesta parseada - texto: {textoRespuesta}, enlaces: {enlacesValidos.Count}, archivos externos: {archivosExternos.Count}");
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"[WARN] Error parseando respuesta JSON: {ex.Message}. Usando texto plano.");
                //    textoRespuesta = respuestaRaw;
                //}
                //#endregion

                //#region 2. VALIDAR PARÁMETROS
                //if (actividadId <= 0 || alumnoId <= 0)
                //{
                //    return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //    {
                //        Mensaje = "Faltan parámetros obligatorios",
                //        Codigo = "PARAMETROS_INVALIDOS",
                //        Detalles = $"ActividadId: {actividadId}, AlumnoId: {alumnoId}"
                //    });
                //}

                //DateTime fechaEntregaParsed;
                //try
                //{
                //    fechaEntregaParsed = DateTime.Parse(fechaEntrega);
                //}
                //catch
                //{
                //    return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //    {
                //        Mensaje = "Formato de fecha inválido",
                //        Codigo = "FECHA_INVALIDA",
                //        Detalles = $"Recibido: {fechaEntrega}"
                //    });
                //}
                //#endregion

                //#region 3. AGREGAR ENLACES ADICIONALES (si vienen en el campo separado)
                //try
                //{
                //    var enlaces = JsonConvert.DeserializeObject<List<string>>(enlacesJson) ?? new List<string>();
                //    foreach (var enlace in enlaces)
                //    {
                //        if (_validarURL(enlace))
                //        {
                //            if (!enlacesValidos.Contains(enlace))
                //            {
                //                enlacesValidos.Add(enlace);
                //                Console.WriteLine($"[LOG] Enlace válido: {enlace}");
                //            }
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"[WARN] Error parseando enlaces JSON: {enlacesJson} - {ex.Message}");
                //}
                //#endregion



                //#region 4. VERIFICAR SI YA EXISTE ENTREGA

                //var entregaActiva = entregasAlumno.FirstOrDefault(a => a.Estatus);

                //var entregaExistente = await Db.tbEntregaActividadAlumno
                //    .FirstOrDefaultAsync(e => e.EntregaActividadAlumnoId == entregaActiva.EntregaActividadAlumnoId);


                //tbEntregaActividadAlumno entregaActividad;
                //int entregaActividadAlumnoId = 0;


                //var fechaLimite = actividad.FechaLimite;
                //var permiteEntregaTardia = actividad.PermitirEntregasTarde;

                //if (entregaActiva.FechaEntrega > fechaLimite && !actividad.PermitirEntregasTarde)
                //{
                //    return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //    {
                //        Mensaje = "Fecha limite de entrega.",
                //        Codigo = "FECHA_LIMITE_ENTREGA_ACTIVIDAD_ALUMNO",
                //        Detalles = $"La fecha de entrega es el {fechaEntrega:dd/MM/yyyy} a las {fechaEntrega:HH:mm}"
                //    });
                //}

                ////if (entregaExistente != null)
                ////{
                ////    int entregaIdExistente = entregaExistente.EntregaActividadAlumnoId;


                ////    entregaActividad = new tbEntregaActividadAlumno()
                ////    {
                ////        ActividadId = actividadId,
                ////        AlumnoId = alumnoId,
                ////        FechaEntrega = fechaEntregaParsed,
                ////        EstadoEntregaId = 1,
                ////        Estatus = true
                ////    };


                ////    if (entregaActiva.FechaEntrega > fechaLimite)
                ////    {
                ////        entregaActiva.EntregaTardia = true;
                ////    }



                ////    Db.tbEntregaActividadAlumno.Add(entregaActividad);
                ////    await Db.SaveChangesAsync();

                ////    entregaActividadAlumnoId = entregaActividad.EntregaActividadAlumnoId;
                ////}
                ////else
                ////{
                ////    // ✅ NUEVA ENTREGA - CREAR
                ////    entregaActividad = new tbEntregaActividadAlumno()
                ////    {
                ////        ActividadId = actividadId,
                ////        AlumnoId = alumnoId,
                ////        FechaEntrega = fechaEntregaParsed,
                ////        EstadoEntregaId = 1
                ////    };


                ////    if (entregaActiva.FechaEntrega > fechaLimite)
                ////    {
                ////        entregaActiva.EntregaTardia = true;
                ////    }

                ////    Db.tbEntregaActividadAlumno.Add(entregaActividad);
                ////    await Db.SaveChangesAsync();

                ////    entregaActividadAlumnoId = entregaActividad.EntregaActividadAlumnoId;
                ////}

                //// ✅ NUEVA ENTREGA - CREAR
                //entregaActividad = new tbEntregaActividadAlumno()
                //{
                //    ActividadId = actividadId,
                //    AlumnoId = alumnoId,
                //    FechaEntrega = fechaEntregaParsed,
                //    EstadoEntregaId = 1,
                //    EntregaTardia = (entregaActiva.FechaEntrega > fechaLimite),
                //    Estatus = true
                //};


                //Db.tbEntregaActividadAlumno.Add(entregaActividad);
                //await Db.SaveChangesAsync();

                //entregaActividadAlumnoId = entregaActividad.EntregaActividadAlumnoId;
                //#endregion


                //#region 5. PROCESAR ARCHIVOS
                //var archivosMetadata = new List<object>();
                //var files = httpRequest.Files;
                //var uploadRoot = HttpContext.Current.Server.MapPath("~/Uploads/Entregas/");
                //var destFolder = Path.Combine(uploadRoot, actividadId.ToString(), alumnoId.ToString());

                //if (!Directory.Exists(destFolder))
                //    Directory.CreateDirectory(destFolder);

                //var extensionesPermitidas = new[]
                //{
                //    ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
                //    ".jpg", ".jpeg", ".png", ".gif", ".txt", ".zip", ".rar", ".7z",
                //    ".odt", ".ods", ".odp", ".rtf"
                //};

                //const long maxPorArchivo = 50 * 1024 * 1024;
                //const long maxTotal = 200 * 1024 * 1024;
                //long tamanoTotal = 0;

                //Console.WriteLine($"[LOG] Procesando {files.Count} archivo(s)");

                //for (int i = 0; i < files.Count; i++)
                //{
                //    var file = files[i];
                //    if (file == null || file.ContentLength == 0)
                //    {
                //        Console.WriteLine($"[LOG] Archivo {i} vacío, se omite");
                //        continue;
                //    }

                //    var extension = Path.GetExtension(file.FileName).ToLower();

                //    // Validar extensión
                //    if (!extensionesPermitidas.Contains(extension))
                //    {
                //        Console.WriteLine($"[ERROR] Extensión no permitida: {extension}");
                //        return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //        {
                //            Mensaje = $"Extensión no permitida: {extension}",
                //            Codigo = "ARCHIVO_NO_PERMITIDO",
                //            Detalles = $"Extensiones válidas: {string.Join(", ", extensionesPermitidas)}"
                //        });
                //    }

                //    // Validar tamaño individual
                //    if (file.ContentLength > maxPorArchivo)
                //    {
                //        Console.WriteLine($"[ERROR] Archivo demasiado grande: {file.FileName}");
                //        return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //        {
                //            Mensaje = "Archivo excede 50MB",
                //            Codigo = "ARCHIVO_MUY_GRANDE",
                //            Detalles = $"Archivo: {file.FileName} ({file.ContentLength / (1024 * 1024)}MB)"
                //        });
                //    }

                //    tamanoTotal += file.ContentLength;

                //    // Validar tamaño total
                //    if (tamanoTotal > maxTotal)
                //    {
                //        Console.WriteLine($"[ERROR] Tamaño total excedido");
                //        return Content(HttpStatusCode.BadRequest, new ErrorResponse
                //        {
                //            Mensaje = "Tamaño total excede 200MB",
                //            Codigo = "ESPACIO_INSUFICIENTE",
                //            Detalles = $"Total actual: {tamanoTotal / (1024 * 1024)}MB"
                //        });
                //    }

                //    // Guardar archivo
                //    var safeName = Path.GetFileName(file.FileName);
                //    var destPath = Path.Combine(destFolder, safeName);

                //    if (File.Exists(destPath))
                //    {
                //        var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                //        safeName = $"{ts}_{safeName}";
                //        destPath = Path.Combine(destFolder, safeName);
                //    }

                //    file.SaveAs(destPath);
                //    var ruta = $"/Uploads/Entregas/{actividadId}/{alumnoId}/{safeName}";

                //    archivosMetadata.Add(new
                //    {
                //        nombre = file.FileName,
                //        nombreGuardado = safeName,
                //        size = file.ContentLength,
                //        ruta = ruta,
                //        fechaGuardado = DateTime.Now
                //    });

                //    Console.WriteLine($"[LOG] Archivo guardado: {ruta}");
                //}
                //#endregion


                //#region 6. DETERMINAR TIPO DE ENTREGA
                //int tipoEntregaDeterminado = _determinarTipoEntrega(textoRespuesta, enlacesValidos, archivosMetadata);

                //// Combinar archivos subidos directamente con archivos del JSON (URLs)
                //var todosArchivos = new List<object>();
                //todosArchivos.AddRange(archivosMetadata);
                //todosArchivos.AddRange(archivosExternos);

                //// 7. CREAR ENTREGABLE CON TODO ESTRUCTURADO
                //var contenidoEstructurado = new
                //{
                //    texto = textoRespuesta,
                //    enlaces = enlacesValidos,
                //    archivos = todosArchivos,
                //    fechaEntrega = DateTime.Now,
                //    totalArchivos = todosArchivos.Count,
                //    totalEnlaces = enlacesValidos.Count
                //};

                //var entregable = new tbEntregables()
                //{
                //    EntregaActividadAlumnoId = entregaActividadAlumnoId,
                //    TipoEntregaId = tipoEntregaDeterminado,
                //    Contenido = JsonConvert.SerializeObject(contenidoEstructurado),
                //    Calificacion = null
                //};

                //Db.tbEntregables.Add(entregable);
                //await Db.SaveChangesAsync();

                //Console.WriteLine($"[LOG] Entregable creado: {entregable.EntregableId}");

                //// 8. LIMPIAR CACHÉ
                //Db.ChangeTracker.Entries()
                //    .Where(e => e.Entity is tbEntregables || e.Entity is tbEntregaActividadAlumno)
                //    .ToList()
                //    .ForEach(e => e.State = System.Data.Entity.EntityState.Detached);

                //// 9. RETORNAR RESPUESTA
                //var datosAlumnoActividad = await Db.tbEntregaActividadAlumno
                //    .FirstOrDefaultAsync(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId);

                //var lsDatosEntregables = Db.tbEntregables
                //    .Where(a => a.EntregaActividadAlumnoId == datosAlumnoActividad.EntregaActividadAlumnoId)
                //    .ToList();

                //var lsEnvios = new List<object>();

                //foreach (var datoEntregable in lsDatosEntregables)
                //{
                //    lsEnvios.Add(new
                //    {
                //        AlumnoId = alumnoId,
                //        EntregaActividadAlumnoId = datoEntregable.EntregaActividadAlumnoId,
                //        EntregableId = datoEntregable.EntregableId,
                //        ActividadId = datosAlumnoActividad.ActividadId,
                //        FechaEntrega = datosAlumnoActividad.FechaEntrega,
                //        Contenido = datoEntregable.Contenido,
                //        Calificacion = datoEntregable.Calificacion ?? 0,
                //        EstadoEntregaId = datosAlumnoActividad.EstadoEntregaId,
                //        TipoEntrega = tipoEntregaDeterminado
                //    });
                //}
                //#endregion


                var lsEnvios = await AlumnoApiService.RegistrarEnvioActividadAlumnoConEnlaces(httpRequest, actividadId, alumnoId, tipoEntregaId, fechaEntrega, respuestaRaw, enlacesJson);

                return Ok(new SuccessResponse
                {
                    //Mensaje = $"Entrega registrada correctamente ({archivosMetadata.Count} archivo(s), {enlacesValidos.Count} enlace(s))",
                    Mensaje = $"Entrega registrada correctamente",
                    Codigo = "EXITO",
                    Datos = lsEnvios
                });
            }
            catch (EntregaAlumnoException e)
            {
                var mensaje = e.Message;
                return Content(HttpStatusCode.BadRequest, new ErrorResponse
                {
                    //Mensaje = "Limite de entregas",
                    //Codigo = "LIMITE_ENTREGA_ACTIVIDAD_ALUMNO",
                    Detalles = mensaje
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RegistrarEnvioActividadAlumnoConEnlaces: {ex.Message}\n{ex.StackTrace}");
                return Content(HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Mensaje = "Error al registrar la entrega",
                    Codigo = "ERROR_INTERNO",
                    Detalles = ex.Message
                });
            }
        }


        #region FG
        /// <summary>
        /// Helper: Valida que una URL sea válida
        /// </summary>
        private bool _validarURL(string url)
        {
            try
            {
                Uri result;
                bool esValida = Uri.TryCreate(url, UriKind.Absolute, out result) &&
                    (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
                return esValida;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Helper: Determina el tipo de entrega según el contenido
        /// </summary>
        private int _determinarTipoEntrega(string texto, List<string> enlaces, List<object> archivos)
        {
            bool tieneTexto = !string.IsNullOrEmpty(texto);
            bool tieneEnlaces = enlaces != null && enlaces.Any();
            bool tieneArchivos = archivos != null && archivos.Any();

            if (tieneTexto && tieneEnlaces && tieneArchivos) return 4;
            if (tieneArchivos) return 3;
            if (tieneEnlaces) return 2;
            if (tieneTexto) return 1;
            return 1;

        }


        /// <summary>
        /// Método auxiliar para formatear tamaños de archivo
        /// </summary>
        private string FormatearTamano(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
        #endregion


        /// <summary>
        /// Registra el envío de una actividad del alumno con soporte para archivos y texto
        /// Recibe: ActividadId, AlumnoId, Respuesta (opcional), FechaEntrega, TipoEntregaId, y archivos
        /// </summary>
        //[HttpPost]
        //[Route("RegistrarEnvioActividadAlumnoConArchivos")]
        //public async Task<IHttpActionResult> RegistrarEnvioActividadAlumnoConArchivos()
        //{
        //    try
        //    {
        //        var httpRequest = HttpContext.Current.Request;
        //        if (httpRequest == null)
        //            return Content(HttpStatusCode.BadRequest, new ErrorResponse
        //            {
        //                Mensaje = "No se recibió la solicitud.",
        //                Codigo = "SOLICITUD_VACIA",
        //                Detalles = "El servidor no recibió ninguna solicitud HTTP válida."
        //            });

        //        // 1. EXTRAER PARÁMETROS
        //        int actividadId = 0;
        //        int alumnoId = 0;
        //        string respuesta = httpRequest.Form["Respuesta"] ?? string.Empty;
        //        string fechaEntrega = httpRequest.Form["FechaEntrega"] ?? DateTime.Now.ToString("O");
        //        int tipoEntregaId = 0;

        //        int.TryParse(httpRequest.Form["ActividadId"], out actividadId);
        //        int.TryParse(httpRequest.Form["AlumnoId"], out alumnoId);
        //        int.TryParse(httpRequest.Form["TipoEntregaId"], out tipoEntregaId);

        //        // 2. VALIDAR PARÁMETROS OBLIGATORIOS
        //        if (actividadId <= 0 || alumnoId <= 0)
        //        {
        //            return Content(HttpStatusCode.BadRequest, new ErrorResponse
        //            {
        //                Mensaje = "Faltan datos obligatorios.",
        //                Codigo = "DATOS_INCOMPLETOS",
        //                Detalles = $"ActividadId y AlumnoId deben ser mayores a 0. Recibido - ActividadId: {actividadId}, AlumnoId: {alumnoId}"
        //            });
        //        }

        //        // 3. VALIDAR FECHA
        //        DateTime fechaEntregaParsed;
        //        try
        //        {
        //            fechaEntregaParsed = DateTime.Parse(fechaEntrega);
        //        }
        //        catch
        //        {
        //            return Content(HttpStatusCode.BadRequest, new ErrorResponse
        //            {
        //                Mensaje = "Formato de fecha inválido.",
        //                Codigo = "FECHA_INVALIDA",
        //                Detalles = $"La fecha debe estar en formato ISO 8601. Recibido: {fechaEntrega}"
        //            });
        //        }

        //        // 4. CREAR REGISTRO EN tbEntregaActividadAlumno
        //        var entregaActividad = new tbEntregaActividadAlumno()
        //        {
        //            ActividadId = actividadId,
        //            AlumnoId = alumnoId,
        //            FechaEntrega = fechaEntregaParsed,
        //            EstadoEntregaId = 1
        //        };

        //        Db.tbEntregaActividadAlumno.Add(entregaActividad);
        //        await Db.SaveChangesAsync();

        //        int entregaActividadAlumnoId = entregaActividad.EntregaActividadAlumnoId;
        //        Console.WriteLine($"[LOG] Creada entrega para Actividad {actividadId}, Alumno {alumnoId}. EntregaId: {entregaActividadAlumnoId}");

        //        // 5. PROCESAR ARCHIVOS
        //        var savedUrls = new List<string>();
        //        var files = httpRequest.Files;
        //        var uploadRoot = HttpContext.Current.Server.MapPath("~/Uploads/Entregas/");
        //        var destFolder = Path.Combine(uploadRoot, actividadId.ToString(), alumnoId.ToString());

        //        if (!Directory.Exists(destFolder))
        //            Directory.CreateDirectory(destFolder);

        //        // Extensiones permitidas
        //        var extensionesPermitidas = new[]
        //        {
        //            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        //            ".jpg", ".jpeg", ".png", ".gif", ".txt", ".zip", ".rar", ".7z",
        //            ".odt", ".ods", ".odp", ".rtf"
        //        };

        //        // Límites
        //        const long maxTamanoPorArchivo = 25 * 1024 * 1024; // 25MB
        //        const long maxTamanoTotal = 100 * 1024 * 1024; // 100MB total
        //        long tamanoTotalArchivos = 0;

        //        Console.WriteLine($"[LOG] Procesando {files.Count} archivo(s)...");

        //        for (int i = 0; i < files.Count; i++)
        //        {
        //            var file = files[i];
        //            if (file == null || file.ContentLength == 0)
        //            {
        //                Console.WriteLine($"[LOG] Archivo {i} está vacío, se omite.");
        //                continue;
        //            }

        //            var extension = Path.GetExtension(file.FileName).ToLower();

        //            // Validar extensión
        //            if (!extensionesPermitidas.Contains(extension))
        //            {
        //                Console.WriteLine($"[ERROR] Extensión no permitida: {extension}");
        //                return Content(HttpStatusCode.BadRequest, new ErrorResponse
        //                {
        //                    Mensaje = "Tipo de archivo no permitido.",
        //                    Codigo = "ARCHIVO_NO_PERMITIDO",
        //                    Detalles = $"La extensión '{extension}' no es permitida. Extensiones válidas: {string.Join(", ", extensionesPermitidas)}"
        //                });
        //            }

        //            // Validar tamaño individual
        //            if (file.ContentLength > maxTamanoPorArchivo)
        //            {
        //                Console.WriteLine($"[ERROR] Archivo demasiado grande: {file.FileName}");
        //                return Content(HttpStatusCode.BadRequest, new ErrorResponse
        //                {
        //                    Mensaje = "Archivo demasiado grande.",
        //                    Codigo = "ARCHIVO_MUY_GRANDE",
        //                    Detalles = $"El archivo '{file.FileName}' excede el límite de 50MB. Tamaño: {file.ContentLength / (1024 * 1024)}MB"
        //                });
        //            }

        //            tamanoTotalArchivos += file.ContentLength;

        //            // Validar tamaño total
        //            if (tamanoTotalArchivos > maxTamanoTotal)
        //            {
        //                Console.WriteLine($"[ERROR] Tamaño total de archivos excedido");
        //                return Content(HttpStatusCode.BadRequest, new ErrorResponse
        //                {
        //                    Mensaje = "Tamaño total de archivos excedido.",
        //                    Codigo = "ESPACIO_INSUFICIENTE",
        //                    Detalles = $"El tamaño total de los archivos no debe exceder 200MB. Actual: {tamanoTotalArchivos / (1024 * 1024)}MB"
        //                });
        //            }

        //            // Generar nombre seguro
        //            var safeName = Path.GetFileName(file.FileName);
        //            var destPath = Path.Combine(destFolder, safeName);

        //            // Evitar sobreescribir: agregar timestamp
        //            if (File.Exists(destPath))
        //            {
        //                var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
        //                safeName = $"{ts}_{safeName}";
        //                destPath = Path.Combine(destFolder, safeName);
        //            }

        //            // Guardar archivo
        //            file.SaveAs(destPath);
        //            var relativeUrl = $"/Uploads/Entregas/{actividadId}/{alumnoId}/{safeName}";
        //            savedUrls.Add(relativeUrl);

        //            Console.WriteLine($"[LOG] Archivo guardado: {relativeUrl}");
        //        }

        //        // 6. CREAR OBJETO CON CONTENIDO + ARCHIVOS
        //        var contenidoEntregable = new
        //        {
        //            Respuesta = respuesta ?? string.Empty,
        //            Archivos = savedUrls,
        //            FechaGuardado = DateTime.Now,
        //            TotalArchivos = savedUrls.Count,
        //            TamanoTotal = FormatearTamano(tamanoTotalArchivos)
        //        };

        //        string contenidoJson = JsonConvert.SerializeObject(contenidoEntregable);
        //        Console.WriteLine($"[LOG] Contenido JSON: {contenidoJson}");

        //        // 7. CREAR REGISTRO EN tbEntregables
        //        var entregable = new tbEntregables()
        //        {
        //            EntregaActividadAlumnoId = entregaActividadAlumnoId,
        //            TipoEntregaId = tipoEntregaId > 0 ? tipoEntregaId : 1,
        //            Contenido = contenidoJson,
        //            Calificacion = null
        //        };

        //        Db.tbEntregables.Add(entregable);
        //        await Db.SaveChangesAsync();

        //        Console.WriteLine($"[LOG] Entregable guardado con ID: {entregable.EntregableId}");

        //        // 8. OBTENER Y RETORNAR RESPUESTA
        //        var entregables = Db.tbEntregables
        //            .Where(a => a.EntregaActividadAlumnoId == entregaActividadAlumnoId)
        //            .ToList();

        //        var lsEnvios = entregables.Select(datoEntregable => new
        //        {
        //            AlumnoId = alumnoId,
        //            EntregaActividadAlumnoId = datoEntregable.EntregaActividadAlumnoId,
        //            EntregableId = datoEntregable.EntregableId,
        //            ActividadId = actividadId,
        //            FechaEntrega = entregaActividad.FechaEntrega,
        //            Contenido = datoEntregable.Contenido,
        //            Calificacion = datoEntregable.Calificacion ?? 0,
        //            EstadoEntregaId = entregaActividad.EstadoEntregaId
        //        }).ToList();

        //        return Ok(new SuccessResponse
        //        {
        //            Mensaje = $"Entrega registrada correctamente. {savedUrls.Count} archivo(s) guardado(s).",
        //            Codigo = "EXITO",
        //            Datos = lsEnvios
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[ERROR] RegistrarEnvioActividadAlumnoConArchivos: {ex.Message}\n{ex.StackTrace}");
        //        return Content(HttpStatusCode.InternalServerError, new ErrorResponse
        //        {
        //            Mensaje = "Error al registrar la entrega con archivos.",
        //            Codigo = "ERROR_INTERNO",
        //            Detalles = ex.Message
        //        });
        //    }
        //}



        [HttpGet]
        [Route("ObtenerEnviosActividadesAlumno")]
        public async Task<IHttpActionResult> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            try
            {
                var lsEnvios = await AlumnoApiService.ObtenerEnviosActividadesAlumno(ActividadId, AlumnoId);

                if (lsEnvios.Count > 0)
                {
                    return Ok(lsEnvios);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                // 9. ✅ Logging detallado del error
                return Content(HttpStatusCode.InternalServerError, new
                {
                    mensaje = "Error al obtener los envíos de la actividad.",
                    error = ex.Message,
                    ActividadId = ActividadId,
                    AlumnoId = AlumnoId
                });
            }
        }

        /// <summary>
        /// Elimina la inscripción de un alumno a una materia
        /// Recibe: MateriaId y AlumnoId
        /// </summary>
        [HttpPost]
        [Route("EliminarAlumnoMateria")]
        public async Task<IHttpActionResult> EliminarAlumnoDeMateria([FromBody] dynamic request)
        {
            try
            {
                if (request == null)
                {
                    return Content(HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Mensaje = "El cuerpo de la solicitud está vacío.",
                        Codigo = AlumnoErrorCodes.ERROR_INTERNO,
                        Detalles = "Se esperaba un objeto JSON con MateriaId y AlumnoId."
                    });
                }

                // Extraer MateriaId y AlumnoId
                int materiaId = Convert.ToInt32(request.MateriaId ?? request.materiaId ?? 0);
                int alumnoId = Convert.ToInt32(request.AlumnoId ?? request.alumnoId ?? 0);

                if (materiaId <= 0 || alumnoId <= 0)
                {
                    return Content(HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Mensaje = "Los datos enviados son inválidos.",
                        Codigo = AlumnoErrorCodes.ERROR_INTERNO,
                        Detalles = $"MateriaId y AlumnoId deben ser mayores a 0. Recibido - MateriaId: {materiaId}, AlumnoId: {alumnoId}"
                    });
                }

                await AlumnoApiService.EliminarAlumnoDeMateria(materiaId, alumnoId);

                return Ok(new SuccessResponse
                {
                    Mensaje = "El alumno ha sido desinscrito de la materia correctamente.",
                    Codigo = "EXITO",
                    Datos = new { AlumnoId = alumnoId, MateriaId = materiaId }
                });
            }
            catch (AlumnosException e)
            {
                var mensaje = e.Mensaje;
                var detalles = e.Detalles;

                return Content(HttpStatusCode.NotFound, new ErrorResponse
                {
                    Mensaje = mensaje,
                    Detalles = detalles
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] EliminarAlumnoDeMateria: {e.Message}\n{e.StackTrace}");
                return Content(HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Mensaje = "Ocurrió un error interno al intentar desincribir al alumno.",
                    Codigo = AlumnoErrorCodes.ERROR_INTERNO,
                    Detalles = e.Message
                });
            }
        }

        /// <summary>
        /// Elimina la inscripción de un alumno a un grupo
        /// Recibe: GrupoId y AlumnoId
        /// </summary>
        [HttpPost]
        [Route("EliminarAlumnoGrupo")]
        public async Task<IHttpActionResult> EliminarAlumnoDeGrupo([FromBody] dynamic request)
        {
            try
            {
                if (request == null)
                {
                    return Content(HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Mensaje = "El cuerpo de la solicitud está vacío.",
                        Codigo = AlumnoErrorCodes.ERROR_INTERNO,
                        Detalles = "Se esperaba un objeto JSON con GrupoId y AlumnoId."
                    });
                }

                // Extraer GrupoId y AlumnoId
                int grupoId = Convert.ToInt32(request.GrupoId ?? request.grupoId ?? 0);
                int alumnoId = Convert.ToInt32(request.AlumnoId ?? request.alumnoId ?? 0);

                if (grupoId <= 0 || alumnoId <= 0)
                {
                    return Content(HttpStatusCode.BadRequest, new ErrorResponse
                    {
                        Mensaje = "Los datos enviados son inválidos.",
                        Codigo = AlumnoErrorCodes.ERROR_INTERNO,
                        Detalles = $"GrupoId y AlumnoId deben ser mayores a 0. Recibido - GrupoId: {grupoId}, AlumnoId: {alumnoId}"
                    });
                }

                await AlumnoApiService.EliminarAlumnoDeGrupo(grupoId, alumnoId);

                return Ok(new SuccessResponse
                {
                    Mensaje = "El alumno ha sido desinscrito del grupo correctamente.",
                    Codigo = "EXITO",
                    Datos = new { AlumnoId = alumnoId, GrupoId = grupoId }
                });
            }
            catch (AlumnosException e)
            {
                var mensaje = e.Mensaje;
                var detalles = e.Detalles;

                return Content(HttpStatusCode.NotFound, new ErrorResponse
                {
                    Mensaje = mensaje,
                    Detalles = detalles
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] EliminarAlumnoDeGrupo: {e.Message}\n{e.StackTrace}");
                return Content(HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Mensaje = "Ocurrió un error interno al intentar desincribir al alumno del grupo.",
                    Codigo = AlumnoErrorCodes.ERROR_INTERNO,
                    Detalles = e.Message
                });
            }
        }



        [HttpPost]
        [Route("CancelarEnvioActividadAlumno")]
        public async Task<IHttpActionResult> CancelarEnvioActividadAlumno([FromBody] CancelarEnvioActividadAlumno datosCancelacion)
        {
            try
            {
                //var alumnoActividadId = datosCancelacion.AlumnoActividadId;
                var alumnoId = datosCancelacion.AlumnoId;
                var actividadId = datosCancelacion.ActividadId;



                //var alumnoActividadEliminar = Db.tbEntregaActividadAlumno.FirstOrDefault(a => a.AlumnoId == alumnoId && a.ActividadId == actividadId && a.EstadoEntregaId == 1);

                //if (alumnoActividadEliminar != null)
                //{
                //    var entregables = Db.tbEntregables.Where(a => a.EntregaActividadAlumnoId == alumnoActividadEliminar.EntregaActividadAlumnoId).ToList();

                //    foreach (var entrega in entregables)
                //    {
                //        if (entrega.Calificacion.HasValue)
                //        {
                //            return BadRequest("No puedes cancelar esta entrega pues ya esta calificada");
                //        }
                //    }

                //    foreach (var entrega in entregables)
                //    {
                //        Db.tbEntregables.Remove(entrega);
                //    }
                //    await Db.SaveChangesAsync();


                //    alumnoActividadEliminar.Estatus = false;
                //    Db.Entry(alumnoActividadEliminar).State = EntityState.Modified;

                //    await Db.SaveChangesAsync();

                //    return Ok();
                //}

                //return BadRequest();

                await AlumnoApiService.CancelarEnvioActividad(alumnoId, actividadId);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        //Registrar
        [Route("AlumnoGrupoCodigo")]
        public async Task<IHttpActionResult> AlumnoGrupoCodigo([FromBody] AlumnoGMRegistroCodigo alumnoGrupoRegistro)
        {
            try
            {
                int alumnoId = alumnoGrupoRegistro.AlumnoId;
                string codigoAcceso = alumnoGrupoRegistro.CodigoAcceso;


                var grupoId = Db.tbGrupos.Where(a => a.CodigoAcceso == codigoAcceso).Select(a => a.GrupoId).FirstOrDefault();

                tbAlumnosGrupos alumnoGrupo = new tbAlumnosGrupos()
                {
                    AlumnoId = alumnoId,
                    GrupoId = grupoId,
                };

                Db.tbAlumnosGrupos.Add(alumnoGrupo);
                await Db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = e.Message });
            }
        }


        [HttpPost]
        [Route("AlumnoMateriaCodigo")]
        public async Task<IHttpActionResult> AlumnoMateriaCodigo([FromBody] AlumnoGMRegistroCodigo alumnoMateriaRegistro)
        {
            try
            {
                int alumnoId = alumnoMateriaRegistro.AlumnoId;
                string codigoAcceso = alumnoMateriaRegistro.CodigoAcceso;


                var materiaId = Db.tbMaterias.Where(a => a.CodigoAcceso == codigoAcceso).Select(a => a.MateriaId).FirstOrDefault();

                tbAlumnosMaterias alumnoMateria = new tbAlumnosMaterias()
                {
                    AlumnoId = alumnoId,
                    MateriaId = materiaId
                };

                Db.tbAlumnosMaterias.Add(alumnoMateria);
                await Db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = e.Message });
            }
        }


        [HttpPost]
        [Route("VerificarAlumnoEmail")]
        public async Task<IHttpActionResult> VerificarAlumnoEmail([FromBody] EmailVerificadoAlumno verifyEmail)
        {
            try
            {
                var email = verifyEmail.Email;
                if (!string.IsNullOrEmpty(email))
                {
                    var user = await UserManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        var alumnoExiste = Db.tbAlumnos.Any(a => a.UserId == user.Id);

                        if (alumnoExiste)
                        {
                            return Ok(new { Email = email });
                        }
                        return BadRequest();
                    }
                }
                return Content(HttpStatusCode.BadRequest, new { Email = email });

            }
            catch (Exception)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = "Correo no valido" });
            }
        }

        [HttpPost]
        [Route("RegistrarAlumnoGMDocente")]
        public async Task<IHttpActionResult> RegistrarAlumnoGMDocente([FromBody] AlumnoGMRegistroDocente alumnoGMRegistro)
        {
            bool alumnoRegistradoGrupo = false;
            bool alumnoRegistradoMateria = false;
            int docenteId = -1;
            List<int> lsAlumnosId = new List<int>();
            try
            {
                List<string> lsEmails = alumnoGMRegistro.Emails;

                int grupoId = alumnoGMRegistro.GrupoId;
                int materiaId = alumnoGMRegistro.MateriaId;

                //foreach (var email in lsEmails)
                //{
                //    var user = await UserManager.FindByEmailAsync(email);

                //    if (user != null)
                //    {
                //        var alumnoId = await Db.tbAlumnos.Where(a => a.UserId == user.Id).Select(a => a.AlumnoId).FirstOrDefaultAsync();

                //        lsAlumnosId.Add(alumnoId);
                //    }
                //}

                //int grupoId = alumnoGMRegistro.GrupoId;
                //int materiaId = alumnoGMRegistro.MateriaId;


                //if (grupoId != 0)
                //{
                //    docenteId = await Db.tbGrupos.Where(a => a.GrupoId == grupoId).Select(a => a.DocenteId).FirstOrDefaultAsync();
                //    foreach (var aluId in lsAlumnosId)
                //    {
                //        bool alumnoYaRegistrado = Db.tbAlumnosGrupos.Any(a => a.GrupoId == grupoId && a.AlumnoId == aluId);
                //        if (!alumnoYaRegistrado)
                //        {
                //            tbAlumnosGrupos alumnosGrupos = new tbAlumnosGrupos()
                //            {
                //                AlumnoId = aluId,
                //                GrupoId = grupoId
                //            };

                //            Db.tbAlumnosGrupos.Add(alumnosGrupos);
                //        }
                //        else
                //        {
                //            Content(HttpStatusCode.BadRequest, new { mensaje = "El alumno ya esta registrado" });
                //        }
                //    }
                //    await Db.SaveChangesAsync();
                //    alumnoRegistradoGrupo = true;
                //    var lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);
                //    return Ok(lsAlumnos);
                //}
                //else if (materiaId != 0)
                //{
                //    docenteId = await Db.tbMaterias.Where(a => a.MateriaId == materiaId).Select(a => a.DocenteId).FirstOrDefaultAsync();
                //    foreach (var aluId in lsAlumnosId)
                //    {
                //        bool alumnoYaRegistrado = Db.tbAlumnosMaterias.Any(a => a.MateriaId == materiaId && a.AlumnoId == aluId);
                //        if (!alumnoYaRegistrado)
                //        {
                //            tbAlumnosMaterias alumnosMaterias = new tbAlumnosMaterias()
                //            {
                //                AlumnoId = aluId,
                //                MateriaId = materiaId
                //            };
                //            Db.tbAlumnosMaterias.Add(alumnosMaterias);
                //        }
                //        else
                //        {
                //            return Content(HttpStatusCode.BadRequest, new { mensaje = "El alumno ya esta registrado" });
                //        }
                //    }
                //    await Db.SaveChangesAsync();
                //    alumnoRegistradoMateria = true;
                //    var lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);

                //    return Ok(lsAlumnos);
                //}


                //TODO: Retornar UserName, Nombre, Apellido Paterno, ApellidoMaterno
                //return Ok(new { mensaje = "El alumno fue agregado correctamente" });

                var registrarAlumnoGrupoMateria = await AlumnoApiService.RegistrarAlumnoGrupoMateriaDocente(lsEmails, grupoId, materiaId);

                alumnoRegistradoGrupo = registrarAlumnoGrupoMateria.AlumnoRegistradoGrupo;
                alumnoRegistradoMateria = registrarAlumnoGrupoMateria.AlumnoRegistradoMateria;
                var lsAlumnos = registrarAlumnoGrupoMateria.Alumnos;

                return Ok(lsAlumnos);
            }
            catch (AlumnosException e)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = e.Mensaje });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = e.Message });
            }
            finally
            {
                int grupoId = alumnoGMRegistro.GrupoId;
                int materiaId = alumnoGMRegistro.MateriaId;

                if (alumnoRegistradoGrupo)
                {
                    await Ns.NotificacionRegistrarAlumnoClase(lsAlumnosId, docenteId, grupoId: grupoId);
                }
                else if (alumnoRegistradoMateria)
                {
                    await Ns.NotificacionRegistrarAlumnoClase(lsAlumnosId, docenteId, materiaId: materiaId);
                }
            }
        }


        [HttpPost]
        [Route("ObtenerListaAlumnosGrupo")]
        public async Task<IHttpActionResult> ObtenerListaAlumnosGrupo([FromBody] Indices indice)
        {
            try
            {
                int grupoId = indice.GrupoId;

                //List<int> lsAlumnosId = await Db.tbAlumnosGrupos.Where(a => a.GrupoId == grupoId).Select(a => a.AlumnoId).ToListAsync();

                //List<EmailVerificadoAlumno> lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);

                var lsAlumnos = await AlumnoApiService.ObtenerListaAlumnosGrupo(grupoId);

                return Ok(lsAlumnos);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = e.Message });
            }
        }

        [HttpPost]
        [Route("ObtenerListaAlumnosMateria")]
        public async Task<IHttpActionResult> ObtenerListaAlumnosMateria(
            [FromBody] Indices indice)
        {
            try
            {
                int grupoId = indice.GrupoId;
                int materiaId = indice.MateriaId;

                //// =========================
                //// CASO 1: CON GRUPO
                //// =========================
                //if (grupoId > 0 && materiaId > 0)
                //{
                //    var alumnosGrupo = await Db.tbAlumnosGrupos
                //        .Where(g => g.GrupoId == grupoId)
                //        .Select(g => g.AlumnoId)
                //        .ToListAsync();

                //    var alumnos = await ObtenerListaAlumnos(alumnosGrupo);

                //    return Ok(alumnos);
                //}

                //// =========================
                //// CASO 2: SIN GRUPO (MATERIA)
                //// =========================
                //var alumnosMateria = await Db.tbAlumnosMaterias
                //    .Where(am => am.MateriaId == materiaId)
                //    .Select(am => new
                //    {
                //        am.AlumnoMateriaId,
                //        am.AlumnoId
                //    })
                //    .ToListAsync();

                //if (!alumnosMateria.Any())
                //    return Ok(new List<object>());

                //var alumnosIds = alumnosMateria
                //    .Select(a => a.AlumnoId)
                //    .ToList();

                //var alumnosInfo = await ObtenerListaAlumnos(alumnosIds);

                //// 🔑 UNIÓN CORRECTA
                //var resultado = alumnosInfo.Select((a, index) => new
                //{
                //    AlumnoMateriaId = alumnosMateria[index].AlumnoMateriaId,
                //    AlumnoId = alumnosMateria[index].AlumnoId,

                //    UserName = a.UserName,
                //    Email = a.Email,
                //    Nombre = a.Nombre,
                //    ApellidoPaterno = a.ApellidoPaterno,
                //    ApellidoMaterno = a.ApellidoMaterno,
                //    GrupoId = (int?)null
                //});

                var resultado = await AlumnoApiService.ObtenerListaAlumnosMateria(grupoId, materiaId);

                return Ok(resultado);
            }
            catch (Exception e)
            {
                return Content(
                    HttpStatusCode.BadRequest,
                    new { mensaje = e.Message }
                );
            }
        }

        // Dentro de tu AlumnoApiController.cs o la clase donde se encuentra el método

        //private async Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnos(List<int> lsAlumnosId)
        //{
        //    try
        //    {
        //        var lsAlumnos = new List<EmailVerificadoAlumno>();
        //        foreach (var id in lsAlumnosId)
        //        {
        //            var alumnoDatos = Db.tbAlumnos.Where(a => a.AlumnoId == id).FirstOrDefault();
        //            if (alumnoDatos != null)
        //            {
        //                var userName = await UserManager.FindByIdAsync(alumnoDatos.UserId);

        //                var alumno = new EmailVerificadoAlumno()
        //                {
        //                    // 🎯 LÍNEA AÑADIDA: Asignar el ID del alumno al DTO de respuesta
        //                    AlumnoId = alumnoDatos.AlumnoId,
        //                    // O si tu DTO usa la propiedad 'Id': Id = alumnoDatos.AlumnoId, 

        //                    Email = userName?.Email ?? "",
        //                    UserName = userName?.UserName ?? "",
        //                    Nombre = alumnoDatos.Nombre,
        //                    ApellidoPaterno = alumnoDatos.ApellidoPaterno,
        //                    ApellidoMaterno = alumnoDatos.ApellidoMaterno,
        //                };

        //                lsAlumnos.Add(alumno);
        //            }
        //        }
        //        return lsAlumnos;
        //    }
        //    catch (Exception)
        //    {
        //        return new List<EmailVerificadoAlumno>();
        //    }
        //}

        /// <summary>
        /// Elimina un alumno de un grupo
        /// Recibe: GrupoId y AlumnoId
        /// </summary>
        //[HttpPost]
        //[Route("EliminarAlumnoDelGrupo")]
        //public async Task<IHttpActionResult> EliminarAlumnoGrupo([FromBody] dynamic request)
        //{
        //    try
        //    {
        //        if (request == null)
        //        {
        //            return Content(HttpStatusCode.BadRequest, new ErrorResponse
        //            {
        //                Mensaje = "El cuerpo de la solicitud está vacío.",
        //                Codigo = AlumnoErrorCodes.ERROR_INTERNO,
        //                Detalles = "Se esperaba un objeto JSON con GrupoId y AlumnoId."
        //            });
        //        }

        //        // Extraer GrupoId y AlumnoId
        //        int grupoId = Convert.ToInt32(request.GrupoId ?? request.grupoId ?? 0);
        //        int alumnoId = Convert.ToInt32(request.AlumnoId ?? request.alumnoId ?? 0);

        //        if (grupoId <= 0 || alumnoId <= 0)
        //        {
        //            return Content(HttpStatusCode.BadRequest, new ErrorResponse
        //            {
        //                Mensaje = "Los datos enviados son inválidos.",
        //                Codigo = AlumnoErrorCodes.ERROR_INTERNO,
        //                Detalles = $"GrupoId y AlumnoId deben ser mayores a 0. Recibido - GrupoId: {grupoId}, AlumnoId: {alumnoId}"
        //            });
        //        }

        //        // Buscar la relación alumno-grupo
        //        var alumnoGrupo = await Db.tbAlumnosGrupos
        //            .FirstOrDefaultAsync(a => a.AlumnoId == alumnoId && a.GrupoId == grupoId);

        //        if (alumnoGrupo == null)
        //        {
        //            return Content(HttpStatusCode.NotFound, new ErrorResponse
        //            {
        //                Mensaje = "El alumno no está inscrito en este grupo.",
        //                Codigo = AlumnoErrorCodes.ALUMNO_NO_ENCONTRADO,
        //                Detalles = $"No se encontró una inscripción del alumno {alumnoId} en el grupo {grupoId}."
        //            });
        //        }

        //        // Eliminar la inscripción
        //        Db.tbAlumnosGrupos.Remove(alumnoGrupo);
        //        await Db.SaveChangesAsync();

        //        // Limpiar caché
        //        Db.ChangeTracker.Entries()
        //            .Where(e => e.Entity is tbAlumnosGrupos)
        //            .ToList()
        //            .ForEach(e => e.State = System.Data.Entity.EntityState.Detached);

        //        Console.WriteLine($"[LOG] Alumno {alumnoId} eliminado del grupo {grupoId}.");

        //        return Ok(new SuccessResponse
        //        {
        //            Mensaje = "El alumno ha sido eliminado del grupo correctamente.",
        //            Codigo = "EXITO",
        //            Datos = new { AlumnoId = alumnoId, GrupoId = grupoId }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[ERROR] EliminarAlumnoGrupo: {ex.Message}\n{ex.StackTrace}");
        //        return Content(HttpStatusCode.InternalServerError, new ErrorResponse
        //        {
        //            Mensaje = "Ocurrió un error interno al eliminar el alumno del grupo.",
        //            Codigo = AlumnoErrorCodes.ERROR_INTERNO,
        //            Detalles = ex.Message
        //        });
        //    }
        //}

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