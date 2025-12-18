using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbActividades
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActividadId { get; set; }
        
        [Required]
        public string NombreActividad { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime FechaLimite { get; set; }

        //public int TipoActividadId { get; set; }

        public int Puntaje {  get; set; }

        public int MateriaId { get; set; }

        // Indica si la actividad fue enviada/publicada a los alumnos.
        // true = publicada/enviada, false = borrador, null = programada / publicar después
        public bool? Enviado { get; set; }

        // Opcional: fecha programada para publicación (puede usarse si Enviado == null)
        public DateTime? FechaProgramada { get; set; }


        //public virtual ICollection<tbAlumnosActividades> AlumnosActividades { get; set; }
        //public virtual cTiposActividades TiposActividades { get; set; }
        
        public virtual ICollection<tbEntregaActividadAlumno> tbEntregaActividadAlumno { get; set; }
        
        public virtual tbMaterias Materias { get; set; }
    }
}
