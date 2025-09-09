using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace ControlActividades.Models.db
{
    public class tbAlumnosActividades
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlumnoActividadId { get; set; }

        [Required]
        public int ActividadId { get; set; }

        [Required]
        public int AlumnoId { get; set; }

        [Required]  
        public DateTime FechaEntrega { get; set; }

        [Required]  
        public bool EstatusEntrega { get; set; } 

        public virtual tbActividades Actividades {  get; set; } 
        public virtual tbAlumnos Alumnos { get; set; }
        public virtual tbEntregableAlumno EntregablesAlumno { get; set; }

    }
}
