using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlActividades.Models;
using ControlActividades.Models.db;

namespace ControlActividades.Interfaces.Materias
{
    public interface IMateriasService
    {
        Task<List<MateriaCARes>> ObtenerMateriasSinGrupoPorUsuario(int ca_usuarioId, int st_usuarioId, string role);

        Task< MateriaCARes> ObtenerMateriaDetalles(int materiaId, int grupoId, string role, int ca_usuarioId, int st_usuarioId);

        Task<List<AlumnoCorreo>> BuscarAlumnosPorCorreo(string query);
       
        Task<ActividadRes> CrearActividadAsync(ActividadDTO actividad);

        Task<EntregablesPartialViewModel> ObtenerEntregablesAlumno(int materiaId);
    }
}
