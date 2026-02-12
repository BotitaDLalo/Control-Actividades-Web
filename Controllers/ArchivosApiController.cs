using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/Archivos")]
    public class ArchivosApiController : ApiController
    {
        [HttpPost]
        [Route("SubirArchivo")]
        public async Task<IHttpActionResult> SubirArchivo()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                if (httpRequest == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "Solicitud vacía" });
                }

                if (httpRequest.Files.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "No se recibió ningún archivo" });
                }

                var file = httpRequest.Files[0];
                if (file == null || file.ContentLength == 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "Archivo vacío" });
                }

                int.TryParse(httpRequest.Form["ActividadId"], out int actividadId);
                int.TryParse(httpRequest.Form["AlumnoId"], out int alumnoId);

                if (actividadId <= 0 || alumnoId <= 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "ActividadId y AlumnoId requeridos" });
                }

                var uploadRoot = HttpContext.Current.Server.MapPath("~/Uploads/Entregas/");
                var destFolder = Path.Combine(uploadRoot, actividadId.ToString(), alumnoId.ToString());

                if (!Directory.Exists(destFolder))
                    Directory.CreateDirectory(destFolder);

                var extension = Path.GetExtension(file.FileName).ToLower();
                var extensionesPermitidas = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".jpg", ".jpeg", ".png", ".gif", ".txt", ".zip", ".rar", ".7z", ".odt", ".ods", ".odp", ".rtf" };

                bool extensionValida = false;
                foreach (var ext in extensionesPermitidas)
                {
                    if (extension.Equals(ext))
                    {
                        extensionValida = true;
                        break;
                    }
                }

                if (!extensionValida)
                {
                    return Content(HttpStatusCode.BadRequest, new { mensaje = $"Extensión no permitida: {extension}" });
                }

                const long maxPorArchivo = 50 * 1024 * 1024;
                if (file.ContentLength > maxPorArchivo)
                {
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "Archivo excede 50MB" });
                }

                var safeName = Path.GetFileName(file.FileName);
                var destPath = Path.Combine(destFolder, safeName);

                if (File.Exists(destPath))
                {
                    var ts = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    safeName = $"{ts}_{safeName}";
                    destPath = Path.Combine(destFolder, safeName);
                }

                file.SaveAs(destPath);
                var ruta = $"/Uploads/Entregas/{actividadId}/{alumnoId}/{safeName}";

                Console.WriteLine($"[LOG] Archivo subido: {ruta}");

                return Ok(new
                {
                    url = ruta,
                    nombre = file.FileName,
                    nombreGuardado = safeName,
                    size = file.ContentLength
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SubirArchivo: {ex.Message}");
                return Content(HttpStatusCode.InternalServerError, new { mensaje = ex.Message });
            }
        }
    }
}
