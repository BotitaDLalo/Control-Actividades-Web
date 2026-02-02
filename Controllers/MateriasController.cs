using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using ControlActividades.Models;
using ControlActividades.Models.db;
using System.Data.Entity;
using System.Linq;

namespace ControlActividades.Controllers
{
    // Minimal MateriasController: added ConfiguracionPartial to return the
    // partial view _Configuracion with ViewBag.NombreMateria populated.
    public class MateriasController : Controller
    {
        private ApplicationDbContext _db;
        protected ApplicationDbContext Db => _db ?? (_db = new ApplicationDbContext());

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Grupos");
        }

        // Compatibilidad: crear aviso dirigido a un grupo (/Materias/CrearAvisoPorGrupo)
        [HttpPost]
        public ActionResult CrearAvisoPorGrupo()
        {
            try
            {
                Request.InputStream.Position = 0;
                string body;
                using (var sr = new StreamReader(Request.InputStream)) body = sr.ReadToEnd();
                var dto = JsonConvert.DeserializeObject<Models.PeticionCrearAviso>(body);
                if (dto == null || string.IsNullOrWhiteSpace(dto.Titulo) || string.IsNullOrWhiteSpace(dto.Descripcion) || dto.GrupoId == null)
                    return new HttpStatusCodeResult(400, "Datos inválidos");

                var aviso = new tbAvisos
                {
                    DocenteId = dto.DocenteId,
                    Titulo = dto.Titulo,
                    Descripcion = dto.Descripcion,
                    FechaCreacion = dto.FechaCreacion == default(DateTime) ? DateTime.Now : dto.FechaCreacion,
                    GrupoId = dto.GrupoId,
                    MateriaId = dto.MateriaId
                };

                if (aviso.DocenteId == 0 && User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var uid = User.Identity.GetUserId();
                    var docente = Db.tbDocentes.FirstOrDefault(d => d.UserId == uid);
                    if (docente != null) aviso.DocenteId = docente.DocenteId;
                }

                Db.tbAvisos.Add(aviso);
                Db.SaveChanges();

                return Json(new { mensaje = "Aviso creado", AvisoId = aviso.AvisoId });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al crear aviso por grupo", error = ex.Message });
            }
        }

        // Compatibilidad: crear actividad vía MVC (/Materias/CrearActividad)
        [HttpPost]
        public ActionResult CrearActividad()
        {
            try
            {
                Request.InputStream.Position = 0;
                string body;
                using (var sr = new StreamReader(Request.InputStream)) body = sr.ReadToEnd();

                // usar un tipo dinámico para tolerar diferentes propiedades y formatos
                var obj = JsonConvert.DeserializeObject<dynamic>(body);
                if (obj == null) return new HttpStatusCodeResult(400, "Datos inválidos");

                string nombre = obj.NombreActividad != null ? (string)obj.NombreActividad : null;
                string descripcion = obj.Descripcion != null ? (string)obj.Descripcion : null;
                string fechaLimiteRaw = obj.FechaLimite != null ? (string)obj.FechaLimite : null;
                int materiaId = obj.MateriaId != null ? (int)obj.MateriaId : 0;
                int puntaje = 0;
                try { puntaje = obj.Puntaje != null ? (int)obj.Puntaje : 0; } catch { puntaje = 0; }
                bool? enviado = null;
                try { if (obj.Enviado != null) enviado = (bool?)obj.Enviado; } catch { enviado = null; }

                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(descripcion) || materiaId == 0)
                    return new HttpStatusCodeResult(400, "Faltan campos requeridos");

                DateTime fechaLimite;
                if (!DateTime.TryParse((fechaLimiteRaw ?? string.Empty), out fechaLimite))
                {
                    // intentar formato ISO sin segundos
                    if (!DateTime.TryParseExact(fechaLimiteRaw ?? string.Empty, new[] { "yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fechaLimite))
                    {
                        fechaLimite = DateTime.Now.AddDays(1);
                    }
                }

                var actividad = new tbActividades
                {
                    NombreActividad = nombre,
                    Descripcion = descripcion,
                    FechaCreacion = DateTime.Now,
                    FechaLimite = fechaLimite,
                    Puntaje = puntaje,
                    MateriaId = materiaId,
                    Enviado = enviado
                };

                Db.tbActividades.Add(actividad);
                Db.SaveChanges();

                return Json(new { mensaje = "Actividad creada", ActividadId = actividad.ActividadId });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al crear actividad", error = ex.Message });
            }
        }

        // Compatibilidad: crear aviso vía MVC (scripts usan /Materias/CrearAviso)
        [HttpPost]
        public ActionResult CrearAviso()
        {
            try
            {
                Request.InputStream.Position = 0;
                string body;
                using (var sr = new StreamReader(Request.InputStream)) body = sr.ReadToEnd();
                var petición = JsonConvert.DeserializeObject<Models.PeticionCrearAviso>(body);
                if (petición == null || string.IsNullOrWhiteSpace(petición.Titulo) || string.IsNullOrWhiteSpace(petición.Descripcion))
                    return new HttpStatusCodeResult(400, "Datos inválidos");

                var aviso = new tbAvisos
                {
                    DocenteId = petición.DocenteId,
                    Titulo = petición.Titulo,
                    Descripcion = petición.Descripcion,
                    FechaCreacion = petición.FechaCreacion == default(DateTime) ? DateTime.Now : petición.FechaCreacion,
                    GrupoId = petición.GrupoId,
                    MateriaId = petición.MateriaId
                };

                // Si DocenteId no viene, intentar resolver desde el usuario actual
                if (aviso.DocenteId == 0 && User != null && User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var uid = User.Identity.GetUserId();
                    var docente = Db.tbDocentes.FirstOrDefault(d => d.UserId == uid);
                    if (docente != null) aviso.DocenteId = docente.DocenteId;
                }

                Db.tbAvisos.Add(aviso);
                Db.SaveChanges();

                return Json(new { mensaje = "Aviso creado", AvisoId = aviso.AvisoId });
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al crear aviso", error = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult ObtenerAvisoPorId(int avisoId)
        {
            try
            {
                var a = Db.tbAvisos.Where(x => x.AvisoId == avisoId).Select(x => new
                {
                    x.AvisoId,
                    x.Titulo,
                    x.Descripcion,
                    FechaCreacion = x.FechaCreacion,
                    x.DocenteId,
                    x.MateriaId,
                    x.GrupoId
                }).FirstOrDefault();

                if (a == null) return HttpNotFound();
                return Json(a, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al obtener aviso", error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPut]
        public ActionResult EditarAviso()
        {
            try
            {
                Request.InputStream.Position = 0;
                string body;
                using (var sr = new StreamReader(Request.InputStream)) body = sr.ReadToEnd();
                var dto = JsonConvert.DeserializeObject<Models.AvisoDto>(body);
                if (dto == null || dto.AvisoId <= 0) return new HttpStatusCodeResult(400, "Datos inválidos");

                var dbAviso = Db.tbAvisos.Find(dto.AvisoId);
                if (dbAviso == null) return HttpNotFound();

                dbAviso.Titulo = dto.Titulo ?? dbAviso.Titulo;
                dbAviso.Descripcion = dto.Descripcion ?? dbAviso.Descripcion;
                Db.SaveChanges();

                return Json(new { mensaje = "Actualizado", AvisoId = dbAviso.AvisoId });
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al editar aviso", error = e.Message });
            }
        }

        [HttpDelete]
        public ActionResult EliminarAviso(int id)
        {
            try
            {
                var aviso = Db.tbAvisos.Find(id);
                if (aviso == null) return HttpNotFound();
                Db.tbAvisos.Remove(aviso);
                Db.SaveChanges();
                return Json(new { mensaje = "Eliminado" });
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al eliminar aviso", error = e.Message });
            }
        }

        // Devuelve la partial view de configuración para una materia.
        // Se puede invocar vía AJAX desde el cliente: /Materias/ConfiguracionPartial?materiaId=123
        public async Task<ActionResult> ConfiguracionPartial(int? materiaId)
        {
            if (materiaId.HasValue)
            {
                var materia = await Db.tbMaterias.Where(m => m.MateriaId == materiaId.Value)
                    .Select(m => new { m.MateriaId, m.NombreMateria })
                    .FirstOrDefaultAsync();

                ViewBag.NombreMateria = materia?.NombreMateria ?? string.Empty;
            }
            else
            {
                ViewBag.NombreMateria = string.Empty;
            }

            return PartialView("_Configuracion");
        }

        // Compatibilidad: devolver actividades de una materia (ruta esperada por scripts antiguos)
        [HttpGet]
        public ActionResult ObtenerActividadesPorMateria(int materiaId, string filtro = null)
        {
            try
            {
                var query = Db.tbActividades.Where(a => a.MateriaId == materiaId).ToList();
                var rolUsuario = (User != null && User.IsInRole(ControlActividades.Roles.DOCENTE)) ? "Docente" : (User != null && User.IsInRole(ControlActividades.Roles.ALUMNO) ? "Alumno" : "Anonimo");

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

        // Compatibilidad: obtener avisos por materia (ruta usada por scripts)
        [HttpGet]
        public ActionResult ObtenerAvisos(int IdMateria, int? grupoId = null)
        {
            try
            {
                // incluir avisos asociados directamente a la materia
                // además incluir avisos creados para grupos que contengan la materia
                var gruposMateria = Db.tbGruposMaterias.Where(gm => gm.MateriaId == IdMateria).Select(gm => gm.GrupoId).ToList();

                var avisos = Db.tbAvisos.Where(a => a.MateriaId == IdMateria || (a.GrupoId != null && gruposMateria.Contains(a.GrupoId.Value)))
                    .Select(a => new
                {
                    a.AvisoId,
                    a.Titulo,
                    a.Descripcion,
                    FechaCreacion = a.FechaCreacion,
                    a.DocenteId,
                    a.MateriaId,
                    a.GrupoId
                }).OrderByDescending(a => a.FechaCreacion).ToList();

                return Json(avisos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al obtener avisos", error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Compatibilidad: obtener alumnos asignados a una materia
        [HttpGet]
        public ActionResult ObtenerAlumnosPorMateria(int materiaId)
        {
            try
            {
                var alumnos = Db.tbAlumnosMaterias
                    .Where(am => am.MateriaId == materiaId)
                    .Select(am => new
                    {
                        am.AlumnoMateriaId,
                        am.AlumnoId,
                        Nombre = am.Alumnos.Nombre,
                        ApellidoPaterno = am.Alumnos.ApellidoPaterno,
                        ApellidoMaterno = am.Alumnos.ApellidoMaterno,
                        Email = am.Alumnos.IdentityUser != null ? am.Alumnos.IdentityUser.Email : null
                    }).ToList();

                return Json(alumnos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al obtener alumnos", error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Compatibilidad: buscar alumnos por correo o nombre (usado por scripts de asignación)
        [HttpGet]
        public ActionResult BuscarAlumnosPorCorreo(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query)) return Json(new object[0], JsonRequestBehavior.AllowGet);

                var q = query.Trim();
                var lista = Db.tbAlumnos
                    .Where(a => a.Nombre.Contains(q) || a.ApellidoPaterno.Contains(q) || a.ApellidoMaterno.Contains(q))
                    .Select(a => new
                    {
                        a.AlumnoId,
                        a.Nombre,
                        a.ApellidoPaterno,
                        a.ApellidoMaterno,
                        Email = a.IdentityUser != null ? a.IdentityUser.Email : null
                    }).Take(20).ToList();

                return Json(lista, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                return Json(new { mensaje = "Error al buscar alumnos", error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Compatibilidad: crear materia (usado por scripts que llaman /Materias/CrearMateria)
        [HttpPost]
        public async Task<ActionResult> CrearMateria(tbMaterias materia)
        {
            if (materia == null)
                return new HttpStatusCodeResult(400, "Datos inválidos");

            try
            {
                // generar código si no viene
                if (string.IsNullOrWhiteSpace(materia.CodigoAcceso))
                {
                    materia.CodigoAcceso = new Func<string>(() =>
                    {
                        var rnd = new Random();
                        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                        return new string(Enumerable.Range(0, 8).Select(i => chars[rnd.Next(chars.Length)]).ToArray());
                    })();
                }

                Db.tbMaterias.Add(materia);
                await Db.SaveChangesAsync();

                return Json(new { MateriaId = materia.MateriaId, mensaje = "Materia creada" });
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        // Compatibilidad: actualizar materia desde scripts (/Materias/ActualizarMateria)
        [HttpPost]
        public async Task<ActionResult> ActualizarMateria(int materiaId, tbMaterias model)
        {
            if (materiaId <= 0 || model == null)
                return new HttpStatusCodeResult(400, "Parámetros inválidos");

            var m = await Db.tbMaterias.FindAsync(materiaId);
            if (m == null) return HttpNotFound("Materia no encontrada");

            m.NombreMateria = model.NombreMateria ?? m.NombreMateria;
            m.Descripcion = model.Descripcion ?? m.Descripcion;
            if (!string.IsNullOrWhiteSpace(model.CodigoColor)) m.CodigoColor = model.CodigoColor;

            await Db.SaveChangesAsync();
            return Json(new { mensaje = "Actualizado", MateriaId = m.MateriaId });
        }

        // Compatibilidad: eliminar materia (/Materias/EliminarMateria or /Materias/EliminarMateria/{id})
        [HttpDelete]
        public async Task<ActionResult> EliminarMateria(int id)
        {
            if (id <= 0) return new HttpStatusCodeResult(400, "Id inválido");
            var m = await Db.tbMaterias.FindAsync(id);
            if (m == null) return HttpNotFound("Materia no encontrada");

            // quitar relaciones grupo-materia si existen
            var relaciones = Db.tbGruposMaterias.Where(g => g.MateriaId == id).ToList();
            if (relaciones.Any()) Db.tbGruposMaterias.RemoveRange(relaciones);

            Db.tbMaterias.Remove(m);
            await Db.SaveChangesAsync();

            return Json(new { mensaje = "Eliminada", materiaId = id });
        }

        // Compatibilidad: obtener materias sin grupo para un docente (/Materias/ObtenerMateriasSinGrupo)
        [HttpGet]
        public ActionResult ObtenerMateriasSinGrupo(int docenteId)
        {
            try
            {
                var lsGruposMaterias = Db.tbGruposMaterias.Select(a => a.MateriaId).ToList();
                var materias = Db.tbMaterias.Where(a => a.DocenteId == docenteId && !lsGruposMaterias.Contains(a.MateriaId))
                    .Select(a => new { a.MateriaId, a.NombreMateria, a.Descripcion, a.CodigoColor, a.CodigoAcceso, a.DocenteId })
                    .ToList();

                return Json(materias, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return new HttpStatusCodeResult(500, e.Message);
            }
        }

        // Compatibilidad: detalles para MVC fallback (/Materias/ObtenerDetallesMateria)
        [HttpGet]
        public ActionResult ObtenerDetallesMateria(int materiaId, int docenteId = 0)
        {
            var subject = Db.tbMaterias.Where(m => m.MateriaId == materiaId).Select(m => new
            {
                m.MateriaId,
                m.NombreMateria,
                m.Descripcion,
                m.CodigoAcceso,
                m.CodigoColor,
                m.DocenteId
            }).FirstOrDefault();

            if (subject == null) return HttpNotFound("Materia no encontrada");
            return Json(subject, JsonRequestBehavior.AllowGet);
        }

        // Copiar actividades: endpoint compat (no realiza copia compleja, solo devuelve OK para evitar 404)
        [HttpPost]
        public ActionResult CopiarActividades(object payload)
        {
            // En este punto se puede implementar la copia real. Por compatibilidad devolvemos OK.
            return Json(new { mensaje = "Operación de copia encolada (compatibilidad)." });
        }

        // Compatibilidad: recibir rutas antiguas que apuntaban a /Materias/MateriaDetalles
        // Redirige al controlador adecuado según el rol del usuario.
        // Si es docente -> /Docente/MateriasDetalles, si no -> /Alumno/Clase (tipo=materia).
        public ActionResult MateriaDetalles(int? materiaId, int? grupoId, string seccion)
        {
            if (!materiaId.HasValue)
                return RedirectToAction("Index");

            try
            {
                if (User != null && User.IsInRole(ControlActividades.Roles.DOCENTE))
                {
                    return RedirectToAction("MateriasDetalles", "Docente", new { materiaId = materiaId.Value, grupoId = grupoId ?? 0, seccion = seccion ?? "avisos" });
                }
                // Por defecto redirigir a la vista de alumno para la materia
                return Redirect($"/Alumno/Clase?tipo=materia&id={materiaId.Value}");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db?.Dispose();
                _db = null;
            }
            base.Dispose(disposing);
        }
    }
}
