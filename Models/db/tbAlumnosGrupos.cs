using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ControlActividades.Models.db
{
    public class tbAlumnosGrupos
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlumnoGrupoId { get; set; }

        [Required]
        public int AlumnoId { get; set; }

        public virtual tbAlumnos Alumnos { get; set; }

        [Required]
        public int GrupoId { get; set; }

        public virtual tbGrupos Grupos { get; set; }

    }
}
