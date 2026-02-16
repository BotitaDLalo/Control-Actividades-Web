using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Exceptions
{
    using System;

    public class AlumnosException : Exception
    {
        public string Mensaje { get; }
        public string Detalles { get; }

        public AlumnosException() { }

        public AlumnosException(
            string mensaje,
            string detalles,
            Exception innerException = null)
            : base(detalles, innerException)
        {
            Mensaje = mensaje;
            Detalles = detalles;
        }
    }

}