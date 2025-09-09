﻿using System;
using System.Collections.Generic;
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
    public class GruposController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;

        public GruposController()
        {
        }

        public GruposController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg)
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
            private set
            {
                _fg = value;
            }
        }



        [HttpPost]
        public JsonResult CrearGrupo(tbGrupos grupo)
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400; // BadRequest
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
                .Select(a=> new { a.GrupoId, a.Descripcion, a.NombreGrupo, a.CodigoColor, a.CodigoAcceso })
                .ToList();

            return Json(grupos, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult AsociarMaterias(AsociarMateriasRequest request)
        {
            if (request == null || request.MateriaIds == null || request.MateriaIds.Count == 0)
            {
                Response.StatusCode = 400; // BadRequest
                return Json(new { mensaje = "Datos Invalidos" });
            }

            var grupo = Db.tbGrupos.Find(request.GrupoId);
            if (grupo == null)
            {
                Response.StatusCode = 404; // NotFound
                return Json(new { mensaje = "Grupo no encontrado." });
            }

            var asociacionesActuales = Db.tbGruposMaterias.Where(gm => gm.GrupoId == request.GrupoId).ToList();
            Db.tbGruposMaterias.RemoveRange(asociacionesActuales);

            foreach (var materiaId in request.MateriaIds)
            {
                var materiaExiste = Db.tbMaterias.Any(m => m.MateriaId == materiaId);
                if (materiaExiste)
                {
                    var nuevaRelacion = new tbGruposMaterias
                    {
                        GrupoId = request.GrupoId,
                        MateriaId = materiaId
                    };
                    Db.tbGruposMaterias.Add(nuevaRelacion);
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
                Response.StatusCode = 404; // NotFound
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
                Response.StatusCode = 404; // NotFound
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
