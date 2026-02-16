using System;
using System.Collections.Generic;
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

        public async Task<List<MateriaCARes>> ObtenerMateriasSinGrupoPorUsuario(int usuarioId, int st_usuarioId, string role)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await MateriasSTService.ObtenerMateriasSinGrupoPorUsuario(usuarioId, st_usuarioId, role);
            }
            return await MateriasCAService.ObtenerMateriasSinGrupoPorUsuario(usuarioId, st_usuarioId, role);
        }


        public async Task<MateriaCARes> ObtenerMateriaDetalles(int materiaId, int grupoId, string role, int ca_usuarioId, int st_usuarioId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await MateriasSTService.ObtenerMateriaDetalles(materiaId, grupoId, role, ca_usuarioId, st_usuarioId);
            }
            return await MateriasCAService.ObtenerMateriaDetalles(materiaId, grupoId, role, ca_usuarioId, st_usuarioId);
        }

        public async Task<List<AlumnoCorreo>> BuscarAlumnosPorCorreo(string query)
        {
            //var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            //if (fuenteDatos == FuenteDatos.API)
            //{
            //    return await MateriasSTService.BuscarAlumnosPorCorreo(query);
            //}
            return await MateriasCAService.BuscarAlumnosPorCorreo(query);
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

        public async Task<EntregablesPartialViewModel> ObtenerEntregablesAlumno(int materiaId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await MateriasSTService.ObtenerEntregablesAlumno(materiaId);
            }
            return await MateriasCAService.ObtenerEntregablesAlumno(materiaId);
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