using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbGruposMaterias
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GrupoMateriasId { get; set; }

        public int GrupoId { get; set; }

        public virtual tbGrupos Grupos { get; set; }

        public int MateriaId { get; set; }

        public virtual tbMaterias Materias { get; set; }
    }
}
