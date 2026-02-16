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
    public class MateriasSTService : IMateriasService
    {

        private static readonly ControlActividades.Recursos.FuncionalidadesGenerales fg = new Recursos.FuncionalidadesGenerales();
        private static readonly string url = fg.ObtenerUrlST();
        private static readonly string apiKey = fg.ObtenerXApiKey();
        private static readonly string View = Views.WEB;

        public Task<List<AlumnoCorreo>> BuscarAlumnosPorCorreo(string query)
        {
            throw new NotImplementedException();
        }

        public async Task<MateriaCARes> ObtenerMateriaDetalles(int materiaId, int grupoId, string role, int ca_usuarioId, int st_usuarioId)
        {
            string query = url + "ObtenerMateriaDetalles";
            try
            {
                string response = string.Empty;

                var model = new ObtenerMateriaDetallesRequest()
                {
                    MateriaId = materiaId,
                    GrupoId = grupoId,
                    View = View,
                    st_usuarioId = st_usuarioId,
                    Role = role
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

                var materia = JsonConvert.DeserializeObject<MateriaCARes>(response);

                return materia;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<MateriaCARes>> ObtenerMateriasSinGrupoPorUsuario(int ca_usuarioId, int st_usuarioId, string role)
        {
            try
            {
                string query = url + "ObtenerMateriasSinGrupo";
                string response = string.Empty;

                ObtenerMateriasCARequest model = new ObtenerMateriasCARequest()
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
                    client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

                    HttpResponseMessage res = await client.PostAsync(query, content);

                    if (!res.IsSuccessStatusCode)
                    {
                        throw new Exception();
                    }
                    response = await res.Content.ReadAsStringAsync();
                }

                var materias = JsonConvert.DeserializeObject<List<MateriaCARes>>(response);

                return materias;
            }
            catch (Exception)
            {
                return new List<MateriaCARes>();
            }
        }


        public Task<ActividadRes> CrearActividadAsync(ActividadDTO actividad)
        {
            throw new NotImplementedException();
        }

        public Task<EntregablesPartialViewModel> ObtenerEntregablesAlumno(int materiaId)
        {
            throw new NotImplementedException();
        }
    }
}