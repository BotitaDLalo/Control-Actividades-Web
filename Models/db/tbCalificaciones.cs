using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbCalificaciones
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CalificacionId { get; set; }

        [Required]  
        public int EntregaId { get; set; }

        [Required]  
        public DateTime FechaCalificacionAsignada {  get; set; }

        public string Comentarios {  get; set; }

        [Required]
        public int Calificacion { get; set; }
        public virtual tbEntregableAlumno EntregablesAlumno { get; set; }
    }
}
