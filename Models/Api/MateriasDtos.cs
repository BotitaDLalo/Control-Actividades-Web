using System;
using System.Collections.Generic;

namespace ControlActividades.Models.Api
{
    public class ActividadDto
    {
        public int ActividadId { get; set; }
        public string NombreActividad { get; set; }
        public string Descripcion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaLimite { get; set; }
        public int Puntaje { get; set; }
        public int MateriaId { get; set; }
    }

    public class MateriaDto
    {
        public int MateriaId { get; set; }
        public string NombreMateria { get; set; }
        public string Descripcion { get; set; }
        public string CodigoAcceso { get; set; }
        public string CodigoColor { get; set; }
        public int? DocenteId { get; set; }
        public List<ActividadDto> Actividades { get; set; } = new List<ActividadDto>();
    }

    public class MateriasResponseDto
    {
        public List<MateriaDto> Materias { get; set; } = new List<MateriaDto>();
        public string View { get; set; }
    }

    public class MateriaResponseDto
    {
        public MateriaDto Subject { get; set; }
        public string View { get; set; }
    }
}
