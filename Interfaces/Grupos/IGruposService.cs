using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ControlActividades.Models;

namespace ControlActividades.Interfaces
{
    public interface IGruposService
    {
        Task<bool> TieneGrupos(string role, int ca_usuarioId, int st_usuarioId);

        Task<bool> TieneMaterias(string role, int ca_usuarioId, int st_usuarioId);

        Task<List<GruposCARes>> ObtenerGruposPorUsuario(string role, int ca_usuarioId, int st_usuarioId);

        Task<List<MateriaCARes>> ObtenerMateriasPorGrupo(int grupoId, int ca_usuarioId, int st_usuarioId, string role);
    }
}
