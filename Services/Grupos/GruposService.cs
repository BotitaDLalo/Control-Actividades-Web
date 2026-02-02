using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<List<GruposCARes>> ObtenerGruposPorUsuario(string role, int ca_usuarioId, int st_usuarioId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await GruposSTService.ObtenerGruposPorUsuario(role, ca_usuarioId, st_usuarioId);
            }
            return await GruposCAService.ObtenerGruposPorUsuario(role, ca_usuarioId, st_usuarioId);
        }

        public async Task<List<MateriaCARes>> ObtenerMateriasPorGrupo(int grupoId, int ca_usuarioId, int st_usuarioId, string role)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await GruposSTService.ObtenerMateriasPorGrupo(grupoId, ca_usuarioId, st_usuarioId, role);
            }
            return await GruposCAService.ObtenerMateriasPorGrupo(grupoId, ca_usuarioId, st_usuarioId, role);
        }

        public async Task<bool> TieneGrupos(string role, int ca_usuarioId, int st_usuarioId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await GruposSTService.TieneGrupos(role, ca_usuarioId, st_usuarioId);
            }
            return await GruposCAService.TieneGrupos(role, ca_usuarioId, st_usuarioId);
        }

        public async Task<bool> TieneMaterias(string role, int ca_usuarioId, int st_usuarioId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await GruposSTService.TieneMaterias(role, ca_usuarioId, st_usuarioId);
            }
            return await GruposCAService.TieneMaterias(role, ca_usuarioId, st_usuarioId);
        }
    }
}