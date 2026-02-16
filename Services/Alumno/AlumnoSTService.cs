using ControlActividades.Interfaces.Alumnos;
using ControlActividades.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ControlActividades.Services.Alumno
{
    public class AlumnoSTService : IAlumnoService
    {
        public AlumnoSTService()
        {
        }

        public Task<UnirseAClaseMRespuesta> UnirseAClase(int alumnoId, string codigoAcceso)
        {
            throw new NotImplementedException();
        }
    }
}