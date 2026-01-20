using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Services
{
    public class MigracionResultado
    {
        public int TotalRecibidos { get; set; }
        public int Insertados { get; set; }
        public int Fallidos { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
    }
}