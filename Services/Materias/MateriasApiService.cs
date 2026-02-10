using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Materias;
using ControlActividades.Models;

namespace ControlActividades.Services.Materias
{
    public class MateriasApiService : IMateriasApiService
    {
        private MateriasApiCAService _materiasApiCAService;
        private MateriasApiSTService _materiasSTService;
        private FuenteDatosService _fuenteDatos;


        public MateriasApiService()
        {
        }

        public MateriasApiService(MateriasApiCAService materiasApiCAService, MateriasApiSTService materiasApiSTService, FuenteDatosService fuenteDatosService)
        {
            FuenteDatosService = fuenteDatosService;
            MateriasApiSTService = materiasApiSTService;
            MateriasApiCAService = materiasApiCAService;
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

        public MateriasApiSTService MateriasApiSTService
        {
            get
            {
                return _materiasSTService ?? (_materiasSTService = new MateriasApiSTService());
            }
            private set
            {
                _materiasSTService = value;
            }
        }

        public MateriasApiCAService MateriasApiCAService
        {
            get
            {
                return _materiasApiCAService ?? (_materiasApiCAService = new MateriasApiCAService());
            }
            private set
            {
                _materiasApiCAService = value;
            }
        }

        #endregion

        public async Task<List<MateriaCARes>> ObtenerMaterias(int ca_usuarioId, int st_usuarioId, string role)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await MateriasApiSTService.ObtenerMaterias(ca_usuarioId, st_usuarioId, role);
            }
            return await MateriasApiCAService.ObtenerMaterias(ca_usuarioId, st_usuarioId, role);
        }

    }
}