using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;


namespace ControlActividades.Models.db
{
    public class cTiposActividades
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TipoActividadId { get; set; }
        [Required]
        public string Nombre { get; set; }
        public virtual ICollection<tbActividades> Actividades { get; set; }

    }
}
