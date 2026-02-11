using NPOI.POIFS.Crypt;
using Org.BouncyCastle.Asn1.Ocsp;
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
            return Environment.GetEnvironmentVariable("GENERATIVE_API_KEY")
                ?? ConfigurationManager.AppSettings["GenerativeApiKey"];
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
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, new { mensaje = "AIService_ForwardUrl no configurada." });
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                return Content(System.Net.HttpStatusCode.InternalServerError, new { mensaje = "AIService_ApiKey no configurada." });
            }

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

                    // return raw response preserving status code
                    return ResponseMessage(new HttpResponseMessage(resp.StatusCode)
                    {
                        Content = new StringContent(respText, System.Text.Encoding.UTF8, "application/json")
                    });
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