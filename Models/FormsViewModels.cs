using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Models
{
    public class CrearMateria
    {
        public string Nombre {  get; set; }

        public string Descripcion { get; set; }
    }

    public class CrearGrupoDTO
    {
        public string NombreGrupo { get; set; }
        public string Descripcion { get; set; }
        public string CodigoColor { get; set; }
    }

}