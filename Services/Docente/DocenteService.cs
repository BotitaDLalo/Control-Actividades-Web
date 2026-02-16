using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControlActividades.Interfaces.Docente;
using ControlActividades.Services.Alumno;

namespace ControlActividades.Services.Docente
{
    public class DocenteService : IDocentesService
    {

        #region Propiedades
        private DocenteCAService _docenteCAService;
        private DocenteSTService _docenteSTService;
        private FuenteDatosService _fuenteDatos;


        public DocenteService()
        {
        }

        public DocenteService(DocenteCAService docenteCAService, DocenteSTService docenteSTService, FuenteDatosService fuenteDatosService)
        {
            FuenteDatosService = fuenteDatosService;
            DocenteSTService = docenteSTService;
            DocenteCAService = docenteCAService;
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

        public DocenteSTService DocenteSTService
        {
            get
            {
                return _docenteSTService ?? (_docenteSTService = new DocenteSTService());
            }
            private set
            {
                _docenteSTService = value;
            }
        }

        public DocenteCAService DocenteCAService
        {
            get
            {
                return _docenteCAService ?? (_docenteCAService = new DocenteCAService());
            }
            private set
            {
                _docenteCAService = value;
            }
        }

        #endregion
    }
}