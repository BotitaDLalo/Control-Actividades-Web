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
        List<MateriaViewModel> ObtenerMateriasSinGrupoPorUsuario(int usuarioId, string role);

        Task<MateriaViewModel> ObtenerMateriaDetalles(int materiaId, int docenteId);

        Task<ActividadRes> CrearActividadAsync(ActividadDTO actividad);
    }
}
