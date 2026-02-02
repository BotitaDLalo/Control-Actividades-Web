using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces;
using ControlActividades.Models;
using Newtonsoft.Json;

namespace ControlActividades.Services
{
    public class GruposSTService : IGruposService
    {

        private static readonly ControlActividades.Recursos.FuncionalidadesGenerales fg = new Recursos.FuncionalidadesGenerales();
        private static readonly string url = fg.ObtenerUrlST();
        private static readonly string apiKey = fg.ObtenerXApiKey(); 
        private static readonly string View = Views.WEB;

        public async Task<List<GruposCARes>> ObtenerGruposPorUsuario(string role, int ca_usuarioId, int st_usuarioId)
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

        public async Task<List<MateriaCARes>> ObtenerMateriasPorGrupo(int grupoId, int ca_usuarioId, int st_usuarioId, string role)
        {
            string query = url + "ObtenerMateriasPorGrupo";
            try
            {
                string response = string.Empty;

                var model = new ObtenerMateriasPorGrupoRequest
                {
                    Role = role,
                    ca_usuarioId = ca_usuarioId,
                    st_usuarioId = st_usuarioId,
                    GrupoId = grupoId
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

                var lsMaterias = JsonConvert.DeserializeObject<List<MateriaCARes>>(response);

                return lsMaterias;
            }
            catch (Exception)
            {
                return new List<MateriaCARes>();
            }
        }

        public async Task<bool> TieneGrupos(string role, int ca_usuarioId, int st_usuarioId)
        {
            string query = url + "TieneGrupos";


            try
            {
                string response = string.Empty;

                var model = new TieneClasesRequest()
                {
                    Role = role,
                    st_usuarioId = st_usuarioId,
                    ca_usuarioId = ca_usuarioId,
                };

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                    HttpResponseMessage res = await client.PostAsync(query, content);

                    response = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {
                        throw new Exception();
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> TieneMaterias(string role, int ca_usuarioId, int st_usuarioId)
        {
            string query = url + "TieneMaterias";
            try
            {
                string response = string.Empty;

                var model = new TieneClasesRequest()
                {
                    Role = role,
                    st_usuarioId = st_usuarioId,
                    ca_usuarioId = ca_usuarioId,
                };

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                    HttpResponseMessage res = await client.PostAsync(query, content);

                    response = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {
                        throw new Exception();
                    }
                    
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}