using ControlActividades.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ControlActividades.Services.Actividades
{
    public class ActividadesCAService
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

        public async Task EliminarActividadAsync(int id)
        {
            try
            {
                var activity = await Db.tbActividades.FirstOrDefaultAsync(a => a.ActividadId == id);
                if (activity == null)
                {
                    throw new KeyNotFoundException("Actividad no encontrada.");
                }

                //var alumnoActividad = await Db.tbAlumnosActividades.FirstOrDefaultAsync(a => a.ActividadId == activity.ActividadId);
                var existenEntregas = await Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == activity.ActividadId).AnyAsync();
                if (existenEntregas)
                {
                    throw new InvalidOperationException("No se puede eliminar la actividad porque ya tiene entregas de alumnos.");
                }

                Db.tbActividades.Remove(activity);
                await Db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar la actividad: " + ex.Message);
            }

        }

    }
}