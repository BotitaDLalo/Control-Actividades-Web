using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ControlActividades.Models.db
{
    public class cEstadoEntrega
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EstadoEntregaId { get; set; }
        [Required]
        public string Nombre { get; set; }

        public ICollection<tbEntregaActividadAlumno> tbEntregaActividadAlumno { get; set; }
    }
}