using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces;
using ControlActividades.Interfaces.Materias;
using ControlActividades.Models;
using ControlActividades.Services.Materias;

namespace ControlActividades.Services
{
    public class MateriasService : IMateriasService
    {
        private MateriasCAService _materiasCAService;
        private MateriasSTService _materiasSTService;
        private FuenteDatosService _fuenteDatos;


        public MateriasService()
        {
        }

        public MateriasService(MateriasCAService materiasCAService, MateriasSTService materiasSTService, FuenteDatosService fuenteDatosService)
        {
            FuenteDatosService = fuenteDatosService;
            MateriasSTService = materiasSTService;
            MateriasCAService = materiasCAService;
        }

        public List<MateriaViewModel> ObtenerMateriasSinGrupoPorUsuario(int usuarioId, string role)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return MateriasSTService.ObtenerMateriasSinGrupoPorUsuario(usuarioId, role);
            }
            return MateriasCAService.ObtenerMateriasSinGrupoPorUsuario(usuarioId, role);
        }


        public async Task<MateriaViewModel> ObtenerMateriaDetalles(int materiaId, int docenteId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await MateriasSTService.ObtenerMateriaDetalles(materiaId, docenteId);
            }
            return await MateriasCAService.ObtenerMateriaDetalles(materiaId, docenteId);
        }

        public async Task<ActividadRes> CrearActividadAsync(ActividadDTO actividad)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await MateriasSTService.CrearActividadAsync(actividad);
            }
            return await MateriasCAService.CrearActividadAsync(actividad);
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

        public MateriasSTService MateriasSTService
        {
            get
            {
                return _materiasSTService ?? (_materiasSTService = new MateriasSTService());
            }
            private set
            {
                _materiasSTService = value;
            }
        }

        public MateriasCAService MateriasCAService
        {
            get
            {
                return _materiasCAService ?? (_materiasCAService = new MateriasCAService());
            }
            private set
            {
                _materiasCAService = value;
            }
        }

        #endregion

    }
}