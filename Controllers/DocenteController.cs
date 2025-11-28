using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace ControlActividades.Controllers
{
    [Authorize]
    public class DocenteController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;

        public DocenteController()
        {
        }
        public DocenteController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg)
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
        // GET: Docente
        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();
            var docenteId = Db.tbDocentes.Where(a => a.UserId == userId).Select(a => a.DocenteId).FirstOrDefault();

            ViewBag.DocenteId = docenteId;
            // Propagar la sección solicitada (si viene en query string) para que la vista la pueda usar
            ViewBag.Seccion = Request.QueryString["seccion"] ?? string.Empty;

            return View();
        }

        public ActionResult Perfil()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CrearAviso(tbAvisos aviso)
        {
            if (ModelState.IsValid)
            {
                aviso.FechaCreacion = DateTime.Now;
                //Db.tbAvisos.Add(aviso); 
                Db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(aviso);
        }

        
        public ActionResult MateriasDetalles(int? materiaId)
        {
            if (!materiaId.HasValue)
            {
                
                return RedirectToAction("Index");
            }

            string userId = User.Identity.GetUserId();
            var docenteId = Db.tbDocentes.Where(a => a.UserId == userId).Select(a => a.DocenteId).FirstOrDefault();

            ViewBag.DocenteId = docenteId;
            ViewBag.MateriaId = materiaId.Value;

            return View();
        }

        // GET: /Docente/GrupoMaterias -> vista que muestra materias de un grupo
        [HttpGet]
        public ActionResult GrupoMaterias(int? grupoId)
        {
            if (!grupoId.HasValue)
            {
                return RedirectToAction("Grupos");
            }

            string userId = User.Identity.GetUserId();
            var docenteId = Db.tbDocentes.Where(a => a.UserId == userId).Select(a => a.DocenteId).FirstOrDefault();

            ViewBag.DocenteId = docenteId;
            ViewBag.GrupoId = grupoId.Value;

            return View();
        }

        public ActionResult EvaluarActividades()
        {
            string userId = User.Identity.GetUserId();
            var docenteId = Db.tbDocentes.Where(a => a.UserId == userId).Select(a => a.DocenteId).FirstOrDefault();

            ViewBag.DocenteId = docenteId;
            return View();
        }

        // GET: /Docente/Grupos -> mostrar vista independiente GruposStandalone
        [HttpGet]
        public ActionResult Grupos()
        {
            string userId = User.Identity.GetUserId();
            var docenteId = Db.tbDocentes.Where(a => a.UserId == userId).Select(a => a.DocenteId).FirstOrDefault();

            ViewBag.DocenteId = docenteId;
            // Devolver la vista independiente que no choque con otros archivos Grupos.cshtml
            return View("GruposStandalone");
        }

        // GET: /Docente/MateriasSinGrupo -> mostrar vista independiente MateriasSinGrupoStandalone
        [HttpGet]
        public ActionResult MateriasSinGrupo()
        {
            string userId = User.Identity.GetUserId();
            var docenteId = Db.tbDocentes.Where(a => a.UserId == userId).Select(a => a.DocenteId).FirstOrDefault();

            ViewBag.DocenteId = docenteId;
            return View("MateriasSinGrupoStandalone");
        }

        [HttpPost]
        public JsonResult CrearGrupo(tbGrupos grupo)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = "Datos del grupo invalidos." });
            }

            grupo.CodigoAcceso = ObtenerClaveGrupo();
            Db.tbGrupos.Add(grupo);
            Db.SaveChanges();

            return Json(new { mensaje = "Grupo creado con exito.", grupoId = grupo.GrupoId });
        }

        private string ObtenerClaveGrupo()
        {
            var random = new Random();
            return new string(Enumerable.Range(0, 8).Select(_ => (char)random.Next('A', 'Z' + 1)).ToArray());
        }

        [HttpGet]
        public JsonResult ObtenerGrupos(int docenteId)
        {
            var grupos = Db.tbGrupos
                .Where(g => g.DocenteId == docenteId)
                .ToList();

            return Json(grupos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AsociarMaterias(AsociarMateriasRequest request)
        {
            if (request == null || request.MateriaIds == null || request.MateriaIds.Count == 0)
            {
                Response.StatusCode = 400;
                return Json(new { mensaje = "Datos Invalidos" });
            }

            var grupo = Db.tbGrupos.Find(request.GrupoId);
            if (grupo == null)
            {
                Response.StatusCode = 404;
                return Json(new { mensaje = "Grupo no encontrado." });
            }

            var asociacionesActuales = Db.tbGruposMaterias.Where(gm => gm.GrupoId == request.GrupoId).ToList();
            Db.tbGruposMaterias.RemoveRange(asociacionesActuales);

            foreach (var materiaId in request.MateriaIds)
            {
                var materiaExiste = Db.tbMaterias.Any(m => m.MateriaId == materiaId);
                if (materiaExiste)
                {
                    Db.tbGruposMaterias.Add(new tbGruposMaterias
                    {
                        GrupoId = request.GrupoId,
                        MateriaId = materiaId
                    });
                }
            }

            Db.SaveChanges();
            return Json(new { mensaje = "Materias asociadas correctamente." });
        }

        [HttpGet]
        public JsonResult ObtenerMateriasPorGrupo(int grupoId)
        {
            var materiasIds = Db.tbGruposMaterias
                .Where(gm => gm.GrupoId == grupoId)
                .Select(gm => gm.MateriaId)
                .ToList();

            var materiasConActividades = Db.tbMaterias
                .Where(m => materiasIds.Contains(m.MateriaId))
                .Select(m => new
                {
                    m.MateriaId,
                    m.NombreMateria,
                    m.Descripcion,
                    m.DocenteId,
                    DocenteNombre = Db.tbDocentes.Where(d => d.DocenteId == m.DocenteId).Select(d => d.Nombre + " " + d.ApellidoPaterno + " " + d.ApellidoMaterno).FirstOrDefault(),
                    m.CodigoColor,
                    ActividadesRecientes = Db.tbActividades
                        .Where(a => a.MateriaId == m.MateriaId)
                        .OrderByDescending(a => a.FechaCreacion)
                        .Take(2)
                        .Select(a => new
                        {
                            a.ActividadId,
                            a.NombreActividad,
                            a.FechaCreacion
                        })
                        .ToList()
                })
                .ToList();

            return Json(materiasConActividades, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public JsonResult EliminarGrupo(int grupoId)
        {
            var grupo = Db.tbGrupos.Find(grupoId);
            if (grupo == null)
            {
                Response.StatusCode = 404;
                return Json(new { mensaje = "El grupo no existe." });
            }

            var relaciones = Db.tbGruposMaterias.Where(gm => gm.GrupoId == grupoId).ToList();
            Db.tbGruposMaterias.RemoveRange(relaciones);

            Db.tbGrupos.Remove(grupo);
            Db.SaveChanges();

            return Json(new { mensaje = "Grupo eliminado correctamente." });
        }

        [HttpDelete]
        public JsonResult EliminarGrupoConMaterias(int grupoId)
        {
            var grupo = Db.tbGrupos.Find(grupoId);
            if (grupo == null)
            {
                Response.StatusCode = 404;
                return Json(new { mensaje = "El grupo no existe" });
            }

            var relacionesGruposMaterias = Db.tbGruposMaterias.Where(mg => mg.GrupoId == grupoId).ToList();
            var materiasIds = relacionesGruposMaterias.Select(r => r.MateriaId).ToList();

            var relacionesAlumnosMaterias = Db.tbAlumnosMaterias.Where(am => materiasIds.Contains(am.MateriaId)).ToList();
            Db.tbAlumnosMaterias.RemoveRange(relacionesAlumnosMaterias);

            Db.tbGruposMaterias.RemoveRange(relacionesGruposMaterias);

            var actividades = Db.tbActividades.Where(a => materiasIds.Contains(a.MateriaId)).ToList();
            Db.tbActividades.RemoveRange(actividades);

            var avisos = Db.tbAvisos.Where(av => av.MateriaId.HasValue && materiasIds.Contains(av.MateriaId.Value)).ToList();
            Db.tbAvisos.RemoveRange(avisos);

            var materias = Db.tbMaterias.Where(m => materiasIds.Contains(m.MateriaId)).ToList();
            Db.tbMaterias.RemoveRange(materias);

            Db.tbGrupos.Remove(grupo);
            Db.SaveChanges();

            return Json(new { mensaje = "Grupo, materias, actividades y avisos eliminados correctamente" });
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
