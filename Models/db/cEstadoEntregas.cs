using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlActividades.Models.db
{
    [Table("cEstadoEntregas")]
    public class cEstadoEntregas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int EstadoEntregaId { get; set; }

        [Required]
        public string Nombre { get; set; }

        public virtual ICollection<tbEntregaActividadAlumno> tbEntregaActividadAlumno { get; set; }
    }
}
