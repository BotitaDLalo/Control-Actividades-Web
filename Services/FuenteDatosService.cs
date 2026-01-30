using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControlActividades.Interfaces;

namespace ControlActividades.Services
{
    public class FuenteDatosService : IFuenteDatos
    {
        public string ObtenerFuenteDatos()
        {
            return FuenteDatos.DB;
        }
    }
}