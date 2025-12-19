using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ControlActividades.Models.db
{
    [Table("cTipoNotificacion")]
    public class cTipoNotificacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TipoNotificacionId { get; set; }
        
        [Required]
        public string Nombre { get; set; }

        public ICollection<tbNotificaciones> tbNotificaciones { get; set; }
    }
}