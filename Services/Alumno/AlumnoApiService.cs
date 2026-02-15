using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Alumnos;
using ControlActividades.Models;
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
        public async Task<List<RegistrarEnvioActividadRes>> RegistrarEnvioActividadAlumnoConEnlaces(HttpRequest httpRequest, int actividadId, int alumnoId, int tipoEntrega, string fechaEntrega, string respuestaRaw, string enlacesJson)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await AlumnoApiSTService.RegistrarEnvioActividadAlumnoConEnlaces(httpRequest, actividadId, alumnoId, tipoEntrega, fechaEntrega, respuestaRaw, enlacesJson);
            }
            return await AlumnoApiCAService.RegistrarEnvioActividadAlumnoConEnlaces(httpRequest, actividadId, alumnoId, tipoEntrega, fechaEntrega, respuestaRaw, enlacesJson);
        }



        public async Task<List<EnvioActividadAlumnoResponse>> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await AlumnoApiSTService.ObtenerEnviosActividadesAlumno(ActividadId, AlumnoId);
            }
            return await AlumnoApiCAService.ObtenerEnviosActividadesAlumno(ActividadId, AlumnoId);
        }

        public async Task CancelarEnvioActividad(int alumnoId, int actividadId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                await AlumnoApiSTService.CancelarEnvioActividad(alumnoId, actividadId);
            }
            await AlumnoApiCAService.CancelarEnvioActividad(alumnoId, actividadId);
        }

        public async Task AlumnoGrupoCodigo(int alumnoId, string codigoAcceso)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                await AlumnoApiSTService.AlumnoGrupoCodigo(alumnoId, codigoAcceso);
            }
            await AlumnoApiCAService.AlumnoGrupoCodigo(alumnoId, codigoAcceso);
        }

        public async Task AlumnoMateriaCodigo(int alumnoId, string codigoAcceso)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                await AlumnoApiSTService.AlumnoMateriaCodigo(alumnoId, codigoAcceso);
            }
            await AlumnoApiCAService.AlumnoMateriaCodigo(alumnoId, codigoAcceso);
        }

        public async Task<UnirseAClaseMRespuesta> UnirseAClase(int alumnoId, string codigoAcceso)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await AlumnoApiSTService.UnirseAClase(alumnoId, codigoAcceso);
            }
            return await AlumnoApiCAService.UnirseAClase(alumnoId, codigoAcceso);
        }

        public async Task<RegistrarAlumnoGrupoMateriaDocenteRes> RegistrarAlumnoGrupoMateriaDocente(List<string> lsEmails, int grupoId, int materiaId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                return await AlumnoApiSTService.RegistrarAlumnoGrupoMateriaDocente(lsEmails, grupoId, materiaId);
            }
            return await AlumnoApiCAService.RegistrarAlumnoGrupoMateriaDocente(lsEmails, grupoId, materiaId);
        }

        public async Task EliminarAlumnoDeMateria(int materiaId, int alumnoId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();

            if (fuenteDatos == FuenteDatos.API)
            {
                await AlumnoApiSTService.EliminarAlumnoDeMateria(materiaId, alumnoId);
            }
            await AlumnoApiCAService.EliminarAlumnoDeMateria(materiaId, alumnoId);
        }

        public async Task EliminarAlumnoDeGrupo(int grupoId, int alumnoId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                await AlumnoApiSTService.EliminarAlumnoDeGrupo(grupoId, alumnoId);
            }
            await AlumnoApiCAService.EliminarAlumnoDeGrupo(grupoId, alumnoId);
        }

        public async Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnosGrupo(int grupoId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await AlumnoApiSTService.ObtenerListaAlumnosGrupo(grupoId);
            }
            return await AlumnoApiCAService.ObtenerListaAlumnosGrupo(grupoId);
        }

        public async Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnosMateria(int grupoId, int materiaId)
        {
            var fuenteDatos = FuenteDatosService.ObtenerFuenteDatos();
            if (fuenteDatos == FuenteDatos.API)
            {
                return await AlumnoApiSTService.ObtenerListaAlumnosMateria(grupoId, materiaId);
            }
            return await AlumnoApiCAService.ObtenerListaAlumnosMateria(grupoId, materiaId);
        }
    }
}