using ControlActividades.Dtos.Migracion;
using ControlActividades.Migracion;
using System;
using System.IO;
using System.Text.Json;
using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Controllers
{
    public class MigracionTestController : Controller
    {
        [HttpGet]
        public ActionResult EjecutarBulkTest()
        {
            try
            {
                var ruta = Server.MapPath("~/App_Data/usuarios-simulados2500.json");

                if (!System.IO.File.Exists(ruta))
                    return Content("ERROR: No existe el archivo JSON");

                var json = System.IO.File.ReadAllText(ruta);
                var dto = JsonSerializer.Deserialize<MigracionUsuariosDto>(json);

                if (dto?.Usuarios == null || dto.Usuarios.Count == 0)
                    return Content("ERROR: El JSON no contiene usuarios");
                
                var inicio = DateTime.Now;
                var migrator = new BulkIdentityMigrator();
                migrator.MigrarUsuarios(dto.Usuarios);

                var fin = DateTime.Now;
                var segundos = (fin - inicio).TotalSeconds;
                return Content($"Migración BULK ejecutada correctamente. Tiempo: {segundos:N2} segundos");
            }
            catch (Exception ex)
            {
                return Content("ERROR: " + ex.Message);
            }
        }
    }
}