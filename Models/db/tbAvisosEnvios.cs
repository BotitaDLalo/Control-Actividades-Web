using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlActividades.Models.db
{
    public class tbAvisosEnvios
    {
        [Key]
        public int AvisoEnvioId { get; set; }

        [Required]
        public int AvisoId { get; set; }

        [Required]
        public DateTime FechaEnvio { get; set; }

        
        [ForeignKey("AvisoId")]
        public virtual tbAvisos Aviso { get; set; }
    }
}