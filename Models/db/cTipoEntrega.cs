using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ControlActividades.Models.db
{
    [Table("cTipoEntregas")]
    public class cTipoEntrega
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TipoEntregaId { get; set; }
        
        [Required]
        public string Nombre { get; set; }

        public virtual ICollection<tbEntregables> tbEntregables { get; set; }
    }
}