using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ControlActividades.Models.db
{
    [Table("tbAuditoria")]
    public class tbAuditoria
    {
        public int Id { get; set; }

        // Quién hizo la acción
        public string AdminId { get; set; }
        public string AdminEmail { get; set; }

        // Usuario impersonado
        public string UsuarioImpersonadoId { get; set; }
        public string UsuarioImpersonadoEmail { get; set; }

        // Acción realizada
        public string Accion { get; set; }
        public string Controlador { get; set; }

        // Información extra opcional
        public string Descripcion { get; set; }

        // Datos técnicos
        public DateTime DateUtc { get; set; }
        public string DireccionIp { get; set; }
    }
}