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
        bool TieneGrupos(string role, int usuarioId);

        bool TieneMaterias(string role, int usuarioId);

        List<GrupoViewModel> ObtenerGruposPorUsuario(string role, int usuarioId);

        List<MateriaViewModel> ObtenerMateriasPorGrupo(int grupoId, int usuarioId, string role);
    }
}
