using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using ControlActividades.Interfaces.Grupos;
using ControlActividades.Models;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt;
using Org.BouncyCastle.Asn1.Ocsp;

namespace ControlActividades.Services.Grupos
{
    public class GruposApiSTService : IGruposApiService
    {

        private static readonly ControlActividades.Recursos.FuncionalidadesGenerales fg = new Recursos.FuncionalidadesGenerales();
        private static readonly string url = fg.ObtenerUrlST();
        private static readonly string apiKey = fg.ObtenerXApiKey();
        private static readonly string View = Views.MOVIL;
        public async Task<List<GruposCreadoCARes>> ObtenerGruposCreados(int ca_usuarioId, int st_usuarioId)
        {
            string query = url + "ObtenerGruposCreados";
            try
            {
                string response = string.Empty;

                var model = new ObtenerGrupoCreadoCARequest
                {
                    ca_usuarioId = ca_usuarioId,
                    st_usuarioId = st_usuarioId
                };

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                    HttpResponseMessage res = await client.PostAsync(query, content);

                    if (!res.IsSuccessStatusCode)
                    {
                        throw new Exception();
                    }
                    response = await res.Content.ReadAsStringAsync();
                }

                var lsGrupos = JsonConvert.DeserializeObject<List<GruposCreadoCARes>>(response);

                return lsGrupos;
            }
            catch (Exception)
            {
                return new List<GruposCreadoCARes>();
            }
        }

        public async Task<List<GruposCARes>> ObtenerGruposMaterias(int ca_usuarioId, int st_usuarioId, string role)
        {
            string query = url + "ObtenerGruposPorUsuario";
            try
            {
                string response = string.Empty;

                var model = new ObtenerGruposPorUsuarioRequest()
                {
                    Role = role,
                    st_usuarioId = st_usuarioId,
                    ca_usuarioId = ca_usuarioId,
                    View = View,
                };

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                    HttpResponseMessage res = await client.PostAsync(query, content);

                    if (!res.IsSuccessStatusCode)
                    {
                        throw new Exception();
                    }
                    response = await res.Content.ReadAsStringAsync();
                }

                var lsGrupos = JsonConvert.DeserializeObject<List<GruposCARes>>(response);

                return lsGrupos;
            }
            catch (Exception)
            {
                return new List<GruposCARes>();
            }
        }
    }
}