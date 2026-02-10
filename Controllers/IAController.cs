using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/IA")]
    public class IAController : ApiController
    {
        private string GetApiKey()
        {
            // Prefer environment variable for safety
            var key = Environment.GetEnvironmentVariable("AIService_ApiKey");
            if (!string.IsNullOrEmpty(key)) return key;

            // Support alternate key name used in Web.config
            key = ConfigurationManager.AppSettings["AI:ApiKey"];
            if (!string.IsNullOrEmpty(key)) return key;

            // Fallback to legacy name
            return ConfigurationManager.AppSettings["AIService_ApiKey"];
        }

        private string GetForwardUrl()
        {
            return ConfigurationManager.AppSettings["AIService_ForwardUrl"];
        }

        private bool UseApiKeyInHeader()
        {
            var v = ConfigurationManager.AppSettings["AIService_UseApiKeyInHeader"];
            if (string.IsNullOrEmpty(v)) return true; // default to header
            return v.Trim().ToLower() == "true";
        }

        // Ping simple para comprobar disponibilidad
        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping() => Ok(new { mensaje = "pong" });

        // Stub/proxy para pruebas: /api/IA/MejorarDescripcion
        [HttpPost]
        [Route("MejorarDescripcion")]
        public async Task<IHttpActionResult> MejorarDescripcion()
        {
            return await ProxyRequest();
        }

        // Stub para el chat: /api/IA/GenerarContenido
        [HttpPost]
        [Route("GenerarContenido")]
        public async Task<IHttpActionResult> GenerarContenido()
        {
            return await ProxyRequest();
        }

        private async Task<IHttpActionResult> ProxyRequest()
        {
            var forwardUrl = GetForwardUrl();
            var apiKey = GetApiKey();
            var useHeader = UseApiKeyInHeader();

            if (string.IsNullOrEmpty(forwardUrl))
                return Content(System.Net.HttpStatusCode.InternalServerError, new { mensaje = "AIService_ForwardUrl no configurada." });

            if (string.IsNullOrEmpty(apiKey))
                return Content(System.Net.HttpStatusCode.InternalServerError, new { mensaje = "AIService_ApiKey no configurada." });

            string body;
            try
            {
                body = await Request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.BadRequest, new { mensaje = "Error leyendo el cuerpo de la petición", detalle = ex.Message });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    var target = forwardUrl;
                    if (!useHeader)
                    {
                        var separator = target.Contains("?") ? "&" : "?";
                        target = target + separator + "key=" + WebUtility.UrlEncode(apiKey);
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                    }

                    var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
                    var resp = await client.PostAsync(target, content);
                    var respText = await resp.Content.ReadAsStringAsync();

                    return Content(resp.StatusCode, respText);
                }
            }
            catch (HttpRequestException hre)
            {
                return Content(System.Net.HttpStatusCode.BadGateway, new { mensaje = "Error al comunicarse con el servicio AI externo", detalle = hre.Message });
            }
            catch (Exception ex)
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, new { mensaje = "Error interno al procesar la petición AI", detalle = ex.Message });
            }
        }
    }
}