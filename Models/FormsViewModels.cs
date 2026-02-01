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

    public class ActividadDTO
    {
        public string NombreActividad { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaLimite { get; set; }
        public int Puntaje { get; set; }
        public int MateriaId { get; set; }
    }
}