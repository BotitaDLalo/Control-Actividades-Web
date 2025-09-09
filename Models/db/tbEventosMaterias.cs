using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbEventosMaterias
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventoMateriaId { get; set; }

        public int FechaId { get; set; }

        [Required]
        public int MateriaId {  get; set; }
        public virtual tbEventosAgenda EventosAgenda { get; set; }
        public virtual tbMaterias Materias { get; set; }
    }
}
