using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Actividades;
using ControlActividades.Models;
using ControlActividades.Models.db;

namespace ControlActividades.Services.Actividades
{
    public class ActividadesApiService : IActividadesApiService
    {
        private ActividadesApiCAService _actividadesCAService;
        private ActividadesApiSTService _actividadesSTService;
        private FuenteDatosService _fuenteDatos;

        public ActividadesApiService()
        {
        }
        #region dependencias
        public ActividadesApiService(ActividadesApiCAService actividadesApiCAService, ActividadesApiSTService actividadesApiSTService, FuenteDatosService fuenteDatosService)
        {
            FuenteDatosService = fuenteDatosService;
            ActividadesApiSTService = actividadesApiSTService;
            ActividadesApiCAService = actividadesApiCAService;

        }

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

        public ActividadesApiSTService ActividadesApiSTService
        {
            get
            {
                return _actividadesSTService ?? (_actividadesSTService = new ActividadesApiSTService());
            }
            private set
            {
                _actividadesSTService = value;
            }
        }

        public ActividadesApiCAService ActividadesApiCAService
        {
            get
            {
                return _actividadesCAService ?? (_actividadesCAService = new ActividadesApiCAService());
            }
            private set
            {
                _actividadesCAService = value;
            }
        }

        #endregion
        public async Task<ObtenerEnviosActividadesAlumnoRes> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await ActividadesApiSTService.ObtenerEnviosActividadesAlumno(ActividadId, AlumnoId);
            }
            return await ActividadesApiCAService.ObtenerEnviosActividadesAlumno(ActividadId, AlumnoId);
        }



        public async Task<List<ObtenerActividadesPorMateriaRes>> ObtenerActividadesPorMateria(int materiaId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await ActividadesApiSTService.ObtenerActividadesPorMateria(materiaId);
            }
            return await ActividadesApiCAService.ObtenerActividadesPorMateria(materiaId);
        }

        public async Task<RespuestaAlumnosEntregables> ObtenerAlumnosEntregables(int actividadId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await ActividadesApiSTService.ObtenerAlumnosEntregables(actividadId);
            }
            return await ActividadesApiCAService.ObtenerAlumnosEntregables(actividadId);

        }

        public async Task AsignarCalificacion(int entregableId, decimal calificacion)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                await ActividadesApiSTService.AsignarCalificacion(entregableId, calificacion);
            }
            await ActividadesApiCAService.AsignarCalificacion(entregableId, calificacion);
        }

        public async Task QuitarCalificacion(int entregableId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                await ActividadesApiSTService.QuitarCalificacion(entregableId);
            }
            await ActividadesApiCAService.QuitarCalificacion(entregableId);
        }

        public async Task CrearActividad(tbActividades nuevaActividad)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                await ActividadesApiSTService.CrearActividad(nuevaActividad);
            }
            await ActividadesApiCAService.CrearActividad(nuevaActividad);
        }

        public async Task<ActividadesDTO> ActualizarActividad(int id, tbActividades updatedActivity)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await ActividadesApiSTService.ActualizarActividad(id, updatedActivity);
            }
            return await ActividadesApiCAService.ActualizarActividad(id, updatedActivity);
        }

        public async Task EliminarActividad(int id)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                await ActividadesApiSTService.EliminarActividad(id);
            }
            await ActividadesApiCAService.EliminarActividad(id);
        }
    }
}