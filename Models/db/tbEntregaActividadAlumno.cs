using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    [Table("tbEntregableActividadAlumno")]
    public class tbEntregaActividadAlumno
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntregaActividadAlumnoId { get; set; }

        public int ActividadId { get; set; }

        public int AlumnoId { get; set; }

        public DateTime FechaEntrega { get; set; }

        //public bool EstatusEntregada { get; set; }

        public int EstadoEntregaId { get; set; }
        public DateTime? FechaCalificado { get; set; }
    
        public tbActividades tbActividades { get; set; }    
    
        public ICollection<tbEntregables> tbEntregables { get; set; }

        public tbAlumnos tbAlumnos { get; set; }    

        public cEstadoEntrega cEstadoEntrega { get; set; }
    }
}
