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
    public class FCMService
    {
        private static bool _initialized = false;

        public FCMService()
        {
            InicializarFirebase();
        }

        private void InicializarFirebase()
        {
            if (_initialized)
                return;

            var rutaArchivo = HostingEnvironment.MapPath("~/App_Data/apptokens-dc835-firebase-adminsdk-fbsvc-df059391ca.json");

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(rutaArchivo)
            });

            _initialized = true;
        }

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
        }
    }
}