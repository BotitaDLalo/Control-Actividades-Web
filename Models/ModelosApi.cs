using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Models
{

    public static class Views
    {
        public static string WEB => "web";

        public static string MOVIL => "movil";
    }


    public class GruposCreadoCARes
    {
        public int GrupoId { get; set; }

        public string NombreGrupo { get; set; }
    }

    public class GruposCARes
    {
        public int GrupoId { get; set; }

        public string NombreGrupo { get; set; }

        public string Descripcion { get; set; }

        public string CodigoColor { get; set; }

        public string CodigoAcceso { get; set; }

        public int? DocenteId { get; set; }

        public string ApellidoPaternoDocente { get; set; }

        public string ApellidoMaternoDocente { get; set; }

        public string NombresDocente { get; set; }

        public List<MateriaCARes> Materias { get; set; }
    }


    public class MateriaCARes
    {
        public int MateriaId { get; set; }

        public int? GrupoId { get; set; }

        public string NombreMateria { get; set; }

        public string Descripcion { get; set; }

        public string CodigoColor { get; set; }

        public string CodigoAcceso { get; set; }

        public string ApellidoPaternoDocente { get; set; }

        public string ApellidoMaternoDocente { get; set; }

        public string NombresDocente { get; set; }

        public int? DocenteId { get; set; }

        public List<ActividadCARes> Actividades { get; set; }
    }

    public class ActividadCARes
    {
        public int ActividadId { get; set; }

        public string NombreActividad { get; set; }

        public string Descripcion { get; set; }

        public DateTime? FechaCreacion { get; set; }

        public DateTime? FechaLimite { get; set; }

        public int Puntaje { get; set; }

        public int MateriaId { get; set; }

        public int? GrupoId {  get; set; }
    }

    public class DetallesActividadRes
    {
        public int ActividadId { get; set; }

        public int MateriaId { get; set; }
    }


}