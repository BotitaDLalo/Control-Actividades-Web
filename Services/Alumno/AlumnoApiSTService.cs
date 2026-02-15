using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Alumnos;

namespace ControlActividades.Services.Alumno
{
    public class AlumnoApiSTService : IAlumnoApiService
    {
        public Task RegistrarEnvioActividadAlumnoConEnlaces(int actividadId, int alumnoId, int tipoEntrega, string fechaEntrega, string respuestaRaw, string enlacesJson)
        {
            throw new NotImplementedException();
        }
    }
}