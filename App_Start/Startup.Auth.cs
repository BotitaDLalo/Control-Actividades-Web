using ControlActividades.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using System;
using System.Configuration;
using System.IO;
using System.Web.Hosting;

namespace ControlActividades
{
    public partial class Startup
    {
        
        public void ConfigureAuth(IAppBuilder app)
        {
            
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.CreatePerOwinContext<RoleManager<IdentityRole>>((options, context) =>
            {
                var roleStore = new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>());
                return new RoleManager<IdentityRole>(roleStore);
            });

            // Permitir que la aplicación use una cookie para almacenar información para el usuario que inicia sesión
            // y una cookie para almacenar temporalmente información sobre un usuario que inicia sesión con un proveedor de inicio de sesión de terceros
            // Configurar cookie de inicio de sesión
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Permite a la aplicación validar la marca de seguridad cuando el usuario inicia sesión.
                    // Es una característica de seguridad que se usa cuando se cambia una contraseña o se agrega un inicio de sesión externo a la cuenta.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });            
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Permite que la aplicación almacene temporalmente la información del usuario cuando se verifica el segundo factor en el proceso de autenticación de dos factores.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Permite que la aplicación recuerde el segundo factor de verificación de inicio de sesión, como el teléfono o correo electrónico.
            // Cuando selecciona esta opción, el segundo paso de la verificación del proceso de inicio de sesión se recordará en el dispositivo desde el que ha iniciado sesión.
            // Es similar a la opción Recordarme al iniciar sesión.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            var googleClientId = Environment.GetEnvironmentVariable("GoogleClientId")
                                 ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID")
                                 ?? ConfigurationManager.AppSettings["GoogleClientId"];

            var googleClientSecret = Environment.GetEnvironmentVariable("GoogleClientSecret")
                                 ?? Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
                                 ?? ConfigurationManager.AppSettings["GoogleClientSecret"];

            // Registrar el middleware SOLO si ambos valores están presentes y no vacíos
            if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
            {
                app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
                {
                    ClientId = googleClientId,
                    ClientSecret = googleClientSecret
                });
            }
            else
            {
            }

            // Background validation of Generative API key and basic connectivity to the HuggingFace Inference endpoint.
            try
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        var apiKey = Environment.GetEnvironmentVariable("GENERATIVE_API_KEY")
                                     ?? ConfigurationManager.AppSettings["GenerativeApiKey"];

                        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "REPLACE_WITH_SERVER_KEY")
                        {
                            try
                            {
                                var filePath = HostingEnvironment.MapPath("~/App_Data/GENERATIVE_API_KEY.txt");
                                if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                                {
                                    var fileKey = File.ReadAllText(filePath).Trim();
                                    if (!string.IsNullOrWhiteSpace(fileKey)) apiKey = fileKey;
                                }
                            }
                            catch { }
                        }
                        var model = ConfigurationManager.AppSettings["HuggingFaceModel"] ?? "gpt2";
                        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "REPLACE_WITH_SERVER_KEY")
                        {
                            System.Diagnostics.Trace.TraceWarning("Generative API key no configurada. Las funciones IA no estarán disponibles.");
                            return;
                        }

                        using (var http = new System.Net.Http.HttpClient())
                        {
                            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                            http.Timeout = TimeSpan.FromSeconds(6);
                            // Hacer una petición HEAD/GET ligera para comprobar si el endpoint responde
                            var url = $"https://router.huggingface.co/models/{model}";
                            try
                            {
                                var r = await http.GetAsync(url);
                                if (!r.IsSuccessStatusCode)
                                {
                                    var body = await r.Content.ReadAsStringAsync();
                                    System.Diagnostics.Trace.TraceWarning($"HuggingFace connectivity check: {(int)r.StatusCode} {r.StatusCode} - {body}");
                                }
                                else
                                {
                                    System.Diagnostics.Trace.TraceInformation("HuggingFace connectivity check OK");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Trace.TraceWarning($"HuggingFace connectivity check failed: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceWarning($"Error durante verificación background de la key generativa: {ex.Message}");
                    }
                });
            }
            catch { }
        }
    }
}