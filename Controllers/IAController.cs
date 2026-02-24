using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ControlActividades.Controllers
{
    [Authorize(Roles = "Docente")]
    [RoutePrefix("api/IA")]
    public class IAController : ApiController
    {
        private string GetApiKey()
        {
            return Environment.GetEnvironmentVariable("GENERATIVE_API_KEY")
                   ?? ConfigurationManager.AppSettings["GenerativeApiKey"];
        }

        private static readonly HttpClient _http = new HttpClient();

        static IAController()
        {
            _http.Timeout = TimeSpan.FromSeconds(60);
        }

        private string GetForwardUrl()
        {
            return ConfigurationManager.AppSettings["AIService_ForwardUrl"];
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
          

            if (string.IsNullOrEmpty(forwardUrl))
            {
                return Content(System.Net.HttpStatusCode.InternalServerError,
                    new { mensaje = "AIService_ForwardUrl no configurada." });
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                return Content(System.Net.HttpStatusCode.InternalServerError,
                    new { mensaje = "AIService_ApiKey no configurada." });
            }


            try
            {
                var body = await Request.Content.ReadAsStringAsync();

                // Siempre agregar API key como query param
                var separator = forwardUrl.Contains("?") ? "&" : "?";
                var target = forwardUrl + separator + "key=" + WebUtility.UrlEncode(apiKey);

                var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

                var resp = await _http.PostAsync(target, content);
                var respText = await resp.Content.ReadAsStringAsync();

                return ResponseMessage(new HttpResponseMessage(resp.StatusCode)
                {
                    Content = new StringContent(respText, System.Text.Encoding.UTF8, "application/json")
                });
            }
            catch (HttpRequestException hre)
            {
                return Content(HttpStatusCode.BadGateway,
                    new { mensaje = "Error comunicándose con Gemini", detalle = hre.Message });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError,
                    new { mensaje = "Error interno", detalle = ex.Message });
            }
        }

        [HttpPost]
        [Route("MejorarDescripcion")]
        public async Task<IHttpActionResult> MejorarDescripcion([FromBody] dynamic data)
        {
            try
            {
                var apiKey = GetApiKey();

                if (string.IsNullOrEmpty(apiKey))
                    return InternalServerError(new Exception("API key no configurada"));

                string nombre = data?.Nombre;
                string descripcion = data?.Descripcion;

                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(descripcion))
                    return BadRequest("Nombre y descripción son obligatorios");

                var prompt = $@"
                    Genera exactamente 3 versiones mejoradas de la siguiente actividad.
                    No expliques nada.
                    No numeres.
                    Separa cada sugerencia con una línea en blanco.

                    Título: {nombre}
                    Descripción: {descripcion}
                    ";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                using (var client = new HttpClient())
                {
                    var url = "https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key="
                              + apiKey;

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(url, content);
                    var responseText = await response.Content.ReadAsStringAsync();

                    return ResponseMessage(new HttpResponseMessage(response.StatusCode)
                    {
                        Content = new StringContent(responseText, System.Text.Encoding.UTF8, "application/json")
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}