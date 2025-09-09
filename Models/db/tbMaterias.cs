using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbMaterias
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MateriaId { get; set; }
        
        [Required]
        public string NombreMateria { get; set; }
        
        public string Descripcion { get; set; }

        public string CodigoColor { get; set; }

        public string CodigoAcceso { get; set; }

        [Required]
        public  int DocenteId { get; set; }
        public virtual tbDocentes Docentes { get; set; }

        public virtual ICollection<tbGruposMaterias> GruposMaterias { get; set; }

        public virtual ICollection<tbAlumnosMaterias> AlumnosMaterias { get; set; }

        public virtual ICollection<tbActividades> Actividades { get; set; }

        public  virtual ICollection<tbEventosMaterias> EventosMaterias { get; set; }
    }
}
