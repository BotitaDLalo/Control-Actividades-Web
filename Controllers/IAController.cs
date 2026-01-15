using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web.Hosting;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Google.Apis.Auth.OAuth2;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/IA")]
    public class IAController : ApiController
    {
        private static readonly HttpClient _client = new HttpClient();
        // For testing only: allows forcing a model at runtime (not persisted)
        private static string _forcedModel = null;
        private static readonly object _forcedModelLock = new object();
        private static readonly object _logLock = new object();

        [HttpPost]
        [Route("MejorarDescripcion")]
        public async Task<IHttpActionResult> MejorarDescripcion([FromBody] DescripcionRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Descripcion))
            {
                return BadRequest("Se requiere nombre y descripción");
            }

            // Prefer environment variable for secrets in deployed environments
            var apiKey = Environment.GetEnvironmentVariable("GENERATIVE_API_KEY")
                         ?? ConfigurationManager.AppSettings["GenerativeApiKey"];

            // Fallback: allow a local file in App_Data for development (not for production)
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "REPLACE_WITH_SERVER_KEY")
            {
                try
                {
                    var localKeyPath = HostingEnvironment.MapPath("~/App_Data/GENERATIVE_API_KEY.txt");
                    if (!string.IsNullOrWhiteSpace(localKeyPath) && File.Exists(localKeyPath))
                    {
                        var fileKey = File.ReadAllText(localKeyPath).Trim();
                        if (!string.IsNullOrWhiteSpace(fileKey)) apiKey = fileKey;
                    }
                }
                catch { /* ignore file read errors */ }
            }

            var envVal = Environment.GetEnvironmentVariable("GENERATIVE_API_KEY");
            var appVal = ConfigurationManager.AppSettings["GenerativeApiKey"];
            var filePath = HostingEnvironment.MapPath("~/App_Data/GENERATIVE_API_KEY.txt");
            bool fileExists = false;
            try { fileExists = !string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath); } catch { }

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "REPLACE_WITH_SERVER_KEY")
            {
                // Build a diagnostic message (do not include the raw key)
                string envInfo = string.IsNullOrWhiteSpace(envVal) ? "no" : $"sí (longitud={envVal.Length})";
                string appInfo = string.IsNullOrWhiteSpace(appVal) || appVal == "REPLACE_WITH_SERVER_KEY" ? "no / placeholder" : $"sí (longitud={appVal.Length})";
                string fileInfo = fileExists ? "sí" : "no";

                var mensaje = "API key no configurada en el servidor. " +
                              $"Estado: envVar={envInfo}; appSettings={appInfo}; App_Data file={fileInfo}. " +
                              "Configura la variable de entorno GENERATIVE_API_KEY o añade la key segura en el servidor.";

                return Content(HttpStatusCode.Forbidden, new { mensaje = mensaje });
            }

            // Determine provider (google or openai). Default to google.
            var provider = (Environment.GetEnvironmentVariable("GENERATIVE_PROVIDER") ?? ConfigurationManager.AppSettings["GenerativeProvider"]) ?? "google";

            // Prepare prompt for providers
            var prompt = $"A partir de la siguiente actividad: Título: \"{req.Nombre}\", Descripción: \"{req.Descripcion}\", genera tres versiones mejoradas de la descripción, más claras, completas y bien estructuradas. Cada versión debe incluir detalles útiles sin cambiar el significado original. Devuelve SOLO las tres versiones numeradas (1, 2 y 3) sin texto adicional.";
            // If provider is Google, prepare Google Generative Language request
            var googleModel = ConfigurationManager.AppSettings["GoogleModel"] ?? "text-bison-001";
            lock (_forcedModelLock)
            {
                if (!string.IsNullOrWhiteSpace(_forcedModel)) googleModel = _forcedModel;
            }

            // Select correct Google endpoint and payload depending on model
            string googleUrl;
            string googleJson;
            if (googleModel != null && googleModel.IndexOf("gemini", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Gemini models use the v1beta generateContent endpoint and expect a 'contents' array
                googleUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{googleModel}:generateContent?key={apiKey}";
                var googleBody = new
                {
                    contents = new[] {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };
                googleJson = Newtonsoft.Json.JsonConvert.SerializeObject(googleBody);
            }
            else
            {
                // Other models (e.g. text-bison) use v1 generate with 'prompt' object
                googleUrl = $"https://generativelanguage.googleapis.com/v1/models/{googleModel}:generate?key={apiKey}";
                var googleBody = new
                {
                    prompt = new { text = prompt },
                    temperature = 0.7,
                    maxOutputTokens = 250
                };
                googleJson = Newtonsoft.Json.JsonConvert.SerializeObject(googleBody);
            }

            // OpenAI fallback preparation
            var openAiModel = ConfigurationManager.AppSettings["OpenAIModel"] ?? "gpt-3.5-turbo";
            var openAiUrl = "https://api.openai.com/v1/chat/completions";
            var openBody = new
            {
                model = openAiModel,
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 250,
                temperature = 0.7
            };
            var openJson = Newtonsoft.Json.JsonConvert.SerializeObject(openBody);
            try
            {
                if (provider.Equals("google", StringComparison.OrdinalIgnoreCase))
                {
                    // Try primary request first, then fall back to other model/endpoint combinations
                    var attempts = new List<object>();

                    var tried = new List<(string url, string payload)>();

                    // primary attempt (as built earlier)
                    tried.Add((googleUrl, googleJson));

                    // fallback: try gemini models on v1beta with contents
                    var fallbackModels = new[] { "gemini-2.5-flash", "gemini-1.5-flash" };
                    foreach (var fb in fallbackModels)
                    {
                        var urlFb = $"https://generativelanguage.googleapis.com/v1beta/models/{fb}:generateContent" + $"?key={apiKey}";
                        var bodyFb = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
                        var jsonFb = Newtonsoft.Json.JsonConvert.SerializeObject(bodyFb);
                        tried.Add((urlFb, jsonFb));
                    }

                    foreach (var t in tried)
                    {
                        try
                        {
                            using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, t.url))
                            {
                                reqMsg.Content = new StringContent(t.payload, Encoding.UTF8, "application/json");

                                var resp = await _client.SendAsync(reqMsg);
                                var txt = await resp.Content.ReadAsStringAsync();

                                string excerpt = (txt ?? string.Empty).Replace('\n', ' ').Replace('\r', ' ').Trim();
                                if (excerpt.Length > 600) excerpt = excerpt.Substring(0, 600) + "...";

                                attempts.Add(new { provider = "google", endpoint = t.url, status = (int)resp.StatusCode, reason = resp.ReasonPhrase, detalle = excerpt });

                                // log details
                                TryAppendLog("GenerarDescripcion_requests.log", $"URL: {t.url}\nPAYLOAD:\n{t.payload}\nSTATUS: {(int)resp.StatusCode} {resp.ReasonPhrase}\nRESPONSE:\n{txt}\n---\n");

                                if (resp.IsSuccessStatusCode)
                                {
                                    // parse response into wrapper
                                    string generated = null;
                                    try
                                    {
                                        var tok = JToken.Parse(txt);
                                        if (tok["candidates"] != null && tok["candidates"].HasValues)
                                        {
                                            var first = tok["candidates"].First;
                                            generated = first["content"]?.ToString() ?? first["output"]?.ToString() ?? first.ToString();
                                        }
                                        else if (tok["output"] != null)
                                        {
                                            generated = tok["output"].ToString();
                                        }
                                        else
                                        {
                                            generated = tok.ToString();
                                        }
                                    }
                                    catch { generated = txt; }

                                    var wrapper = new
                                    {
                                        candidates = new[]
                                        {
                                            new {
                                                content = new {
                                                    parts = new[] { new { text = generated } }
                                                }
                                            }
                                        }
                                    };

                                    return Ok(wrapper);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            attempts.Add(new { provider = "google", endpoint = t.url, status = -1, reason = "exception", detalle = ex.Message });
                            TryAppendLog("GenerarDescripcion_requests.log", $"EXCEPTION calling {t.url}: {ex.Message}\n");
                        }
                    }

                    // If we reach here none succeeded
                    return Content(HttpStatusCode.BadGateway, new { mensaje = "Error al consultar Google Generative Language API (intentos fallidos)", intentos = attempts });
                }

                // Fallback to OpenAI if provider is not Google
                using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, openAiUrl))
                {
                    reqMsg.Content = new StringContent(openJson, Encoding.UTF8, "application/json");
                    reqMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                    var resp = await _client.SendAsync(reqMsg);
                    var txt = await resp.Content.ReadAsStringAsync();

                    TryAppendLog("OpenAI_requests.log", $"URL: {openAiUrl}\nPAYLOAD:\n{openJson}\nSTATUS: {(int)resp.StatusCode} {resp.ReasonPhrase}\nRESPONSE:\n{txt}\n---\n");

                    if (!resp.IsSuccessStatusCode)
                    {
                        string excerpt = (txt ?? string.Empty).Replace('\n', ' ').Replace('\r', ' ').Trim();
                        if (excerpt.Length > 240) excerpt = excerpt.Substring(0, 240) + "...";
                        string friendly = "Error al consultar OpenAI API";
                        if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
                        {
                            friendly = "Clave API OpenAI inválida o sin permisos. Verifica la key configurada en el servidor.";
                        }
                        else if (resp.StatusCode == (HttpStatusCode)429)
                        {
                            friendly = "Límite de peticiones excedido (429) en OpenAI. Revisa cuotas o intenta más tarde.";
                        }

                        var messageToClient = string.IsNullOrWhiteSpace(excerpt) ? friendly : $"{friendly} - {excerpt}";
                        return Content(resp.StatusCode, new { mensaje = messageToClient, status = (int)resp.StatusCode, detalle = excerpt });
                    }

                    // Parse OpenAI chat response
                    string generated = null;
                    try
                    {
                        var tok = JToken.Parse(txt);
                        generated = tok["choices"]?[0]?["message"]?["content"]?.ToString();
                    }
                    catch { generated = txt; }

                    if (string.IsNullOrWhiteSpace(generated)) generated = txt;

                    var wrapper = new
                    {
                        candidates = new[]
                        {
                            new {
                                content = new {
                                    parts = new[] { new { text = generated } }
                                }
                            }
                        }
                    };

                    return Ok(wrapper);
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { mensaje = "Error interno", error = ex.Message });
            }
        }

        [HttpPost]
        [Route("GenerarContenido")]
        public async Task<IHttpActionResult> GenerarContenido([FromBody] JObject body)
        {
            if (body == null) return BadRequest("Request body required");

            // read key (env preferred)
            var apiKeyVar = Environment.GetEnvironmentVariable("GENERATIVE_API_KEY")
                            ?? ConfigurationManager.AppSettings["GenerativeApiKey"];

            // try App_Data fallback (development)
            if (string.IsNullOrWhiteSpace(apiKeyVar) || apiKeyVar == "REPLACE_WITH_SERVER_KEY")
            {
                try
                {
                    var localKeyPath = HostingEnvironment.MapPath("~/App_Data/GENERATIVE_API_KEY.txt");
                    if (!string.IsNullOrWhiteSpace(localKeyPath) && File.Exists(localKeyPath))
                    {
                        var fileKey = File.ReadAllText(localKeyPath).Trim();
                        if (!string.IsNullOrWhiteSpace(fileKey)) apiKeyVar = fileKey;
                    }
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(apiKeyVar) || apiKeyVar == "REPLACE_WITH_SERVER_KEY")
                return Content(HttpStatusCode.Forbidden, new { mensaje = "API key no configurada en el servidor (GenerarContenido)" });

            var model = body.Value<string>("model") ?? ConfigurationManager.AppSettings["GoogleModel"] ?? "text-bison-001";
            // Allow runtime forced model for testing (keeps behavior consistent with other endpoints)
            lock (_forcedModelLock)
            {
                if (!string.IsNullOrWhiteSpace(_forcedModel)) model = _forcedModel;
            }

            // Try to obtain service account token first; fall back to API key
            string serviceAccountPath = Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_FILE")
                                        ?? ConfigurationManager.AppSettings["GoogleServiceAccountPath"];
            string bearerToken = null;
            if (!string.IsNullOrWhiteSpace(serviceAccountPath) && File.Exists(serviceAccountPath))
            {
                try
                {
                    var gcred = GoogleCredential.FromFile(serviceAccountPath);
                    // ensure scoped credential
                    var scoped = gcred;
                    try { scoped = gcred.CreateScoped(new[] { "https://www.googleapis.com/auth/cloud-platform" }); } catch { /* ignore */ }

                    // Try ITokenAccess first
                    var tokenAccess = scoped as Google.Apis.Auth.OAuth2.ITokenAccess;
                    if (tokenAccess != null)
                    {
                        bearerToken = await tokenAccess.GetAccessTokenForRequestAsync();
                    }
                    else
                    {
                        // Fallback to ServiceAccountCredential
                        var sac = scoped.UnderlyingCredential as ServiceAccountCredential;
                        if (sac != null)
                        {
                            bearerToken = await sac.GetAccessTokenForRequestAsync(null);
                        }
                    }
                }
                catch
                {
                    bearerToken = null; // ignore and fallback to api key
                }
            }

            bool hasContents = body["contents"] != null;
            string url;
            string payload;

            // Detect gemini-like models which expect v1beta 'generateContent' with 'contents' array
            var effectiveModel = model ?? (ConfigurationManager.AppSettings["GoogleModel"] ?? "text-bison-001");
            var isGemini = !string.IsNullOrWhiteSpace(effectiveModel) && effectiveModel.IndexOf("gemini", StringComparison.OrdinalIgnoreCase) >= 0;

            if (hasContents)
            {
                // If the caller already supplied 'contents', call v1beta endpoint
                url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent" + (bearerToken == null ? $"?key={apiKeyVar}" : "");
                var clone = (JObject)body.DeepClone();
                clone.Remove("model");
                payload = clone.ToString();
            }
            else if (isGemini)
            {
                // Convert prompt/text into 'contents' shape expected by gemini models
                url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent" + (bearerToken == null ? $"?key={apiKeyVar}" : "");
                var clone = (JObject)body.DeepClone();
                clone.Remove("model");

                // Extract text from known locations
                string textVal = null;
                try
                {
                    if (clone["prompt"] != null)
                    {
                        textVal = clone["prompt"]?["text"]?.ToString();
                    }
                }
                catch { }
                if (string.IsNullOrWhiteSpace(textVal) && clone["text"] != null)
                {
                    textVal = clone.Value<string>("text");
                }

                var contentsObj = new JObject(
                    new JProperty("contents", new JArray(
                        new JObject(new JProperty("parts", new JArray(new JObject(new JProperty("text", textVal ?? "")))))
                    ))
                );
                payload = contentsObj.ToString();
            }
            else
            {
                // Default: call v1 generate endpoint with prompt object (for text-bison and similar)
                url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generate" + (bearerToken == null ? $"?key={apiKeyVar}" : "");
                var clone = (JObject)body.DeepClone();
                clone.Remove("model");
                if (clone["prompt"] == null && clone["text"] != null)
                {
                    clone = new JObject(new JProperty("prompt", new JObject(new JProperty("text", clone.Value<string>("text") ?? ""))));
                }
                payload = clone.ToString();
            }

            try
            {
                using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    reqMsg.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                    if (!string.IsNullOrWhiteSpace(bearerToken)) reqMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                    var resp = await _client.SendAsync(reqMsg);
                    var txt = await resp.Content.ReadAsStringAsync();

                    // Log request/response for debugging (writes to App_Data/GenerarContenido_requests.log)
                    try
                    {
                        TryAppendLog("GenerarContenido_requests.log", $"URL: {url}\nPAYLOAD:\n{payload}\nSTATUS: {(int)resp.StatusCode} {resp.ReasonPhrase}\nRESPONSE:\n{txt}\n---\n");
                    }
                    catch { }

                    // If provider returned an empty body or non-success, try gemini v1beta fallbacks (if not already using gemini)
                    if ((!resp.IsSuccessStatusCode || string.IsNullOrWhiteSpace(txt)) && !isGemini)
                    {
                        var fallbackModels = new[] { "gemini-2.5-flash", "gemini-1.5-flash" };

                        // Extract a representative text from the incoming body
                        string textForFallback = null;
                        try { textForFallback = body["prompt"]?["text"]?.ToString(); } catch { }
                        if (string.IsNullOrWhiteSpace(textForFallback) && body["text"] != null)
                        {
                            textForFallback = body.Value<string>("text");
                        }
                        // If still empty and messages array provided (OpenAI style), concatenate contents
                        if (string.IsNullOrWhiteSpace(textForFallback) && body["messages"] != null)
                        {
                            try
                            {
                                var msgs = body["messages"] as JArray;
                                if (msgs != null)
                                {
                                    var sb = new StringBuilder();
                                    foreach (var m in msgs)
                                    {
                                        var c = m["content"]?.ToString();
                                        if (!string.IsNullOrWhiteSpace(c))
                                        {
                                            if (sb.Length > 0) sb.Append("\n");
                                            sb.Append(c);
                                        }
                                    }
                                    textForFallback = sb.ToString();
                                }
                            }
                            catch { }
                        }

                        foreach (var fm in fallbackModels)
                        {
                            try
                            {
                                var urlFb = $"https://generativelanguage.googleapis.com/v1beta/models/{fm}:generateContent" + (bearerToken == null ? $"?key={apiKeyVar}" : "");
                                var contentsObj = new JObject(
                                    new JProperty("contents", new JArray(
                                        new JObject(new JProperty("parts", new JArray(new JObject(new JProperty("text", textForFallback ?? "")))) )
                                    ))
                                );
                                var payloadFb = contentsObj.ToString();

                                using (var reqFb = new HttpRequestMessage(HttpMethod.Post, urlFb))
                                {
                                    reqFb.Content = new StringContent(payloadFb, Encoding.UTF8, "application/json");
                                    if (!string.IsNullOrWhiteSpace(bearerToken)) reqFb.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                                    var respFb = await _client.SendAsync(reqFb);
                                    var txtFb = await respFb.Content.ReadAsStringAsync();

                                    try { TryAppendLog("GenerarContenido_requests.log", $"FALLBACK URL: {urlFb}\nPAYLOAD:\n{payloadFb}\nSTATUS: {(int)respFb.StatusCode} {respFb.ReasonPhrase}\nRESPONSE:\n{txtFb}\n---\n"); } catch { }

                                    if (respFb.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(txtFb))
                                    {
                                        try
                                        {
                                            var parsedFb = JToken.Parse(txtFb);
                                            return Ok(parsedFb ?? (object)new { detalle = txtFb });
                                        }
                                        catch
                                        {
                                            return Ok(new { detalle = txtFb });
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                TryAppendLog("GenerarContenido_requests.log", $"FALLBACK EXCEPTION calling {fm}: {ex.Message}\n");
                            }
                        }
                    }

                    // If provider returned an empty body, return a clearer message to the client
                    if (string.IsNullOrWhiteSpace(txt))
                    {
                        var info = new { mensaje = "Respuesta vacía del proveedor generativo", status = (int)resp.StatusCode };
                        return Content(resp.StatusCode, info);
                    }

                    try
                    {
                        var parsed = JToken.Parse(txt);
                        if (resp.IsSuccessStatusCode) return Ok(parsed ?? (object)new { detalle = txt });
                        return Content(resp.StatusCode, parsed ?? (object)new { detalle = txt });
                    }
                    catch
                    {
                        if (resp.IsSuccessStatusCode) return Ok(new { detalle = txt });
                        return Content(resp.StatusCode, new { detalle = txt });
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { mensaje = "Error interno al generar contenido", error = ex.Message });
            }
        }

        public class DescripcionRequest
        {
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
        }

        private void TryAppendLog(string filename, string content)
        {
            try
            {
                lock (_logLock)
                {
                    var path = HostingEnvironment.MapPath("~/App_Data/") ?? AppDomain.CurrentDomain.BaseDirectory;
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        var full = System.IO.Path.Combine(path, filename);
                        System.IO.File.AppendAllText(full, DateTime.UtcNow.ToString("o") + " " + content + Environment.NewLine);
                    }
                }
            }
            catch
            {
                // never throw from logging
            }
        }

        [HttpPost]
        [Route("ForceModel")]
        public IHttpActionResult ForceModel([FromBody] string modelName)
        {
            if (string.IsNullOrWhiteSpace(modelName)) return BadRequest("modelName required");
            // Repurpose to force the OpenAI model name at runtime
            lock (_forcedModelLock) { _forcedModel = modelName.Trim(); }
            return Ok(new { mensaje = "Modelo OpenAI forzado", model = _forcedModel });
        }

        [HttpPost]
        [Route("ClearForcedModel")]
        public IHttpActionResult ClearForcedModel()
        {
            lock (_forcedModelLock) { _forcedModel = null; }
            return Ok(new { mensaje = "Modelo forzado limpiado" });
        }

        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            return Ok(new { mensaje = "pong" });
        }

        [HttpGet]
        [Route("diagnostic")]
        public async Task<IHttpActionResult> Diagnostic()
        {
            var apiKey = Environment.GetEnvironmentVariable("GENERATIVE_API_KEY")
                         ?? ConfigurationManager.AppSettings["GenerativeApiKey"];

            // try App_Data fallback (development)
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "REPLACE_WITH_SERVER_KEY")
            {
                try
                {
                    var localKeyPath = HostingEnvironment.MapPath("~/App_Data/GENERATIVE_API_KEY.txt");
                    if (!string.IsNullOrWhiteSpace(localKeyPath) && File.Exists(localKeyPath))
                    {
                        var fileKey = File.ReadAllText(localKeyPath).Trim();
                        if (!string.IsNullOrWhiteSpace(fileKey)) apiKey = fileKey;
                    }
                }
                catch { }
            }

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "REPLACE_WITH_SERVER_KEY")
            {
                return Content(HttpStatusCode.Forbidden, new { mensaje = "API key no configurada (diagnostic)", env = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GENERATIVE_API_KEY")), appSettings = !(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["GenerativeApiKey"]) || ConfigurationManager.AppSettings["GenerativeApiKey"] == "REPLACE_WITH_SERVER_KEY") });
            }

            // Select provider for diagnostic (default to google)
            var provider = (Environment.GetEnvironmentVariable("GENERATIVE_PROVIDER") ?? ConfigurationManager.AppSettings["GenerativeProvider"]) ?? "google";

            var results = new List<object>();
            var targetModel = string.Empty;
            try
            {
                if (provider.Equals("openai", StringComparison.OrdinalIgnoreCase))
                {
                    // Test OpenAI connectivity
                    var openAiModel = ConfigurationManager.AppSettings["OpenAIModel"] ?? "gpt-3.5-turbo";
                    var openAiUrl = "https://api.openai.com/v1/chat/completions";
                    var openBody = new { model = openAiModel, messages = new[] { new { role = "user", content = "Prueba breve" } }, max_tokens = 8 };
                    var openJson = Newtonsoft.Json.JsonConvert.SerializeObject(openBody);
                    using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, openAiUrl))
                    {
                        reqMsg.Content = new StringContent(openJson, Encoding.UTF8, "application/json");
                        reqMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

                        var resp = await _client.SendAsync(reqMsg);
                        var txt = await resp.Content.ReadAsStringAsync();
                        string excerpt = (txt ?? string.Empty).Replace('\n',' ').Replace('\r',' ').Trim();
                        if (excerpt.Length > 600) excerpt = excerpt.Substring(0, 600) + "...";
                        results.Add(new { provider = "openai", endpoint = openAiUrl, status = (int)resp.StatusCode, reason = resp.ReasonPhrase, detalle = excerpt });
                        targetModel = openAiModel;
                    }
                }
                else if (provider.Equals("google", StringComparison.OrdinalIgnoreCase))
                {
                    // Test Google Generative Language connectivity more thoroughly:
                    // try both v1 and v1beta endpoints for the configured model, and try a few fallback models
                    var configuredModel = ConfigurationManager.AppSettings["GoogleModel"] ?? "text-bison-001";
                    lock (_forcedModelLock)
                    {
                        if (!string.IsNullOrWhiteSpace(_forcedModel)) configuredModel = _forcedModel;
                    }

                    var baseEndpoints = new[] {
                        "https://generativelanguage.googleapis.com/v1/models/",
                        "https://generativelanguage.googleapis.com/v1beta/models/"
                    };

                    var modelsToTry = new List<string> { configuredModel, "gemini-2.5-flash", "gemini-1.5-flash" };

                    foreach (var m in modelsToTry)
                    {
                        foreach (var baseUrl in baseEndpoints)
                        {
                            var url = baseUrl + m + (baseUrl.Contains("v1beta") ? ":generateContent?key=" : ":generate?key=") + apiKey;
                            object bodyObj;
                            if (baseUrl.Contains("v1beta"))
                            {
                                bodyObj = new
                                {
                                    contents = new[] {
                                        new { parts = new[] { new { text = "Prueba breve" } } }
                                    }
                                };
                            }
                            else
                            {
                                bodyObj = new { prompt = new { text = "Prueba breve" }, maxOutputTokens = 8, temperature = 0.7 };
                            }
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(bodyObj);

                            try
                            {
                                using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, url))
                                {
                                    reqMsg.Content = new StringContent(json, Encoding.UTF8, "application/json");

                                    var resp = await _client.SendAsync(reqMsg);
                                    var txt = await resp.Content.ReadAsStringAsync();

                                    string excerpt = (txt ?? string.Empty).Replace('\n',' ').Replace('\r',' ').Trim();
                                    if (excerpt.Length > 600) excerpt = excerpt.Substring(0, 600) + "...";

                                    results.Add(new { provider = "google", endpoint = url, model = m, status = (int)resp.StatusCode, reason = resp.ReasonPhrase, detalle = excerpt });
                                    if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300)
                                    {
                                        targetModel = m;
                                        // stop trying further
                                        goto EndGoogleDiagnostic;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                results.Add(new { provider = "google", endpoint = url, model = m, status = -1, reason = "exception", detalle = ex.Message });
                            }
                        }
                    }
                    EndGoogleDiagnostic: ;
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, new { mensaje = "Proveedor desconocido. Usa 'google' o 'openai'." });
                }

                // Return diagnostic including raw responses to help debugging
                return Content(HttpStatusCode.OK, new { mensaje = "Diagnostic result (detailed)", targetModel = targetModel, provider = provider, intentos = results });
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, new { mensaje = "Diagnostic error", error = ex.Message });
            }
        }
    }
}
