using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Exceptions
{
    public class EntregaAlumnoException : Exception
    {
        public EntregaAlumnoException()
        {
        }

        public EntregaAlumnoException(string message) : base(message)
        {
        }
    }
}