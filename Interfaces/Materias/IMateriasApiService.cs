using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlActividades.Models;

namespace ControlActividades.Interfaces.Materias
{
    public interface IMateriasApiService
    {
        Task<List<MateriaCARes>> ObtenerMaterias(int ca_usuarioId, int st_usuarioId, string role);
    }
}
