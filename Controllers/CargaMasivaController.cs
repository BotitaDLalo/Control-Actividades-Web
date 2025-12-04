using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ControlActividades.Models;
using ControlActividades.Models.db;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/CargaMasiva")]
    public class CargaMasivaController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationDbContext _db;

        public CargaMasivaController() { }

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

        [HttpPost]
        [Route("ImportarAlumnosExcel")]
        public async Task<IHttpActionResult> ImportarAlumnosExcel()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest == null || httpRequest.Files.Count == 0)
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "No se recibi� archivo." });

                var file = httpRequest.Files[0];
                if (file == null || file.ContentLength == 0)
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "Archivo vac�o." });

                // Leer par�metros opcionales
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
                    var firstCell = headerRow.GetCell(0)?.ToString()?.ToLower() ?? "";
                    if (firstCell.Contains("email")) hasHeader = true;
                }

                var emails = new List<string>();
                for (int r = hasHeader ? startRow + 1 : startRow; r <= sheet.LastRowNum; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;
                    var cell = row.GetCell(0);
                    var text = cell?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(text)) continue;
                    // Validaci�n simple de email
                    if (!text.Contains("@"))
                    {
                        continue;
                    }
                    emails.Add(text);
                }

                if (!emails.Any())
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "No se encontraron emails en la primera columna." });

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

                // Construir lista de alumnos para respuesta
                var alumnos = new List<EmailVerificadoAlumno>();
                foreach (var id in lsAlumnosId)
                {
                    var alumnoDatos = await Db.tbAlumnos.Where(a => a.AlumnoId == id).FirstOrDefaultAsync();
                    if (alumnoDatos != null)
                    {
                        var userName = await UserManager.FindByIdAsync(alumnoDatos.UserId);
                        alumnos.Add(new EmailVerificadoAlumno
                        {
                            Email = userName?.Email ?? "",
                            UserName = userName?.UserName ?? "",
                            Nombre = alumnoDatos.Nombre,
                            ApellidoPaterno = alumnoDatos.ApellidoPaterno,
                            ApellidoMaterno = alumnoDatos.ApellidoMaterno
                        });
                    }
                }

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
                return Content(System.Net.HttpStatusCode.BadRequest, new { mensaje = ex.Message });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _user_manager.Dispose();
                    _user_manager = null;
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