using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;
using NPOI.HSSF.UserModel;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static ControlActividades.Controllers.AlumnoController;
using Microsoft.Owin.Security.Google;
using Owin;

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
        public AlumnoApiController() { }

        public AlumnoApiController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext DbContext,
            FuncionalidadesGenerales fg
            )
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



        // Endpoint para unirse a una clase con código de acceso
        [HttpPost]
        [Route("UnirseAClase")]
        public async Task<IHttpActionResult> UnirseAClase([FromBody] UnirseAClaseRequest request)
        {
            if (string.IsNullOrEmpty(request.CodigoAcceso))
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = "El código de acceso es obligatorio" });
            }

            // Buscar si el código pertenece a un Grupo
            var grupo = await Db.tbGrupos.FirstOrDefaultAsync(g => g.CodigoAcceso == request.CodigoAcceso);

            if (grupo != null)
            {
                // Verificar si el alumno ya está inscrito en el grupo
                var existeRelacion = await Db.tbAlumnosGrupos
                    .AnyAsync(ag => ag.AlumnoId == request.AlumnoId && ag.GrupoId == grupo.GrupoId);

                if (!existeRelacion)
                {
                    // Agregar el alumno al grupo
                    var nuevaRelacion = new tbAlumnosGrupos
                    {
                        AlumnoId = request.AlumnoId,
                        GrupoId = grupo.GrupoId
                    };
                    Db.tbAlumnosGrupos.Add(nuevaRelacion);
                    await Db.SaveChangesAsync();
                    return Ok(new { mensaje = "Te has unido al grupo", nombre = grupo.NombreGrupo, esGrupo = true });
                }

            }

            // Buscar si el código pertenece a una Materia
            var materia = await Db.tbMaterias.FirstOrDefaultAsync(m => m.CodigoAcceso == request.CodigoAcceso);

            if (materia != null)
            {
                // Verificar si el alumno ya está inscrito en la materia
                var existeRelacion = await Db.tbAlumnosMaterias
                    .AnyAsync(am => am.AlumnoId == request.AlumnoId && am.MateriaId == materia.MateriaId);

                if (!existeRelacion)
                {
                    // Agregar el alumno a la materia
                    var nuevaRelacion = new tbAlumnosMaterias
                    {
                        AlumnoId = request.AlumnoId,
                        MateriaId = materia.MateriaId
                    };
                    Db.tbAlumnosMaterias.Add(nuevaRelacion);
                    await Db.SaveChangesAsync();
                    return Ok(new { mensaje = "Te has unido a la materia", nombre = materia.NombreMateria, esGrupo = false });
                }

            }

            return Content(HttpStatusCode.NotFound, new { mensaje = "Código de acceso no válido" });
        }

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

        [HttpPost]
        [Route("RegistrarEnvioActividadAlumno")]
        public async Task<IHttpActionResult> RegistrarEnvioActividadAlumno([FromBody] EntregableAlumno entregable)
        {
            try
            {
                var actividadId = entregable.ActividadId;
                var alumnoId = entregable.AlumnoId;
                var respuesta = entregable.Respuesta;
                var fechaEntrega = entregable.FechaEntrega;

                var fechaLimite = Db.tbActividades.Where(a => a.ActividadId == actividadId).Select(a => a.FechaLimite).FirstOrDefault();

                tbAlumnosActividades actividad = new tbAlumnosActividades()
                {
                    ActividadId = actividadId,
                    AlumnoId = alumnoId,
                    FechaEntrega = DateTime.Parse(fechaEntrega),
                    EstatusEntrega = true,
                    EntregablesAlumno = new tbEntregableAlumno()
                    {
                        Respuesta = respuesta
                    }
                };

                Db.tbAlumnosActividades.Add(actividad);

                await Db.SaveChangesAsync();


                var datosAlumnoActividad = await Db.tbAlumnosActividades.Where(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId).FirstOrDefaultAsync();


                var alumnoActividadId = datosAlumnoActividad?.AlumnoActividadId ?? 0;

                var datosEntregable = await Db.tbEntregablesAlumno.Where(a => a.AlumnoActividadId == alumnoActividadId).FirstOrDefaultAsync();

                if (datosAlumnoActividad != null && datosEntregable != null)
                {
                    int entregaId = datosEntregable.EntregaId;

                    var calificacion = await Db.tbCalificaciones.Where(a => a.EntregaId == entregaId).Select(a => a.Calificacion).FirstOrDefaultAsync();

                    return Ok(new
                    {
                        EntregaId = datosEntregable.EntregaId,
                        AlumnoActividadId = alumnoActividadId,
                        Respuesta = datosEntregable?.Respuesta ?? "",
                        Status = datosAlumnoActividad.EstatusEntrega,
                        Calificacion = calificacion
                    });
                }

                return BadRequest();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("ObtenerEnviosActividadesAlumno")]
        public async Task<IHttpActionResult> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            try
            {

                var datosAlumnoActividad = await Db.tbAlumnosActividades.Where(a => a.ActividadId == ActividadId && a.AlumnoId == AlumnoId).Select(a => new { a.AlumnoActividadId, a.FechaEntrega, a.EstatusEntrega }).FirstOrDefaultAsync();


                var alumnoActividadId = datosAlumnoActividad?.AlumnoActividadId ?? 0;

                var fechaEntrega = datosAlumnoActividad?.FechaEntrega;

                var datosEntregable = await Db.tbEntregablesAlumno.Where(a => a.AlumnoActividadId == alumnoActividadId).FirstOrDefaultAsync();

                if (datosAlumnoActividad != null && datosEntregable != null)
                {
                    int entregaId = datosEntregable.EntregaId;

                    var calificacion = await Db.tbCalificaciones.Where(a => a.EntregaId == entregaId).Select(a => a.Calificacion).FirstOrDefaultAsync();

                    return Ok(new
                    {
                        EntregaId = datosEntregable.EntregaId,
                        AlumnoActividadId = alumnoActividadId,
                        Respuesta = datosEntregable?.Respuesta ?? "",
                        Status = datosAlumnoActividad.EstatusEntrega,
                        FechaEntrega = fechaEntrega,
                        Calificacion = calificacion
                    });
                }

                return BadRequest();

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("CancelarEnvioActividadAlumno")]
        public async Task<IHttpActionResult> CancelarEnvioActividadAlumno([FromBody] CancelarEnvioActividadAlumno datosCancelacion)
        {
            try
            {
                var alumnoActividadId = datosCancelacion.AlumnoActividadId;
                var alumnoId = datosCancelacion.AlumnoId;
                var actividadId = datosCancelacion.ActividadId;


                var alumnoActividadEliminar = Db.tbAlumnosActividades.Include(a => a.EntregablesAlumno)
            .FirstOrDefault(a => a.AlumnoActividadId == alumnoActividadId && a.AlumnoId == alumnoId);

                if (alumnoActividadEliminar != null)
                {
                    if (alumnoActividadEliminar.EntregablesAlumno != null)
                    {
                        Db.tbEntregablesAlumno.Remove(alumnoActividadEliminar.EntregablesAlumno);
                    }

                    Db.tbAlumnosActividades.Remove(alumnoActividadEliminar);
                    await Db.SaveChangesAsync();

                    var datosAlumnoActividad = await Db.tbAlumnosActividades.Where(a => a.ActividadId == actividadId && a.AlumnoId == alumnoId).FirstOrDefaultAsync();


                    //var alumnoActividadId = datosAlumnoActividad?.AlumnoActividadId ?? 0;

                    var datosEntregable = await Db.tbEntregablesAlumno.Where(a => a.AlumnoActividadId == alumnoActividadId).FirstOrDefaultAsync();

                    if (datosAlumnoActividad != null && datosEntregable != null)
                    {
                        return Ok(new
                        {
                            AlumnoActividadId = alumnoActividadId,
                            Respuesta = datosEntregable?.Respuesta ?? "",
                            Status = datosAlumnoActividad.EstatusEntrega
                        });
                    }
                }

                return BadRequest();
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
                    var user = await _userManager.FindByEmailAsync(email);
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

                foreach (var email in lsEmails)
                {
                    var user = await _userManager.FindByEmailAsync(email);

                    if (user != null)
                    {
                        var alumnoId = await Db.tbAlumnos.Where(a => a.UserId == user.Id).Select(a => a.AlumnoId).FirstOrDefaultAsync();

                        lsAlumnosId.Add(alumnoId);
                    }
                }

                int grupoId = alumnoGMRegistro.GrupoId;
                int materiaId = alumnoGMRegistro.MateriaId;


                //TODO: EN CASO DE REGISTRAR UN ALUMNO A UNA MATERIA CON UN GRUPO
                //if (grupoId != 0 && materiaId != 0)
                //{
                //    foreach (var aluId in lsAlumnosId)
                //    {
                //        bool alumnoRegistradoGrupo = Db.tbAlumnosGrupos.Any(a => a.GrupoId == grupoId && a.AlumnoId == aluId);
                //        bool alumnoRegistradoMateria = Db.tbAlumnosMaterias.Any(a => a.MateriaId == materiaId && a.AlumnoId == aluId);
                //        if (!alumnoRegistradoGrupo)
                //        {
                //            AlumnosGrupos alumnosGrupos = new()
                //            {
                //                AlumnoId = aluId,
                //                GrupoId = grupoId
                //            };
                //            await Db.tbAlumnosGrupos.AddAsync(alumnosGrupos);
                //        }
                //        else
                //        {
                //            BadRequest(new { mensaje = "El alumno ya esta registrado" });
                //        }

                //        if (!alumnoRegistradoMateria)
                //        {
                //            AlumnosMaterias alumnosMaterias = new()
                //            {
                //                AlumnoId = aluId,
                //                MateriaId = materiaId
                //            };
                //            await Db.tbAlumnosMaterias.AddAsync(alumnosMaterias);
                //        }
                //        else
                //        {
                //            BadRequest(new { mensaje = "El alumno ya esta registrado" });
                //        }
                //    }
                //    Db.SaveChanges();

                //    var lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);

                //    return Ok(lsAlumnos);
                //}
                //else 
                if (grupoId != 0)
                {
                    docenteId = await Db.tbGrupos.Where(a => a.GrupoId == grupoId).Select(a => a.DocenteId).FirstOrDefaultAsync();
                    foreach (var aluId in lsAlumnosId)
                    {
                        bool alumnoYaRegistrado = Db.tbAlumnosGrupos.Any(a => a.GrupoId == grupoId && a.AlumnoId == aluId);
                        if (!alumnoYaRegistrado)
                        {
                            tbAlumnosGrupos alumnosGrupos = new tbAlumnosGrupos()
                            {
                                AlumnoId = aluId,
                                GrupoId = grupoId
                            };

                            Db.tbAlumnosGrupos.Add(alumnosGrupos);
                        }
                        else
                        {
                            Content(HttpStatusCode.BadRequest, new { mensaje = "El alumno ya esta registrado" });
                        }
                    }
                    await Db.SaveChangesAsync();
                    alumnoRegistradoGrupo = true;
                    var lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);
                    return Ok(lsAlumnos);
                }
                else if (materiaId != 0)
                {
                    docenteId = await Db.tbMaterias.Where(a => a.MateriaId == materiaId).Select(a => a.DocenteId).FirstOrDefaultAsync();
                    foreach (var aluId in lsAlumnosId)
                    {
                        bool alumnoYaRegistrado = Db.tbAlumnosMaterias.Any(a => a.MateriaId == materiaId && a.AlumnoId == aluId);
                        if (!alumnoYaRegistrado)
                        {
                            tbAlumnosMaterias alumnosMaterias = new tbAlumnosMaterias()
                            {
                                AlumnoId = aluId,
                                MateriaId = materiaId
                            };
                            Db.tbAlumnosMaterias.Add(alumnosMaterias);
                        }
                        else
                        {
                            return Content(HttpStatusCode.BadRequest, new { mensaje = "El alumno ya esta registrado" });
                        }
                    }
                    await Db.SaveChangesAsync();
                    alumnoRegistradoMateria = true;
                    var lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);

                    return Ok(lsAlumnos);
                }


                //TODO: Retornar UserName, Nombre, Apellido Paterno, ApellidoMaterno
                //return Ok(new { mensaje = "El alumno fue agregado correctamente" });
                return BadRequest();
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
                    //await _ns.NotificacionRegistrarAlumnoClase(lsAlumnosId, docenteId, grupoId: grupoId);
                }
                else if (alumnoRegistradoMateria)
                {
                    //await _ns.NotificacionRegistrarAlumnoClase(lsAlumnosId, docenteId, materiaId: materiaId);
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

                List<int> lsAlumnosId = await Db.tbAlumnosGrupos.Where(a => a.GrupoId == grupoId).Select(a => a.AlumnoId).ToListAsync();

                List<EmailVerificadoAlumno> lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);

                return Ok(lsAlumnos);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = e.Message });
            }
        }

        [HttpPost]
        [Route("ObtenerListaAlumnosMateria")]
        public async Task<IHttpActionResult> ObtenerListaAlumnosMateria([FromBody] Indices indice)
        {
            try
            {
                int grupoId = indice.GrupoId;
                int materiaId = indice.MateriaId;

                if (grupoId > 0 && materiaId > 0)
                {
                    List<int> lsAlumnosGruposId = await Db.tbAlumnosGrupos.Where(a => a.GrupoId == grupoId).Select(a => a.AlumnoId).ToListAsync();
                    List<EmailVerificadoAlumno> lsAlumnos = await ObtenerListaAlumnos(lsAlumnosGruposId);
                    return Ok(lsAlumnos);
                }
                else
                {
                    List<int> lsAlumnosId = await Db.tbAlumnosMaterias.Where(a => a.MateriaId == materiaId).Select(a => a.AlumnoId).ToListAsync();

                    List<EmailVerificadoAlumno> lsAlumnos = await ObtenerListaAlumnos(lsAlumnosId);

                    return Ok(lsAlumnos);
                }
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = e.Message });
            }
        }

        private async Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnos(List<int> lsAlumnosId)
        {
            try
            {
                var lsAlumnos = new List<EmailVerificadoAlumno>();
                foreach (var id in lsAlumnosId)
                {
                    var alumnoDatos = Db.tbAlumnos.Where(a => a.AlumnoId == id).FirstOrDefault();
                    if (alumnoDatos != null)
                    {
                        // usar la propiedad que garantiza obtener el UserManager
                        var user = await UserManager.FindByIdAsync(alumnoDatos.UserId);

                        var alumno = new EmailVerificadoAlumno()
                        {
                            Email = user?.Email ?? "",
                            UserName = user?.UserName ?? "",
                            Nombre = alumnoDatos.Nombre,
                            ApellidoPaterno = alumnoDatos.ApellidoPaterno,
                            ApellidoMaterno = alumnoDatos.ApellidoMaterno,
                        };

                        lsAlumnos.Add(alumno);
                    }
                }
                return lsAlumnos;
            }
            catch (Exception)
            {
                return new List<EmailVerificadoAlumno>();
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

        [HttpPost]
        [Route("ImportarAlumnosExcel")]
        public async Task<IHttpActionResult> ImportarAlumnosExcel()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest == null || httpRequest.Files.Count == 0)
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "No se recibió archivo." });

                var file = httpRequest.Files[0];
                if (file == null || file.ContentLength == 0)
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "Archivo vacío." });

                // Leer parámetros opcionales
                int grupoId = 0;
                int materiaId = 0;
                int.TryParse(httpRequest.Form["GrupoId"], out grupoId);
                int.TryParse(httpRequest.Form["MateriaId"], out materiaId);

                if (grupoId == 0 && materiaId == 0)
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "Debe enviar GrupoId o MateriaId." });

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
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "Hoja no encontrada en el archivo." });

                // Detectar si la primera fila es encabezado (contiene "email")
                int startRow = sheet.FirstRowNum;
                var headerRow = sheet.GetRow(startRow);
                bool hasHeader = false;
                if (headerRow != null)
                {
                    // revisar todas las celdas del encabezado por si "email" está en otra columna
                    var headerCells = headerRow.LastCellNum > 0 ? headerRow.LastCellNum : 1;
                    for (int hc = 0; hc < headerCells; hc++)
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
                for (int r = hasHeader ? startRow + 1 : startRow; r <= sheet.LastRowNum; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;

                    string found = null;
                    // recorrer celdas de la fila y buscar una que parezca un email
                    var lastCell = row.LastCellNum > 0 ? row.LastCellNum : 1;
                    for (int c = 0; c < lastCell; c++)
                    {
                        var cell = row.GetCell(c);
                        if (cell == null) continue;
                        var cellText = formatter.FormatCellValue(cell)?.Trim();
                        if (string.IsNullOrWhiteSpace(cellText)) continue;
                        // Si la celda contiene un correo, tomarla
                        if (cellText.Contains("@"))
                        {
                            found = cellText;
                            break;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(found)) continue;

                    // normalizar: quitar espacios y convertir a minúsculas
                    var emailNormalized = found.Trim().ToLowerInvariant();
                    // validación básica
                    if (!emailNormalized.Contains("@")) continue;

                    emails.Add(emailNormalized);
                }

                if (!emails.Any())
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "No se encontraron emails en el archivo. Asegúrese que la primera fila tenga una columna con correos o que las celdas contengan emails." });

                var added = new List<string>();
                var skipped = new List<string>();
                var notFound = new List<string>();
                var lsAlumnosId = new List<int>();

                foreach (var email in emails.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var user = await UserManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        notFound.Add(email);
                        continue;
                    }

                    var alumnoId = await Db.tbAlumnos.Where(a => a.UserId == user.Id).Select(a => a.AlumnoId).FirstOrDefaultAsync();
                    if (alumnoId == 0)
                    {
                        notFound.Add(email);
                        continue;
                    }

                    lsAlumnosId.Add(alumnoId);

                    if (grupoId > 0)
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
                    else if (materiaId > 0)
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

                await Db.SaveChangesAsync();

                var alumnos = await (from a in Db.tbAlumnos
                     where lsAlumnosId.Contains(a.AlumnoId)
                     join u in Db.Users on a.UserId equals u.Id into uj
                     from u in uj.DefaultIfEmpty()
                     select new EmailVerificadoAlumno {
                        Email = u.Email ?? "",
                        UserName = u.UserName ?? "",
                        Nombre = a.Nombre,
                        ApellidoPaterno = a.ApellidoPaterno,
                        ApellidoMaterno = a.ApellidoMaterno
                     }).ToListAsync();

                return Ok(new
                {
                    TotalLeidos = emails.Count,
                    Agregados = added,
                    Omitidos = skipped,
                    NoEncontrados = notFound,
                    Alumnos = alumnos
                });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = ex.Message });
            }
        }
    }
}