using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/Materias")]
    public class MateriasApiController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        public MateriasApiController()
        {
        }

        public MateriasApiController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg)
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


        #region Docente
        private static string ObtenerClave()
        {
            int length = 8;

            StringBuilder str_build = new StringBuilder();
            Random random = new Random();

            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }

            return str_build.ToString();
        }


        public async Task<List<object>> ConsultaGrupos()
        {
            try
            {
                var lsGrupos = await Db.tbGrupos.ToListAsync();


                var listaGruposMaterias = new List<object>();
                foreach (var grupo in lsGrupos)
                {
                    var lsMateriasId = await Db.tbGruposMaterias.Where(a => a.GrupoId == grupo.GrupoId).Select(a => a.MateriaId).ToListAsync();

                    var lsMaterias = await Db.tbMaterias.Where(a => lsMateriasId.Contains(a.MateriaId)).Select(m => new
                    {
                        m.MateriaId,
                        m.NombreMateria,
                        m.Descripcion,
                        //m.CodigoColor,
                        Actividades = Db.tbActividades.Where(a => a.MateriaId == m.MateriaId).ToList()
                    }).ToListAsync();


                    listaGruposMaterias.Add(new
                    {
                        GrupoId = grupo.GrupoId,
                        NombreGrupo = grupo.NombreGrupo,
                        Descripcion = grupo.Descripcion,
                        CodigoAcceso = grupo.CodigoAcceso,
                        CodigoColor = grupo.CodigoColor,
                        Materias = lsMaterias
                    });
                }

                return listaGruposMaterias;
            }
            catch (Exception)
            {
                return new List<object>();
            }
        }


        public async Task<List<tbMaterias>> ConsultaMaterias()
        {
            try
            {
                var lsGruposMaterias = await Db.tbGruposMaterias.Select(a => a.MateriaId).ToListAsync();

                var lsMateriasSinGrupo = await Db.tbMaterias.Where(a => !lsGruposMaterias.Contains(a.MateriaId)).ToListAsync();

                return lsMateriasSinGrupo;
            }
            catch (Exception)
            {
                return new List<tbMaterias>();
            }
        }

        public async Task<List<tbMaterias>> ConsultaMateriasPorDocente(int docenteId)
        {
            try
            {
                var lsGruposMaterias = await Db.tbGruposMaterias.Select(a => a.MateriaId).ToListAsync();

                var lsMateriasSinGrupo = await Db.tbMaterias
                    .Where(a => a.DocenteId == docenteId && !lsGruposMaterias.Contains(a.MateriaId))
                    .ToListAsync();

                return lsMateriasSinGrupo;
            }
            catch (Exception)
            {
                return new List<tbMaterias>();
            }
        }

        //ObtenerMateriasSinGrupoDocente
        [HttpGet]
        [Route("ObtenerMateriasDocente")]
        public async Task<IHttpActionResult> ObtenerMateriasDocente(int docenteId)
        {
            try
            {
                List<int> lsMateriasId = await Db.tbMaterias.Where(a => a.DocenteId == docenteId).Select(a => a.MateriaId).ToListAsync();

                List<int> lsGruposMateriasId = await Db.tbGruposMaterias.Where(a => lsMateriasId.Contains(a.MateriaId)).Select(a => a.MateriaId).ToListAsync();

                lsMateriasId = lsMateriasId.Where(a => !lsGruposMateriasId.Contains(a)).ToList();

                var lsMaterias = Db.tbMaterias.Where(a => lsMateriasId.Contains(a.MateriaId)).Select(a => new
                {
                    a.MateriaId,
                    a.NombreMateria,
                    Actividades = Db.tbActividades.Where(b => b.MateriaId == a.MateriaId).Select(b=> new
                    {
                        b.ActividadId,
                        b.NombreActividad,
                        b.Descripcion,
                        b.FechaCreacion,
                        b.FechaLimite,
                        b.TipoActividadId,
                        b.Puntaje,
                        b.MateriaId,
                    }).ToList()
                }).ToList();

                return Ok(lsMaterias);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest,new
                {
                    e.Message
                });
            }
        }


        [HttpGet]
        [Route("ObtenerMaterias")]
        public async Task<IHttpActionResult> ObtenerMaterias()
        {
            try
            {
                var materias = await ConsultaMaterias();

                if (materias == null || !materias.Any())
                    return NotFound();

                return Ok(materias);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.BadRequest, new
                {
                    mensaje = "Hubo un error en ObtenerMaterias"
                });
            }
        }


        [HttpGet]
        [Route("ObtenerMateriaUnica")]
        public async Task<IHttpActionResult> ObtenerMateriaUnica(int id)
        {
            var subject = await Db.tbMaterias.FindAsync(id);
            if (subject is null) return Content(HttpStatusCode.NotFound,"Materia no encontrado");

            return Ok(subject);
        }


        [HttpPost]
        [Route("CrearMateriaSinGrupo")]
        public async Task<IHttpActionResult> CrearMateriaSinGrupo([FromBody] tbMaterias materia)
        {
            try
            {
                int docenteId = materia.DocenteId;
                materia.CodigoAcceso = ObtenerClave();


                Db.tbMaterias.Add(materia);
                await Db.SaveChangesAsync();


                var lsMateriasDocente = await Db.tbMaterias.Where(a => a.DocenteId == docenteId
                && !Db.tbGruposMaterias.Any(b => b.MateriaId == a.MateriaId)).Select(a => new
                {
                    a.MateriaId, 
                    a.NombreMateria,
                    a.Descripcion,
                    a.CodigoColor,
                    a.CodigoAcceso,
                    a.DocenteId
                }).ToListAsync();

                return Ok(lsMateriasDocente);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.BadRequest,new { mensaje = "No se registro la materia" });
            }
        }

        /*
        [HttpPost]
        [Route("CrearMateriaGrupos")]
        public async Task<IHttpActionResult> CrearMateriaGrupos([FromBody] MateriaConGrupo materiaConGrupo)
        {
            try
            {
                int docenteId = materiaConGrupo.DocenteId;
                var lsGruposId = Db.tbGrupos.Where(a => a.DocenteId == docenteId).Select(a => a.GrupoId).ToList();
                List<int> gruposVinculados = materiaConGrupo.Grupos;
                if (gruposVinculados.All(a => lsGruposId.Contains(a)))
                {

                    tbMaterias materia = new tbMaterias()
                    {
                        DocenteId = docenteId,
                        NombreMateria = materiaConGrupo.NombreMateria,
                        Descripcion = materiaConGrupo.Descripcion,
                        CodigoAcceso = ObtenerClave()
                        //CodigoColor = materiaG.CodigoColor,
                    };


                    Db.tbMaterias.Add(materia);
                    await Db.SaveChangesAsync();



                    var idMateria = materia.MateriaId;


                    foreach (var grupo in gruposVinculados)
                    {

                        tbGruposMaterias gruposMaterias = new tbGruposMaterias()
                        {
                            GrupoId = grupo,
                            MateriaId = idMateria
                        };

                        Db.tbGruposMaterias.Add(gruposMaterias);

                    }
                    await Db.SaveChangesAsync();

                    var lsGruposMaterias = await ConsultaGrupos();

                    return Ok(lsGruposMaterias);
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest,new { mensaje = "Un grupo no pertenece al docente" });
                }
            }
            catch (DbUpdateException ex)
            {
                // Captura la excepción interna para más detalles
                var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Content(HttpStatusCode.InternalServerError, $"Internal server error: {innerException}");
            }
        }
        */

        // Funcion modificada para ver logs detallados del problema
        [HttpPost]
        [Route("CrearMateriaGrupos")]
        public async Task<IHttpActionResult> CrearMateriaGrupos([FromBody] MateriaConGrupo materiaConGrupo)
        {
            try
            {
                Console.WriteLine($"[LOG] Iniciando CrearMateriaGrupos. DocenteId: {materiaConGrupo.DocenteId}, NombreMateria: {materiaConGrupo.NombreMateria}");

                int docenteId = materiaConGrupo.DocenteId;
                var lsGruposId = Db.tbGrupos.Where(a => a.DocenteId == docenteId).Select(a => a.GrupoId).ToList();
                Console.WriteLine($"[LOG] Grupos del docente {docenteId}: {string.Join(", ", lsGruposId)}");

                List<int> gruposVinculados = materiaConGrupo.Grupos;
                Console.WriteLine($"[LOG] Grupos a vincular: {string.Join(", ", gruposVinculados)}");

                if (gruposVinculados.All(a => lsGruposId.Contains(a)))
                {
                    Console.WriteLine("[LOG] Verificación de grupos pasada. Todos los grupos pertenecen al docente.");

                    tbMaterias materia = new tbMaterias()
                    {
                        DocenteId = docenteId,
                        NombreMateria = materiaConGrupo.NombreMateria,
                        Descripcion = materiaConGrupo.Descripcion,
                        CodigoAcceso = ObtenerClave()
                        //CodigoColor = materiaG.CodigoColor,
                    };

                    Console.WriteLine($"[LOG] Creando materia con CodigoAcceso: {materia.CodigoAcceso}");
                    Db.tbMaterias.Add(materia);
                    await Db.SaveChangesAsync();
                    Console.WriteLine($"[LOG] Materia creada exitosamente. MateriaId: {materia.MateriaId}");

                    var idMateria = materia.MateriaId;

                    foreach (var grupo in gruposVinculados)
                    {
                        Console.WriteLine($"[LOG] Vinculando materia {idMateria} con grupo {grupo}");
                        tbGruposMaterias gruposMaterias = new tbGruposMaterias()
                        {
                            GrupoId = grupo,
                            MateriaId = idMateria
                        };

                        Db.tbGruposMaterias.Add(gruposMaterias);
                    }

                    await Db.SaveChangesAsync();
                    Console.WriteLine("[LOG] Vinculaciones guardadas exitosamente.");

                    Console.WriteLine("[LOG] Consultando grupos actualizados...");
                    var lsGruposMaterias = await ConsultaGrupos();
                    Console.WriteLine($"[LOG] Consulta completada. Retornando {lsGruposMaterias?.Count ?? 0} grupos.");

                    //return Ok(lsGruposMaterias);
                    return Ok(new { mensaje = "Materia creada exitosamente", materiaId = idMateria });
                }
                else
                {
                    Console.WriteLine("[LOG] ERROR: Uno o más grupos no pertenecen al docente.");
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "Un grupo no pertenece al docente" });
                }
            }
            catch (DbUpdateException ex)
            {
                // Captura la excepción interna para más detalles
                var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Console.WriteLine($"[LOG] ERROR DbUpdateException: {innerException}");
                return Content(HttpStatusCode.InternalServerError, $"Internal server error: {innerException}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOG] ERROR General: {ex.Message}");
                Console.WriteLine($"[LOG] StackTrace: {ex.StackTrace}");
                return Content(HttpStatusCode.InternalServerError, $"Unexpected error: {ex.Message}");
            }
        }



        [HttpPut]
        [Route("UpdateSubject")]
        public async Task<IHttpActionResult> UpdateSubject(tbMaterias updatedSubject)
        {
            var dbSubject = await Db.tbMaterias.FindAsync(updatedSubject.MateriaId);
            if (dbSubject is null) return Content(HttpStatusCode.NotFound, "Materia no encontrado");


            dbSubject.NombreMateria = updatedSubject.NombreMateria;
            dbSubject.Descripcion = updatedSubject.Descripcion;

            await Db.SaveChangesAsync();
            return Ok(await Db.tbMaterias.ToListAsync());
        }

        /*
        [HttpDelete]
        [Route("DeleteSubject/{id}")]
        public async Task<IHttpActionResult> DeleteSubject(int id)
        {
            try
            {
                var dbSubject = await Db.tbMaterias.FindAsync(id);
                if (dbSubject is null) return Content(HttpStatusCode.NotFound, "Materia no encontrada");

                Db.tbMaterias.Remove(dbSubject);
                await Db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        */

        // Nueva funcion para eliminar materias, primero elimina actividadaes asociadas
        [HttpDelete]
        [Route("DeleteSubject/{id}")]
        public async Task<IHttpActionResult> DeleteSubject(int id)
        {
            try
            {
                var dbSubject = await Db.tbMaterias.FindAsync(id);
                if (dbSubject == null) return Content(HttpStatusCode.NotFound, "Materia no encontrada");

                // 1. Eliminar entregables relacionados con las actividades de esta materia
                var activities = Db.tbActividades.Where(a => a.MateriaId == id).Select(a => a.ActividadId);
                var submissions = Db.tbEntregablesAlumno.Where(e => activities.Contains(e.AlumnoActividadId));
                Db.tbEntregablesAlumno.RemoveRange(submissions);

                // 2. Eliminar actividades
                var subjectActivities = Db.tbActividades.Where(a => a.MateriaId == id);
                Db.tbActividades.RemoveRange(subjectActivities);

                // 3. Eliminar registros de alumnos-materias
                var alumnosMaterias = Db.tbAlumnosMaterias.Where(am => am.MateriaId == id);
                Db.tbAlumnosMaterias.RemoveRange(alumnosMaterias);

                // 4. Eliminar relaciones con grupos (esto removerá la materia de todos los grupos)
                var gruposMaterias = Db.tbGruposMaterias.Where(gm => gm.MateriaId == id);
                Db.tbGruposMaterias.RemoveRange(gruposMaterias);

                // 5. Eliminar la materia
                Db.tbMaterias.Remove(dbSubject);
                await Db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar materia: {ex.Message}");
                return BadRequest();
            }
        }




        #endregion

        #region Alumno

        [HttpGet]
        [Route("ObtenerMateriasAlumno")]
        public async Task<IHttpActionResult> ObtenerMateriasAlumno(int alumnoId)
        {
            try
            {
                var lsMateriasAlumnoId = Db.tbAlumnosMaterias.Where(a => a.AlumnoId == alumnoId).Select(a => a.MateriaId);

                var lsMateriasSinGrupo = Db.tbMaterias.Where(a => lsMateriasAlumnoId.Contains(a.MateriaId)).Select(a => new
                {
                    a.MateriaId,
                    a.NombreMateria,
                    a.Descripcion,
                    Actividades = Db.tbActividades.Where(b => b.MateriaId == a.MateriaId).Select(b => new
                    {
                        b.ActividadId,
                        b.TipoActividadId,
                        b.NombreActividad,
                        b.Descripcion,
                        b.FechaCreacion,
                        b.FechaLimite,
                        b.Puntaje,
                        b.MateriaId
                    }).ToList()
                }).ToList();

                //foreach (var materia in lsMateriasSinGrupo)
                //{
                //    var laMaterias = lsMateriasSinGrupo.Select(a => new
                //    {
                //        a.MateriaId,
                //        a.NombreMateria,
                //        a.Descripcion,
                //        actividades = Db.tbActividades.Where(b => b.MateriaId == a.MateriaId).ToList()
                //    });

                //    lsMateriasActividades.Add(laMaterias);
                //}

                return Ok(lsMateriasSinGrupo);
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest,new
                {
                    e.Message
                });
            }
        }

        #endregion



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
