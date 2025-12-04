using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;


namespace ControlActividades.Models.db
{
    public class tbAlumnos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public  int AlumnoId { get; set; }

        [Required]
        public string ApellidoPaterno { get; set; }

        [Required]
        public string ApellidoMaterno { get; set; }
        
        [Required]
        public string Nombre { get; set; }

        public virtual ApplicationUser IdentityUser { get; set; }
        
        [ForeignKey("IdentityUser")]
        [Required]
        public string UserId { get; set; }

        public virtual ICollection<tbAlumnosGrupos> AlumnosGrupos { get; set; }
        
        public virtual ICollection<tbAlumnosMaterias> AlumnosMaterias { get; set; }
        
        public virtual ICollection<tbAlumnosActividades> AlumnosActividades { get; set; }
        
        // Estatus general del alumno (por ejemplo: Activo, Inactivo, Dado de Baja)
        public string Estatus { get; set; }
    }
}
