using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Models;

namespace ControlActividades.Interfaces.Alumnos
{
    public interface IAlumnoApiService
    {
        #region Entregas alumno
        Task<List<RegistrarEnvioActividadRes>> RegistrarEnvioActividadAlumnoConEnlaces(HttpRequest httpRequest, int actividadId, int alumnoId, int tipoEntrega, string fechaEntrega, string respuestaRaw, string enlacesJson);

        Task CancelarEnvioActividad(int alumnoId, int actividadId);

        Task<List<EnvioActividadAlumnoResponse>> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId);
        #endregion

        #region Dar de alta alumno a clases
        Task AlumnoGrupoCodigo(int alumnoId, string codigoAcceso);
        Task AlumnoMateriaCodigo(int alumnoId, string codigoAcceso);
        Task<UnirseAClaseMRespuesta> UnirseAClase(int alumnoId, string codigoAcceso);
        Task<RegistrarAlumnoGrupoMateriaDocenteRes> RegistrarAlumnoGrupoMateriaDocente(List<string> lsEmails, int grupoId, int materiaId);
        #endregion

        #region Dar de baja a alumno de clases
        Task EliminarAlumnoDeMateria(int materiaId, int alumnoId);
        Task EliminarAlumnoDeGrupo(int grupoId, int alumnoId);
        #endregion

        #region Obtener lista de alumnos por grupo o materia
        Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnosGrupo(int grupoId);

        Task<List<EmailVerificadoAlumno>> ObtenerListaAlumnosMateria(int grupoId, int materiaId);
        #endregion
    }
}