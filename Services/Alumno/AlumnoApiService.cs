using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Alumnos;
using ControlActividades.Services.Materias;

namespace ControlActividades.Services.Alumno
{
    public class AlumnoApiService : IAlumnoApiService
    {
        #region Propiedades
        private AlumnoApiCAService _materiasApiCAService;
        private AlumnoApiSTService _AlumnoApiSTService;
        private FuenteDatosService _fuenteDatos;


        public AlumnoApiService()
        {
        }

        public AlumnoApiService(AlumnoApiCAService materiasApiCAService, AlumnoApiSTService materiasApiSTService, FuenteDatosService fuenteDatosService)
        {
            FuenteDatosService = fuenteDatosService;
            AlumnoApiSTService = materiasApiSTService;
            AlumnoApiCAService = materiasApiCAService;
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

        public AlumnoApiSTService AlumnoApiSTService
        {
            get
            {
                return _AlumnoApiSTService ?? (_AlumnoApiSTService = new AlumnoApiSTService());
            }
            private set
            {
                _AlumnoApiSTService = value;
            }
        }

        public AlumnoApiCAService AlumnoApiCAService
        {
            get
            {
                return _materiasApiCAService ?? (_materiasApiCAService = new AlumnoApiCAService());
            }
            private set
            {
                _materiasApiCAService = value;
            }
        }

        #endregion
        public Task RegistrarEnvioActividadAlumnoConEnlaces(int actividadId, int alumnoId, int tipoEntrega, string fechaEntrega, string respuestaRaw, string enlacesJson)
        {
            throw new NotImplementedException();
        }
    }
}