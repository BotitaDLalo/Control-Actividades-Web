using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Models
{
    #region Entregables
    public class EntregablesPartialViewModel
    {
        public List<ActividadesMateria> ActividadesMateria { get; set; }

        public List<AlumnosCalificar> AlumnosCalificar {  get; set; }
    }

    public class ActividadesMateria
    {
        public int ActividadId { get; set; }
     
        public string NombreActividad { get; set; }
        
        public decimal Puntaje { get; set; }
    }

    public class AlumnosCalificar
    {
        public int EntregableId { get; set; }
        public int AlumnoId { get; set; }
        public string NombreCompletoAlumno { get; set; }

        public decimal Calificacion { get; set; }

        public int ActividadId { get; set; }

        public bool EntregaTardia { get; set; }

        public bool SinPuntaje { get; set; }
    }
    #endregion
}