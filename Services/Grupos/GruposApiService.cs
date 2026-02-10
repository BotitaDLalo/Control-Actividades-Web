using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Grupos;
using ControlActividades.Models;

namespace ControlActividades.Services.Grupos
{
    public class GruposApiService : IGruposApiService
    {
        private FuenteDatosService _fuenteDatos;
        private GruposApiCAService _gruposCAService;
        private GruposApiSTService _gruposSTService;

        public GruposApiService()
        {
        }

        public GruposApiService(FuenteDatosService fuenteDatos, GruposApiCAService gruposApiCAService, GruposApiSTService gruposApiSTService)
        {
            FuenteDatosService = fuenteDatos;
            GruposApiCAService = gruposApiCAService;
            GruposApiSTService = gruposApiSTService;
        }


        public async Task<List<GruposCARes>> ObtenerGruposMaterias(int ca_usuarioId, int st_usuarioId, string role)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await GruposApiSTService.ObtenerGruposMaterias(ca_usuarioId, st_usuarioId, role);
            }
            return await GruposApiCAService.ObtenerGruposMaterias(ca_usuarioId, st_usuarioId, role);
        }



        public async Task<List<GruposCreadoCARes>> ObtenerGruposCreados(int ca_usuarioId, int st_usuarioId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await GruposApiSTService.ObtenerGruposCreados(ca_usuarioId, st_usuarioId);
            }
            return await GruposApiCAService.ObtenerGruposCreados(ca_usuarioId, st_usuarioId);
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

        public GruposApiCAService GruposApiCAService
        {
            get
            {
                return _gruposCAService ?? (_gruposCAService = new GruposApiCAService());
            }
            private set
            {
                _gruposCAService = value;
            }
        }

        public GruposApiSTService GruposApiSTService
        {

            get
            {
                return _gruposSTService ?? (_gruposSTService = new GruposApiSTService());
            }
            private set
            {
                _gruposSTService = value;
            }
        }

        #endregion

    }
}