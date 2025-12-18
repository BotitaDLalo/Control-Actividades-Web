using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;


namespace ControlActividades.Models.db
{
    public class tbAlumnosMaterias
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlumnoMateriaId { get; set; }

        [Required]
        public int AlumnoId { get; set; }
        public virtual tbAlumnos Alumnos { get; set; }

        [Required]
        public virtual int MateriaId { get; set; }
        public virtual tbMaterias Materias { get; set; }
    }
}
