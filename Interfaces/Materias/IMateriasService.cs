using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlActividades.Models;

namespace ControlActividades.Interfaces.Materias
{
    public interface IMateriasService
    {
        List<MateriaViewModel> ObtenerMateriasSinGrupoPorUsuario(int usuarioId, string role);

        Task<MateriaViewModel> ObtenerMateriaDetalles(int materiaId, int docenteId);
    }
}
