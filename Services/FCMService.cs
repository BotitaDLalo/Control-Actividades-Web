using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;

namespace ControlActividades.Services
{
    public class FCMService
    {
        private readonly string _projectId;
        private readonly GoogleCredential _googleCredential;
        private readonly string uriApiGoogle = "https://www.googleapis.com/auth/firebase.messaging";
        private readonly string uriFile = HostingEnvironment.MapPath("~/App_Data/push-notification-9bc5f-firebase-adminsdk-es74b-f758cc2102.json");

        public FCMService()
        {
            _projectId = "push-notification-9bc5f";
            _googleCredential = GoogleCredential.FromFile(uriFile).CreateScoped(uriApiGoogle);
        }

        private async Task<string> GetAccessTokenAsync()
        {
            return await _googleCredential.UnderlyingCredential.GetAccessTokenForRequestAsync();
        }

        public async Task<bool> SendNotificationAsync(string targetToken, string title, string body)
        {
            try
            {
                var accessToken = await GetAccessTokenAsync();

                var message = new
                {
                    message = new
                    {
                        token = targetToken,
                        notification = new
                        {
                            title,
                            body
                        }
                    }
                };

                string jsonMessage = JsonConvert.SerializeObject(message);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    var url = $"https://fcm.googleapis.com/v1/projects/{_projectId}/messages:send";

                    var response = await client.PostAsync(url, new StringContent(jsonMessage, Encoding.UTF8, "application/json"));

                    return response.IsSuccessStatusCode;
                }

            }catch (Exception)
            {
                return false;
            }
        }
    }
}