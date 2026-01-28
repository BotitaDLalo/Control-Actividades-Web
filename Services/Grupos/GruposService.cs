using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControlActividades.Interfaces;
using ControlActividades.Models;

namespace ControlActividades.Services
{
    public class GruposService : IGruposService
    {
        private FuenteDatosService _fuenteDatos;
        private GruposCAService _gruposCAService;
        private GruposSTService _gruposSTService;
        
        public GruposService()
        {
        }

        public GruposService(FuenteDatosService fuenteDatos, GruposCAService gruposCAService, GruposSTService gruposSTService)
        {
            FuenteDatosService = fuenteDatos;
            GruposCAService = gruposCAService;
            GruposSTService = gruposSTService;
        }

        #region Propiedades
        public FuenteDatosService FuenteDatosService
        {
            get
            {
                return _fuenteDatos ?? (_fuenteDatos = new FuenteDatosService());
            }
            private set
            {
                _fuenteDatos = value;
            }
        }

        public GruposCAService GruposCAService
        {
            get
            {
                return _gruposCAService ?? (_gruposCAService = new GruposCAService());
            }
            private set
            {
                _gruposCAService = value;
            }
        }

        public GruposSTService GruposSTService
        {

            get
            {
                return _gruposSTService ?? (_gruposSTService = new GruposSTService());
            }
            private set
            {
                _gruposSTService = value;
            }
        }

        #endregion

        public List<GrupoViewModel> ObtenerGruposPorUsuario(string rol, int usuarioId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return GruposSTService.ObtenerGruposPorUsuario(rol, usuarioId);
            }
            return GruposCAService.ObtenerGruposPorUsuario(rol,usuarioId);
        }

        public List<MateriaViewModel> ObtenerMateriasPorGrupo(int grupoId, int usuarioId, string role)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return GruposSTService.ObtenerMateriasPorGrupo(grupoId, usuarioId, role);
            }
            return GruposCAService.ObtenerMateriasPorGrupo(grupoId, usuarioId, role);
        }

        public bool TieneGrupos(string role, int usuarioId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return GruposSTService.TieneGrupos(role,usuarioId);
            }
            return GruposCAService.TieneGrupos(role, usuarioId);
        }

        public bool TieneMaterias(string role, int usuarioId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return GruposSTService.TieneMaterias(role,usuarioId);
            }
            return GruposCAService.TieneMaterias(role, usuarioId);
        }
    }
}