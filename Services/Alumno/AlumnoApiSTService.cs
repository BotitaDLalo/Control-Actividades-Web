using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Alumnos;
using ControlActividades.Models;

namespace ControlActividades.Services.Alumno
{
    public class AlumnoApiSTService : IAlumnoApiService
    {
        public Task AlumnoGrupoCodigo(int alumnoId, string codigoAcceso)
        {
            throw new NotImplementedException();
        }

        public Task AlumnoMateriaCodigo(int alumnoId, string codigoAcceso)
        {
            throw new NotImplementedException();
        }

        public Task CancelarEnvioActividad(int alumnoId, int actividadId)
        {
            throw new NotImplementedException();
        }

        public Task EliminarAlumnoDeGrupo(int grupoId, int alumnoId)
        {
            throw new NotImplementedException();
        }

        public Task EliminarAlumnoDeMateria(int materiaId, int alumnoId)
        {
            throw new NotImplementedException();
        }

        public Task<List<EnvioActividadAlumnoResponse>> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            throw new NotImplementedException();
        }

        public Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnosGrupo(int grupoId)
        {
            throw new NotImplementedException();
        }

        public Task<RegistrarAlumnoGrupoMateriaDocenteRes> RegistrarAlumnoGrupoMateriaDocente(List<string> lsEmails, int grupoId, int materiaId)
        {
            throw new NotImplementedException();
        }

        public Task<List<RegistrarEnvioActividadRes>> RegistrarEnvioActividadAlumnoConEnlaces(HttpRequest httpRequest, int actividadId, int alumnoId, int tipoEntrega, string fechaEntrega, string respuestaRaw, string enlacesJson)
        {
            throw new NotImplementedException();
        }

        public Task<UnirseAClaseMRespuesta> UnirseAClase(int alumnoId, string codigoAcceso)
        {
            throw new NotImplementedException();
        }

        public Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnosMateria(int grupoId,int materiaId)
        {
            throw new NotImplementedException();
        }
    }
}