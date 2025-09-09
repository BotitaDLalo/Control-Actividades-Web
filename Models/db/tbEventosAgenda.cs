using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbEventosAgenda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventoId { get; set; }

        [Required]
        public int DocenteId { get; set; }

        public virtual tbDocentes Docentes { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public  DateTime FechaFinal { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]  
        public string Descripcion { get; set; }

        [Required]  
        public string Color { get; set; }

        public virtual ICollection<tbEventosGrupos> EventosGrupos { get; set; }

        public virtual ICollection<tbEventosMaterias> EventosMaterias { get; set; }
    }
}
