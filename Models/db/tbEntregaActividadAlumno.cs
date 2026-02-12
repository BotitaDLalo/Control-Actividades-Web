using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    // Mapear al nombre efectivo de la tabla en la base de datos
    // Nombre real en la base (sin pluralización extra)
    [Table("tbEntregaActividadAlumno")]
    public class tbEntregaActividadAlumno
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntregaActividadAlumnoId { get; set; }

        public int ActividadId { get; set; }

        public int AlumnoId { get; set; }

        public DateTime FechaEntrega { get; set; }

        public bool Estatus { get; set; }

        public int EstadoEntregaId { get; set; }
        public DateTime? FechaCalificado { get; set; }

        public Decimal Calificacion { get; set; }

        public bool EntregaTardia { get; set; } 
    
        public tbActividades tbActividades { get; set; }    
    
        public ICollection<tbEntregables> tbEntregables { get; set; }

        public tbAlumnos tbAlumnos { get; set; }    

        public cEstadoEntrega cEstadoEntrega { get; set; }
    }
}
