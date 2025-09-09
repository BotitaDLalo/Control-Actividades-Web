using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;


namespace ControlActividades.Models.db
{
    public class tbDocentes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocenteId { get; set; }
        
        [Required]
        public  string ApellidoPaterno { get; set; }
        
        [Required]
        public string ApellidoMaterno { get; set; }

        [Required]
        public  string Nombre { get; set; }
        
        public bool? estaAutorizado { get; set; }
        public bool seEnvioCorreo {  get; set; }
        
        public DateTime? FechaExpiracionCodigo { get; set; }
        public string CodigoAutorizacion {  get; set; }

        public virtual ApplicationUser IdentityUser { get; set; }
        [ForeignKey("IdentityUser")]
        [Required]
        public  string UserId { get; set; }
        public virtual ICollection<tbGrupos> Grupos { get; set; }
        public virtual ICollection<tbMaterias> Materias { get; set; }
        public virtual ICollection<tbEventosAgenda> EventosAgendas { get; set; }
        public virtual ICollection<tbAvisos> Avisos { get; set; }
    }
}
