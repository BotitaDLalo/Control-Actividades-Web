using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbEventosGrupos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventoGrupoId { get; set; }

        public int? FechaId { get; set; }

        [Required]
        public int GrupoId { get; set; }

        public virtual tbEventosAgenda EventosAgenda { get; set; }
        public virtual tbGrupos Grupos { get; set; }
    }
}
