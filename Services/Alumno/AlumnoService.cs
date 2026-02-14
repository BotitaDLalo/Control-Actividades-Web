using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Services.Alumno
{
    public class AlumnoService
    {
        #region Propiedades
        private AlumnoCAService _alumnoCAService;
        private AlumnoSTService _alumnoSTService;
        private FuenteDatosService _fuenteDatos;


        public AlumnoService()
        {
        }

        public AlumnoService(AlumnoCAService alumnoCAService, AlumnoSTService alumnoSTService, FuenteDatosService fuenteDatosService)
        {
            FuenteDatosService = fuenteDatosService;
            AlumnoSTService = alumnoSTService;
            AlumnoCAService = alumnoCAService;
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

        public AlumnoSTService AlumnoSTService
        {
            get
            {
                return _alumnoSTService ?? (_alumnoSTService = new AlumnoSTService());
            }
            private set
            {
                _alumnoSTService = value;
            }
        }

        public AlumnoCAService AlumnoCAService
        {
            get
            {
                return _alumnoCAService ?? (_alumnoCAService = new AlumnoCAService());
            }
            private set
            {
                _alumnoCAService = value;
            }
        }

        #endregion
    }
}