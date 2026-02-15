using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Actividades;
using ControlActividades.Models;
using ControlActividades.Models.db;

namespace ControlActividades.Services.Actividades
{
    public class ActividadesApiSTService : IActividadesApiService
    {
        public Task<ActividadesDTO> ActualizarActividad(int id, tbActividades updatedActivity)
        {
            throw new NotImplementedException();
        }

        public Task AsignarCalificacion(int entregableId, decimal calificacion)
        {
            throw new NotImplementedException();
        }

        public Task CrearActividad(tbActividades nuevaActividad)
        {
            throw new NotImplementedException();
        }

        public Task EliminarActividad(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<ObtenerActividadesPorMateriaRes>> ObtenerActividadesPorMateria(int materiaId)
        {
            throw new NotImplementedException();
        }

        public Task<RespuestaAlumnosEntregables> ObtenerAlumnosEntregables(int actividadId)
        {
            throw new NotImplementedException();
        }

        public Task<ObtenerEnviosActividadesAlumnoRes> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            throw new NotImplementedException();
        }

        public Task QuitarCalificacion(int entregableId)
        {
            throw new NotImplementedException();
        }
    }
}