using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlActividades.Models;

namespace ControlActividades.Interfaces.Alumnos
{
    public interface IAlumnoService
    {
        Task<UnirseAClaseMRespuesta> UnirseAClase(int alumnoId, string codigoAcceso);
    }
}
