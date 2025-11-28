using ControlActividades.Models;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace ControlActividades.Services
{
    public class FCMService : IDisposable
    {
        private static bool _initialized = false;
        private readonly ApplicationDbContext _db;

        public FCMService()
        {
            InicializarFirebase();
            _db = new ApplicationDbContext();
        }

        private void InicializarFirebase()
        {
            if (_initialized)
                return;

            var rutaArchivo = HostingEnvironment.MapPath("~/App_Data/push-notification-9bc5f-firebase-adminsdk-es74b-f758cc2102.json");

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(rutaArchivo)
            });

            _initialized = true;
        }
        /*
        public async Task<bool> SendNotificationAsync(string targetToken, string title, string body)
        {
            try
            {
                var message = new Message
                {
                    Token = targetToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error FCM: " + ex.Message, ex);
            }
        }*/

        // Enviar notificaciones por lotes
        public async Task SendBatchNotificationsAsync(List<string> targetTokens, string title, string body)
        {
            var messages = targetTokens.Select(token => new Message
            {
                Token = token,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                }
            }).ToList();
            var response = await FirebaseMessaging.DefaultInstance.SendEachAsync(messages);

            // Manejar errores individuales si es necesario
            for (int i = 0; i < response.Responses.Count; i++)
            {
                var result = response.Responses[i];

                if (!result.IsSuccess)
                {
                    var token = targetTokens[i];
                    var error = result.Exception;

                    Console.WriteLine($"Error enviando a {token}: {error}");
                    if (error is FirebaseMessagingException fcmEx) {
                        if (fcmEx.ErrorCode == ErrorCode.NotFound ||
                            fcmEx.ErrorCode == ErrorCode.InvalidArgument)
                        {
                            // ELIMINAR TOKEN DE LA BASE DE DATOS
                            await EliminarTokenInvalido(token);
                        }
                    }
                    
                }
            }

        }

        // eliminar token inválido de la base de datos
        private async Task EliminarTokenInvalido(string token)
        {
            var tokens = _db.tbUsuariosFcmTokens.Where(t => t.Token == token).ToList();
            if (tokens.Any())
            {
                _db.tbUsuariosFcmTokens.RemoveRange(tokens);
                await _db.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}