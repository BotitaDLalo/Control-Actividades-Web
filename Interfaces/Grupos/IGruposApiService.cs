using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlActividades.Models;

namespace ControlActividades.Interfaces.Grupos
{
    public interface IGruposApiService
    {
        Task<List<GruposCARes>> ObtenerGruposMaterias(int ca_usuarioId, int st_usuarioId, string role);

        Task<List<GruposCreadoCARes>> ObtenerGruposCreados(int ca_usuarioId, int st_usuarioId);
    }
}
