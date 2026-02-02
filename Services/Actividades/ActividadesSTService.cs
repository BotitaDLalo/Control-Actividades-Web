using ControlActividades.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Services.Actividades
{
    public class ActividadesSTService
    {
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
        
        public async Task<List<ActividadRes>> ObtenerActividadesPorMateria(int materiaId, bool esDocente)
        {
            throw new NotImplementedException();
        }

        public async Task<ActividadDetallesRes> ObtenerActividadPorId(int actividadId)
        {
            throw new NotImplementedException();
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