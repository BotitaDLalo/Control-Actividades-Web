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

        public Decimal Puntaje {  get; set; }

        public int MateriaId { get; set; }

        public bool PermitirEntregasTarde { get; set; }

        public bool? Enviado { get; set; }

        public DateTime? FechaProgramada { get; set; }

        public int LimiteEntregasPorAlumno { get; set; }

        public virtual ICollection<tbEntregaActividadAlumno> tbEntregaActividadAlumno { get; set; }
        
        public virtual tbMaterias Materias { get; set; }
    }
}
