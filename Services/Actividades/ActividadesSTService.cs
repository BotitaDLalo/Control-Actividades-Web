using ControlActividades.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ControlActividades.Services.Actividades
{
    public class ActividadesSTService
    {
        private static readonly ControlActividades.Recursos.FuncionalidadesGenerales fg = new Recursos.FuncionalidadesGenerales();
        private static readonly string url = fg.ObtenerUrlST();
        private static readonly string apiKey = fg.ObtenerXApiKey();
        private static readonly string View = Views.WEB;

        private ApplicationDbContext _db;

        public ApplicationDbContext Db
        {
            get
            {
                return _db ?? (_db = new ApplicationDbContext());
            }
            private set
            {
                _db = value;
            }
        }
        
        public async Task<List<ActividadRes>> ObtenerActividadesPorMateria(int materiaId, string rol)
        {
            string query = url + "ObtenerActividadesPorMateria";

            try
            {
                string response = string.Empty;

                var model = new ObtenerActividadesPorMateriaRequest()
                {
                    Role = rol,
                    MateriaId = materiaId,
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

                var lsActividades = JsonConvert.DeserializeObject<List<ActividadRes>>(response);

                return lsActividades;
            }
            catch (Exception)
            {
                return new List<ActividadRes>();
            }

        }

        public async Task<ActividadDetallesRes> ObtenerActividadPorId(int actividadId)
        {
            string query = url + "ObtenerActividadPorId";
            
            try
            {
                string response = string.Empty;
                
                var model = new ObtenerActividadPorIdRequest()
                {
                    ActividadId = actividadId,
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

                var actividad = JsonConvert.DeserializeObject<ActividadDetallesRes>(response);

                return actividad;
            }
            catch (Exception)
            {
                return new ActividadDetallesRes();
            }
            
        }

        public async Task<ActividadRes> ActualizarActividad(int actividadId, ActividadDTO model)
        {
            throw new NotImplementedException();
        }

        public async Task EliminarActividadAsync (int id)
        {
            throw new NotImplementedException();
        }
    }
}