using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Actividades;
using ControlActividades.Services.Actividades;
using ControlActividades.Models;

namespace ControlActividades.Services
{
    public class ActividadesService : IActividadesService
    {
        private ActividadesCAService _actividadesCAService;
        private ActividadesSTService _actividadesSTService;
        private FuenteDatosService _fuenteDatos;

        public ActividadesService()
        {
        }
        #region dependencias
        public ActividadesService(ActividadesCAService actividadesCAService, ActividadesSTService actividadesSTService, FuenteDatosService fuenteDatosService)
        {
            FuenteDatosService = fuenteDatosService;
            ActividadesSTService = actividadesSTService;
            ActividadesCAService = actividadesCAService;

        }

        public FuenteDatosService FuenteDatosService
        {
            get { 
                return _fuenteDatos ?? (_fuenteDatos = new FuenteDatosService());
            }
            private set 
            { 
                _fuenteDatos = value; 
            }
        }

        public ActividadesSTService ActividadesSTService
        {
            get
            {
                return _actividadesSTService ?? (_actividadesSTService = new ActividadesSTService());
            }
            private set
            {
                _actividadesSTService = value;
            }
        }

        public ActividadesCAService ActividadesCAService
        {
            get
            {
                return _actividadesCAService ?? (_actividadesCAService = new ActividadesCAService());
            }
            private set
            {
                _actividadesCAService = value;
            }
        }
        #endregion

        public async Task<List<ActividadRes>> ObtenerActividadesPorMateria(int materiaId, string rol)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await ActividadesSTService.ObtenerActividadesPorMateria(materiaId, rol);
            }
            return await ActividadesCAService.ObtenerActividadesPorMateria(materiaId, rol);
        }

        public async Task<ActividadDetallesRes> ObtenerActividadPorId(int actividadId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if(fuenteDatos == FuenteDatos.API)
            {
                return await ActividadesSTService.ObtenerActividadPorId(actividadId);
            }
            return await ActividadesCAService.ObtenerActividadPorId(actividadId);
        }

        public async Task<ActividadRes> ActualizarActividad(int id, ActividadDTO actividad)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await ActividadesSTService.ActualizarActividad(id, actividad);
            }
            return await ActividadesCAService.ActualizarActividad(id, actividad);
        }

        public async Task EliminarActividad(int id)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                // ST implementation exposes EliminarActividadAsync
                await ActividadesSTService.EliminarActividadAsync(id);
                return;
            }

            // CA implementation exposes EliminarActividad
            await ActividadesCAService.EliminarActividad(id);
        }       
    }

}