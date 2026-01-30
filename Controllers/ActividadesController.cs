using ControlActividades;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ControlActividades.Controllers
{
    public class ActividadesController : Controller
    {
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;

        public ApplicationDbContext Db => _db ?? (_db = new ApplicationDbContext());
        public FuncionalidadesGenerales Fg => _fg ?? (_fg = new FuncionalidadesGenerales());

        // Endpoint usado por vistas alumno/docente: /Actividades/ObtenerActividadesPorMateria?materiaId=123
        [HttpGet]
        public async Task<ActionResult> ObtenerActividadesPorMateria(int materiaId)
        {
            try
            {
                // Cargar entidades en memoria para evitar problemas de traducción de EF en proyecciones
                var query = Db.tbActividades.Where(a => a.MateriaId == materiaId).ToList();

                // Si es alumno, filtrar sólo las actividades públicas / programadas cuyo horario ya pasó
                if (User != null && User.IsInRole(Roles.ALUMNO))
                {
                    query = query.Where(a => a.Enviado == true || (a.Enviado == null && a.FechaProgramada.HasValue && a.FechaProgramada.Value <= DateTime.Now)).ToList();
                }

                var rolUsuario = Fg.ObtenerRolUsuario(User);

                var resultado = query.Select(a => new
                {
                    a.ActividadId,
                    a.NombreActividad,
                    a.Descripcion,
                    FechaCreacion = a.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),
                    FechaLimite = a.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
                    a.Puntaje,
                    Enviado = a.Enviado,
                    FechaProgramada = a.FechaProgramada,
                    Rol = rolUsuario
                }).ToList();

                return Json(new { Actividades = resultado, RolUsuario = rolUsuario }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al obtener las actividades", error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Compatibilidad con endpoint MVC esperado por scripts: /Actividades/ObtenerActividadPorId?actividadId=10
        [HttpGet]
        public async Task<ActionResult> ObtenerActividadPorId(int actividadId)
        {
            try
            {
                var a = await Db.tbActividades.Where(x => x.ActividadId == actividadId)
                    .Select(x => new
                    {
                        ActividadId = x.ActividadId,
                        NombreActividad = x.NombreActividad,
                        Descripcion = x.Descripcion,
                        FechaCreacion = x.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),
                        FechaLimite = x.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
                        Puntaje = x.Puntaje,
                        MateriaId = x.MateriaId,
                        PermitirEntregasTarde = false,
                        Enviado = x.Enviado
                    }).FirstOrDefaultAsync();

                if (a == null)
                {
                    return HttpNotFound();
                }

                return Json(a, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al obtener la actividad", error = ex.Message }, JsonRequestBehavior.AllowGet);
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
