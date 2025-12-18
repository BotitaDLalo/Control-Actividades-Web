
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbEntregables
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntregableId { get; set; }

        public int EntregaActividadAlumnoId { get; set; }

        public int TipoEntregaId { get; set; }

        public string Contenido { get; set; }

        public int? Calificacion {  get; set; }
        public DateTime? FechaCalificado { get; set; }

        public tbEntregaActividadAlumno tbEntregaActividadAlumno { get; set; }

        public cTipoEntrega cTipoEntrega { get; set; }
    }
}
