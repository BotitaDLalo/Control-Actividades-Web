using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlActividades.Models;
using ControlActividades.Models.db;

namespace ControlActividades.Interfaces.Actividades
{
    public interface IActividadesApiService
    {
        Task<ObtenerEnviosActividadesAlumnoRes> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId);

        Task<List<ObtenerActividadesPorMateriaRes>> ObtenerActividadesPorMateria(int materiaId);

        Task<RespuestaAlumnosEntregables> ObtenerAlumnosEntregables(int actividadId);

        Task AsignarCalificacion(int entregableId, decimal calificacion);

        Task QuitarCalificacion(int entregableId);


        Task CrearActividad(tbActividades nuevaActividad);

        Task<ActividadesDTO> ActualizarActividad(int id, tbActividades updatedActivity);

        Task EliminarActividad(int id);
    }
}
