using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/Avisos")]
    public class AvisosApiController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private NotificacionesService _notifServ;
        public AvisosApiController() { }

        public AvisosApiController(ApplicationUserManager userManager,
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

        // Metodo para crear avisos original
        /*
        [HttpPost]
        [Route("CrearAviso")]
        public async Task<IHttpActionResult> CrearAviso([FromBody] PeticionCrearAviso crearAviso)
        {
            bool avisoCreado = false;

            DateTime dateTime = DateTime.Now;
            tbAvisos avisos = new tbAvisos
            {
                DocenteId = crearAviso.DocenteId,
                Titulo = crearAviso.Titulo,
                Descripcion = crearAviso.Descripcion,
                FechaCreacion = dateTime,
            };

            try
            {
                var materiaId = crearAviso.MateriaId;
                var grupoId = crearAviso.GrupoId;
                if (grupoId != null)
                {
                    avisos.GrupoId = grupoId;
                }
                else if (materiaId != null)
                {
                    avisos.MateriaId = materiaId;
                }

                Db.tbAvisos.Add(avisos);
                await Db.SaveChangesAsync();
                avisoCreado = true;

                var nuevoAviso = Db.tbAvisos.Where(a => a.AvisoId == avisos.AvisoId).FirstOrDefault();

                var docenteNombre = Db.tbDocentes.Where(a => a.DocenteId == nuevoAviso.DocenteId).Select(a => new
                {
                    a.ApellidoPaterno,
                    a.ApellidoMaterno,
                    a.Nombre
                }).FirstOrDefault();

                var res = new
                {
                    AvisoId = nuevoAviso.AvisoId,
                    Titulo = nuevoAviso.Titulo,
                    Descripcion = nuevoAviso.Descripcion,
                    ApePaternoDocente = docenteNombre.ApellidoPaterno,
                    ApeMaternoDocente = docenteNombre.ApellidoMaterno,
                    NombresDocente = docenteNombre.Nombre,
                    FechaCreacion = nuevoAviso.FechaCreacion,
                    GrupoId = nuevoAviso.GrupoId,
                    MateriaId = nuevoAviso.MateriaId
                };

                return Ok(res);
            }
            catch (Exception)
            {
                return BadRequest();
            }
            finally
            {
                if (avisoCreado)
                {
                    var materiaId = crearAviso.MateriaId;
                    var grupoId = crearAviso.GrupoId;
                    await Ns.NotificacionCrearAviso(avisos, grupoId, materiaId);
                }
            }
        }
        */

        /*
         * Metodo para crear avisos modificado
            Cambios realizados:
                ✅ Envolví await Ns.NotificacionCrearAviso() en try-catch dentro del finally
                ✅ Si la notificación falla, se captura la excepción pero no afecta la respuesta HTTP
                ✅ El return Ok(res) se ejecuta correctamente sin interferencias
            Resultado:
                ✅ Respuesta HTTP siempre será exitosa (200 OK) cuando el aviso se cree
                ✅ Las notificaciones se intentan enviar pero no bloquean la respuesta
                ✅ El frontend recibirá status 200 y procesará correctamente
                ✅ No más errores 500 que causen problemas en el móvil
        */
        [HttpPost]
        [Route("CrearAviso")]
        public async Task<IHttpActionResult> CrearAviso([FromBody] PeticionCrearAviso crearAviso)
        {
            bool avisoCreado = false;

            DateTime dateTime = DateTime.Now;
            tbAvisos avisos = new tbAvisos
            {
                DocenteId = crearAviso.DocenteId,
                Titulo = crearAviso.Titulo,
                Descripcion = crearAviso.Descripcion,
                FechaCreacion = dateTime,
            };

            try
            {
                var materiaId = crearAviso.MateriaId;
                var grupoId = crearAviso.GrupoId;
                if (grupoId != null)
                {
                    avisos.GrupoId = grupoId;
                }
                else if (materiaId != null)
                {
                    avisos.MateriaId = materiaId;
                }

                Db.tbAvisos.Add(avisos);
                await Db.SaveChangesAsync();
                avisoCreado = true;

                var nuevoAviso = Db.tbAvisos.Where(a => a.AvisoId == avisos.AvisoId).FirstOrDefault();

                var docenteNombre = Db.tbDocentes.Where(a => a.DocenteId == nuevoAviso.DocenteId).Select(a => new
                {
                    a.ApellidoPaterno,
                    a.ApellidoMaterno,
                    a.Nombre
                }).FirstOrDefault();

                var res = new
                {
                    AvisoId = nuevoAviso.AvisoId,
                    Titulo = nuevoAviso.Titulo,
                    Descripcion = nuevoAviso.Descripcion,
                    ApePaternoDocente = docenteNombre.ApellidoPaterno,
                    ApeMaternoDocente = docenteNombre.ApellidoMaterno,
                    NombresDocente = docenteNombre.Nombre,
                    FechaCreacion = nuevoAviso.FechaCreacion,
                    GrupoId = nuevoAviso.GrupoId,
                    MateriaId = nuevoAviso.MateriaId
                };

                return Ok(res);
            }
            catch (Exception)
            {
                return BadRequest();
            }
            finally
            {
                if (avisoCreado)
                {
                    var materiaId = crearAviso.MateriaId;
                    var grupoId = crearAviso.GrupoId;
                    try
                    {
                        await Ns.NotificacionCrearAviso(avisos, grupoId, materiaId);
                    }
                    catch (Exception ex)
                    {
                        // Log del error pero NO fallar la respuesta HTTP
                        // El aviso ya se creó exitosamente
                        // Puedes agregar logging aquí: Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }



        [HttpGet]
        [Route("ConsultarAvisosCreados")]
        public IHttpActionResult ConsultarAvisos([FromBody] PeticionConsultarAvisos consultarAvisos)
        {
            try
            {
                List<RespuestaConsultarAvisos> lsResAvisos = new List<RespuestaConsultarAvisos>();
                List<tbAvisos> lsAvisos = new List<tbAvisos>();
                int grupoId = consultarAvisos.GrupoId;
                int materiaId = consultarAvisos.MateriaId;

                if (grupoId != 0)
                {
                    lsAvisos = Db.tbAvisos.Where(a => a.GrupoId == grupoId).ToList();
                }
                else if (materiaId != 0)
                {
                    lsAvisos = Db.tbAvisos.Where(a => a.MateriaId == materiaId).ToList();
                }

                foreach (var aviso in lsAvisos)
                {
                    int docenteId = aviso.DocenteId;
                    var docente = Db.tbDocentes.Where(a => a.DocenteId == docenteId)
                        .Select(a => new
                        {
                            a.Nombre,
                            a.ApellidoPaterno,
                            a.ApellidoMaterno
                        }).FirstOrDefault();

                    RespuestaConsultarAvisos resAviso = new RespuestaConsultarAvisos
                    {
                        AvisoId = aviso.AvisoId,
                        Titulo = aviso.Titulo,
                        Descripcion = aviso.Descripcion,
                        NombresDocente = docente != null ? docente.Nombre : "",
                        ApePaternoDocente = docente != null ? docente.ApellidoPaterno : "",
                        ApeMaternoDocente = docente != null ? docente.ApellidoMaterno : "",
                        FechaCreacion = aviso.FechaCreacion,
                        GrupoId = aviso.GrupoId ?? 0,
                        MateriaId = aviso.MateriaId ?? 0
                    };

                    lsResAvisos.Add(resAviso);
                }

                return Ok(lsResAvisos.AsEnumerable().Reverse().ToList());
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        public class AvisoDto
        {
            // 🚨 IMPORTANTE: Asegúrate de que los nombres de las propiedades coincidan EXACTAMENTE 
            // con el JSON que envías desde Dart (AvisoId, Titulo, Descripcion)
            public int AvisoId { get; set; }
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            // Si hay otros campos (como DocenteId, GrupoId, etc.) que necesita el ORM 
            // para la validación, inclúyelos si el NoticeModel de Dart los envía.
            // Por ahora, solo usamos los que se editan y el ID.
        }


        [HttpPost]
        [Route("ActualizarAviso")]
        public async Task<IHttpActionResult> ActualizarAviso(AvisoDto avisoActualizado)
        {
            try
            {
                if (avisoActualizado.AvisoId <= 0)
                {
                    return BadRequest("ID de aviso no válido.");
                }

                // 1. Usamos FindAsync igual que en tu método ActualizarActividad
                var dbAviso = await Db.tbAvisos.FindAsync(avisoActualizado.AvisoId);

                if (dbAviso == null)
                {
                    return Content(HttpStatusCode.NotFound, "Aviso no encontrado");
                }

                // 2. Actualización de campos
                dbAviso.Titulo = avisoActualizado.Titulo;
                dbAviso.Descripcion = avisoActualizado.Descripcion;

                // 3. Guardar cambios
                await Db.SaveChangesAsync();

                // 4. Proyección Limpia (Igual que en ActualizarActividad)
                // Esto evita el Error 500 por referencias circulares.
                var respuestaLimpia = new
                {
                    AvisoId = dbAviso.AvisoId,
                    Titulo = dbAviso.Titulo,
                    Descripcion = dbAviso.Descripcion,
                    DocenteId = dbAviso.DocenteId,
                    GrupoId = dbAviso.GrupoId,
                    MateriaId = dbAviso.MateriaId,
                    FechaCreacion = dbAviso.FechaCreacion,
                    // Si tienes el nombre del docente disponible en el objeto cargado (por caché de EF), lo enviamos.
                    // Si no, enviamos un string genérico o nulo, ya que Flutter probablemente ya tiene el nombre.
               
                };

                return Ok(respuestaLimpia);
            }
            catch (Exception e)
            {
                return BadRequest("Error al actualizar: " + e.Message);
            }
        }


        [HttpPost]
        [Route("EliminarAviso")]
        public async Task<IHttpActionResult> EliminarAviso(int avisoId)
        {
            try
            {
                var aviso = await Db.tbAvisos.FindAsync(avisoId);

                if (aviso == null)
                {
                    return BadRequest();
                }

                Db.tbAvisos.Remove(aviso);
                await Db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
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
