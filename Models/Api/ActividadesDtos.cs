using System;
using System.Collections.Generic;

namespace ControlActividades.Models.Api
{
    public class AlumnoEntregableDto
    {
        public int EntregaId { get; set; }
        public int AlumnoId { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string Respuesta { get; set; }
        public int? Calificacion { get; set; }
    }

    public class RespuestaAlumnosEntregablesDto
    {
        public int ActividadId { get; set; }
        public int Puntaje { get; set; }
        public int TotalEntregados { get; set; }
        public List<AlumnoEntregableDto> AlumnosEntregables { get; set; } = new List<AlumnoEntregableDto>();
    }
}
