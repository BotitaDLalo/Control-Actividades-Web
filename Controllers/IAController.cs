using System;
using System.Configuration;
using System.IO;
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
            // Prefer environment variable for safety, fallback to Web.config appSettings
            var key = Environment.GetEnvironmentVariable("AIService_ApiKey");
            if (!string.IsNullOrEmpty(key)) return key;
            try
            {
                return ConfigurationManager.AppSettings["AIService_ApiKey"];
            }
            catch
            {
                return null;
            }
        }

        private string GetForwardUrl()
        {
            try
            {
                return ConfigurationManager.AppSettings["AIService_ForwardUrl"];
            }
            catch
            {
                return null;
            }
        }

        private bool UseApiKeyInHeader()
        {
            try
            {
                var v = ConfigurationManager.AppSettings["AIService_UseApiKeyInHeader"];
                if (string.IsNullOrEmpty(v)) return true; // default to header
                return v.Trim().ToLower() == "true";
            }
            catch
            {
                return true;
            }
        }

        // Ping simple para comprobar disponibilidad
        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            return Ok(new { mensaje = "pong" });
        }

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
            string forwardUrl = GetForwardUrl();
            string apiKey = GetApiKey();
            bool useHeader = UseApiKeyInHeader();

            if (string.IsNullOrEmpty(forwardUrl))
            {
                return Content(HttpStatusCode.InternalServerError, new { mensaje = "AIService_ForwardUrl no configurada. Actualiza Web.config appSettings con la URL del endpoint AI." });
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                return Content(HttpStatusCode.InternalServerError, new { mensaje = "AIService_ApiKey no configurada. Establece la clave en variable de entorno 'AIService_ApiKey' o en Web.config appSettings." });
            }

            string body;
            try
            {
                body = await Request.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = "Error leyendo el cuerpo de la petición", detalle = ex.Message });
            }

            try
            {
                using (var client = new HttpClient())
                {
                    string target = forwardUrl;
                    if (!useHeader)
                    {
                        // append api key as query string parameter if not present
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

                    // return raw response preserving status code
                    var respMessage = Content(resp.StatusCode, respText);
                    return respMessage;
                }
            }
            catch (HttpRequestException hre)
            {
                return Content(HttpStatusCode.BadGateway, new { mensaje = "Error al comunicarse con el servicio AI externo", detalle = hre.Message });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { mensaje = "Error interno al procesar la petición AI", detalle = ex.Message });
            }
        }
    }
}