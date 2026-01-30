using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace ControlActividades.Controllers
{
    public class AlumnoController : Controller
    {
        private ApplicationDbContext _db;
        public ApplicationDbContext Db => _db ?? (_db = new ApplicationDbContext());

        // GET: /Alumno/Clase or /Alumno/Clase/{id}
        public ActionResult Clase(int? id, string tipo, string nombre)
        {
            try
            {
                // If tipo provided, use it
                if (!string.IsNullOrWhiteSpace(tipo))
                {
                    tipo = tipo.ToLowerInvariant();
                    if (tipo == "materia")
                    {
                        if (!id.HasValue) return RedirectToAction("Index");
                        int mid = id.Value;
                        ViewBag.MateriaId = mid;
                        var materia = Db.tbMaterias.Find(mid);
                        return View("DetalleMateria", materia);
                    }

                    if (tipo == "grupo")
                    {
                        if (!id.HasValue) return RedirectToAction("Index");
                        int gid = id.Value;
                        ViewBag.GrupoId = gid;
                        var grupo = Db.tbGrupos.Find(gid);
                        return View("DetalleGrupo", grupo);
                    }
                }

                // If no tipo but id provided, try to detect whether it's a Grupo or Materia
                if (id.HasValue)
                {
                    var gid = id.Value;
                    var existeGrupo = Db.tbGrupos.Any(g => g.GrupoId == gid);
                    if (existeGrupo)
                    {
                        ViewBag.GrupoId = gid;
                        var grupo = Db.tbGrupos.Find(gid);
                        return View("DetalleGrupo", grupo);
                    }

                    var existeMateria = Db.tbMaterias.Any(m => m.MateriaId == gid);
                    if (existeMateria)
                    {
                        ViewBag.MateriaId = gid;
                        var materia = Db.tbMaterias.Find(gid);
                        return View("DetalleMateria", materia);
                    }
                }

                // Fallback: show alumno index
                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ActividadDetalle(int? actividadId)
        {
            try
            {
                int alumnoId = 0;
                try
                {
                    var userId = User?.Identity?.GetUserId();
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var alumno = Db.tbAlumnos.FirstOrDefault(a => a.UserId == userId);
                        if (alumno != null) alumnoId = alumno.AlumnoId;
                    }
                }
                catch { }

                ViewBag.AlumnoId = alumnoId;
                // pass actividadId via ViewBag as well for convenience
                ViewBag.ActividadId = actividadId ?? 0;
                return View("ActividadDetalle");
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public ActionResult Avisos(int alumnoId, int? materiaId, int? grupoId)
        {
            try
            {
                ViewBag.AlumnoId = alumnoId;

                List<tbAvisos> avisos = new List<tbAvisos>();

                if (grupoId.HasValue && grupoId.Value > 0)
                {
                    avisos = Db.tbAvisos.Where(a => a.GrupoId == grupoId.Value).OrderByDescending(a => a.FechaCreacion).ToList();
                }
                else if (materiaId.HasValue && materiaId.Value > 0)
                {
                    avisos = Db.tbAvisos.Where(a => a.MateriaId == materiaId.Value).OrderByDescending(a => a.FechaCreacion).ToList();
                }
                else
                {
                    // Obtener materias del alumno y sus avisos
                    var materiasAlumno = Db.tbAlumnosMaterias.Where(am => am.AlumnoId == alumnoId).Select(am => am.MateriaId).ToList();
                    avisos = Db.tbAvisos.Where(a => (a.MateriaId != null && materiasAlumno.Contains(a.MateriaId.Value)) || (a.GrupoId != null && Db.tbAlumnosGrupos.Any(ag => ag.AlumnoId == alumnoId && ag.GrupoId == a.GrupoId))).OrderByDescending(a => a.FechaCreacion).ToList();
                }

                return PartialView("_Avisos", avisos);
            }
            catch (Exception)
            {
                return PartialView("_Avisos", new List<tbAvisos>());
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
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
