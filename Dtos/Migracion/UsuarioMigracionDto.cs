using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Dtos.Migracion
{
    public class UsuarioMigracionDto
    {
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public string Correo { get; set; }
        public string PasswordPlano { get; set; }
        public string Rol { get; set; }
    }
}