using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlActividades.Interfaces.Alumnos
{
    public interface IAlumnoApiService
    {
        Task RegistrarEnvioActividadAlumnoConEnlaces(int actividadId, int alumnoId, int tipoEntrega, string fechaEntrega, string respuestaRaw, string enlacesJson);
    }
}
