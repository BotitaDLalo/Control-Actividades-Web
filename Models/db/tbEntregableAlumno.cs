using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbEntregableAlumno
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EntregaId {  get; set; }

        public int AlumnoActividadId { get; set; }
        public string Respuesta {  get; set; }

        public virtual tbAlumnosActividades AlumnosActividades { get; set; }
        public virtual ICollection<tbCalificaciones> Calificaciones { get; set; }

    }
}
