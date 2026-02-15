using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Exceptions
{
    public class ActividadException : Exception
    {
        public ActividadException(string mensaje) : base(mensaje)
        {
        }
    }
}