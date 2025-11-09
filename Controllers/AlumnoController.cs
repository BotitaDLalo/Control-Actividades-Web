using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ControlActividades.Models;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ControlActividades.Controllers
{
    [Authorize]
    public class AlumnoController : Controller
    {
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

        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();

            var alumnoId = Db.tbAlumnos.Where(a => a.UserId == userId).Select(a => a.AlumnoId).FirstOrDefault();

            ViewBag.AlumnoId = alumnoId;

            return View();
        }




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
            return View();
        }

        public ActionResult DetalleGrupo()
        {
            return View();
        }

        public async Task<ActionResult> Avisos(int alumnoId)
        {
            ViewBag.AlumnoId = alumnoId;
            var avisos = await Db.tbAvisos
                .Where(a => Db.tbAlumnosGrupos.Any(ag => ag.AlumnoId == alumnoId && ag.GrupoId == a.GrupoId)
                         || Db.tbAlumnosMaterias.Any(am => am.AlumnoId == alumnoId && am.MateriaId == a.MateriaId))
                .ToListAsync();
            return PartialView("_Avisos", avisos);
        }



        [HttpGet]
        public async Task<ActionResult> ObtenerAvisos(int alumnoId)
        {
            try
            {
                var avisosDb = await Db.tbAvisos
                    .Where(a => Db.tbAlumnosGrupos.Any(ag => ag.AlumnoId == alumnoId && ag.GrupoId == a.GrupoId)
                             || Db.tbAlumnosMaterias.Any(am => am.AlumnoId == alumnoId && am.MateriaId == a.MateriaId))
                    .ToListAsync();

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



        public ActionResult Actividades()
        {
            return PartialView("_Actividades");
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
    }
}
