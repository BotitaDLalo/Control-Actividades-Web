using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbAvisos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int  AvisoId { get; set; }

        [Required]
        public int DocenteId { get; set; }

        [Required]  
        public string Titulo { get; set; }

        [Required]  
        public string Descripcion {  get; set; }

        public int? GrupoId { get; set; }

        public int? MateriaId { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual tbDocentes Docentes { get; set; }
    }
}
