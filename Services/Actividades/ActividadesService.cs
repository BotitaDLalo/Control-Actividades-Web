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

        private ActividadesCAService _activididadesCAService;
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
            ActivididadesCAService = actividadesCAService;

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

        public ActividadesCAService ActivididadesCAService
        {
            get
            {
                return _activididadesCAService ?? (_activididadesCAService = new ActividadesCAService());
            }
            private set
            {
                _activididadesCAService = value;
            }
        }
        #endregion

        public async Task<List<ActividadRes>> ObtenerActividadesPorMateria(int materiaId, bool esDocente)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await ActividadesSTService.ObtenerActividadesPorMateria(materiaId, esDocente);
            }
            return await ActivididadesCAService.ObtenerActividadesPorMateria(materiaId, esDocente);
        }

        public async Task EliminarActividadAsync(int id)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                await ActividadesSTService.EliminarActividadAsync(id);
            }
            await ActivididadesCAService.EliminarActividadAsync(id);
        }       
    }

}