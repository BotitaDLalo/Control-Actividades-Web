using System;
using System.Threading.Tasks;
using System.Web.Http;
namespace ControlActividades.Controllers
{
    [RoutePrefix("api/IA")]
    public class IAController : ApiController
    { // Ping simple para comprobar disponibilidad [HttpGet] [Route("ping")] public IHttpActionResult Ping() { return Ok(new { status = "ok" }); }
      // Stub/proxy para pruebas: /api/IA/MejorarDescripcion
        [HttpPost]
        [Route("MejorarDescripcion")]
        public async Task<IHttpActionResult> MejorarDescripcion([FromBody] DescripcionRequest req)
        {
            await Task.Yield();
            var text = "1. Redacta una descripción clara y breve con objetivos de aprendizaje.\n\n2. Incluye fecha límite y criterios de evaluación.\n\n3. Añade pasos sugeridos y recursos de apoyo.";
            var resp = new
            {
                candidates = new[]
                {
                new
                {
                    content = new
                    {
                        parts = new[] { new { text = text } }
                    }
                }
            }
            };
            return Ok(resp);
        }

        // Stub para el chat: /api/IA/GenerarContenido
        [HttpPost]
        [Route("GenerarContenido")]
        public async Task<IHttpActionResult> GenerarContenido([FromBody] object body)
        {
            await Task.Yield();
            var resp = new
            {
                candidates = new[]
                {
                new
                {
                    content = new
                    {
                        parts = new[] { new { text = "Respuesta simulada desde el servidor. (Modo desarrollo)" } }
                    }
                }
            }
            };
            return Ok(resp);
        }

        // Endpoint diagnóstico opcional
        [HttpGet]
        [Route("diagnostic")]
        public IHttpActionResult Diagnostic()
        {
            return Ok(new { server = Environment.MachineName, now = DateTime.UtcNow });
        }

        public class DescripcionRequest
        {
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public string model { get; set; }
        }
    }
}