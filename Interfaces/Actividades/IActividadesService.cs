using ControlActividades.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ControlActividades.Interfaces.Actividades
{
    public interface IActividadesService
    {
        Task<List<ActividadRes>> ObtenerActividadesPorMateria(int st_usuarioId,int materiaId, string rol, int grupoId = 0);

        Task<ActividadDetallesRes>ObtenerActividadPorId(int actividadId);

        Task<ActividadRes> ActualizarActividad(int id, ActividadDTO actividad);
        
        Task EliminarActividad(int id);

        Task<DetallesActividadRes> ObtenerDetallesActividad(int actividadId);

        //Task<List<AlumnoDTO>> AlumnosParaCalificarActividad(int materiaId);
    }
}
