using ControlActividades.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControlActividades.Interfaces.Actividades
{
    public interface IActividadesService
    {
        Task<List<ActividadRes>> ObtenerActividadesPorMateria(int materiaId, string rol);

        Task<ActividadDetallesRes>ObtenerActividadPorId(int actividadId);

        Task<ActividadRes> ActualizarActividad(int id, ActividadDTO actividad);
        
        Task EliminarActividadAsync(int id);

        //Task<List<AlumnoDTO>> AlumnosParaCalificarActividad(int materiaId);
    }
}
