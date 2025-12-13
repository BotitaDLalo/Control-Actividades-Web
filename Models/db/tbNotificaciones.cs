using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;

namespace ControlActividades.Models.db
{
    public class tbNotificaciones
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificacionId { get; set; }
        [ForeignKey("IdentityUser")]
        [Required]
        public string UserId { get; set; }
        public  string MessageId { get; set; }
        public  string Title {  get; set; }
        public  string Body {  get; set; }
        public string Tipo { get; set; }
        public DateTime FechaRecibido { get; set; }
        public virtual ApplicationUser IdentityUser { get; set; }
    }
}
