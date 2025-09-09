using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbGrupos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GrupoId { get; set; }

        [Required]
        public string NombreGrupo { get; set; }
        public string Descripcion { get; set; }
        public string CodigoAcceso { get; set; }
        public string CodigoColor { get; set; }

        [Required]
        public int DocenteId { get; set; }

        public virtual tbDocentes Docentes { get; set; }
        public virtual ICollection<tbGruposMaterias> GruposMaterias { get; set; }
        public virtual ICollection<tbAlumnosGrupos> AlumnosGrupos { get; set; }  

        public virtual ICollection<tbEventosGrupos> EventosGrupos { get; set; }
    }
}
