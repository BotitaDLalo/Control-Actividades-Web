using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using ControlActividades.Interfaces;
using ControlActividades.Models;

namespace ControlActividades.Services
{
    public class GruposSTService : IGruposService
    {
        public List<GrupoViewModel> ObtenerGruposPorUsuario(string rol, int usuarioId)
        {
            throw new NotImplementedException();
        }

        public List<MateriaViewModel> ObtenerMateriasPorGrupo(int grupoId, int usuarioId, string role)
        {
            throw new NotImplementedException();
        }

        public bool TieneGrupos(string role, int usuarioId)
        {
            throw new NotImplementedException();
        }

        public bool TieneMaterias(string role, int usuarioId)
        {
            throw new NotImplementedException();
        }
    }
}