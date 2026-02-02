using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Materias;
using ControlActividades.Models;
using Newtonsoft.Json;

namespace ControlActividades.Services.Materias
{
    public class MateriasApiSTService : IMateriasApiService
    {
        private static readonly ControlActividades.Recursos.FuncionalidadesGenerales fg = new Recursos.FuncionalidadesGenerales();
        private static readonly string url = fg.ObtenerUrlST();
        private static readonly string apiKey = fg.ObtenerXApiKey();
        private static readonly string View = Views.MOVIL;
        public async Task<List<MateriaCARes>> ObtenerMaterias(int ca_usuarioId, int st_usuarioId, string role)
        {
            string query = url + "ObtenerMateriasSinGrupo";
            try
            {
                string response = string.Empty;

                var model = new ObtenerMateriasCARequest
                {
                    ca_usuarioId = ca_usuarioId,
                    st_usuarioId = st_usuarioId,
                    Role = role,
                    View = View
                };

                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("X-API-KEY",apiKey);
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
    }
}