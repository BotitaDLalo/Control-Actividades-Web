using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlActividades.Models
{
    public class ObtenerGruposPorUsuarioRequest
    {
        public string Role { get; set; }

        public int ca_usuarioId { get; set; }

        public int st_usuarioId { get; set; }

        public string View { get; set; }
    }

    public class ObtenerGrupoCreadoCARequest
    {
        public int ca_usuarioId { get; set; }
        public int st_usuarioId { get; set; }
    }

    public class ObtenerMateriasCARequest
    {
        public int ca_usuarioId { get; set; }
        public int st_usuarioId { get; set; }
        public string Role { get; set; }

        public string View { get; set; }
    }

    public class ObtenerMateriaDetallesRequest
    {
        public int MateriaId { get; set; }

        public int GrupoId { get; set; }
        public string View { get; set; }
        public int st_usuarioId { get; set; }

        public string Role { get; set; }
    }


    public class RegistrarAlumnoRequest
    {
        public List<int> AlumnosId { get; set; }

        public int? MateriaId { get; set; }

        public int? GrupoId { get; set; }
    }


    public class ObtenerMateriasPorGrupoRequest
    {
        public string Role { get; set; }

        public int st_usuarioId { get; set; }

        public int ca_usuarioId { get; set; }

        public int GrupoId { get; set; }
    }


    public class TieneClasesRequest
    {
        public string Role { get; set; }

        public int st_usuarioId { get; set; }

        public int ca_usuarioId { get; set; }
    }

    // ACTIVIDADES //
    public class ObtenerActividadesPorMateriaRequest
    {
        public string Role { get; set; }
        public int MateriaId { get; set; }
        public string View { get; set; }
    }

    public class ObtenerActividadPorIdRequest
    {
        public int ActividadId { get; set;}
        public string View { get; set; }
    }

    public class ActividadDetallesRes
    {
        public int ActividadId { get; set; }
        public string NombreActividad { get; set; }
        public string Descripcion { get; set; }
        public int MateriaId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaLimite { get; set; }
        public decimal Puntaje { get; set; }
        public bool? Enviado { get; set; }
        public bool PermitirEntregasTarde { get; set; }
        public DateTime? FechaProgramada { get; set; }
    }
    public class GruposSTRequest
    {
        public int ca_usuarioId { get; set; }
        public int st_usuarioId { get; set; }
        public string View { get; set; }
    }

    public class GrupoSTModel
    {
        public int GrupoId { get; set; }
        public string NombreGrupo { get; set; }
        public string Descripcion { get; set; }
        public string CodigoAcceso { get; set; }
    }

    public class GruposSTResponse
    {
        public List<GrupoSTModel> Grupos { get; set; }
    }

    public class GenericSTResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ActividadDTO
    {
        public string NombreActividad { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaLimite { get; set; }
        public decimal Puntaje { get; set; }
        public int MateriaId { get; set; }
        public bool? Enviado { get; set; }
        public DateTime? FechaProgramada { get; set; }
        public bool PermitirEntregasTarde { get; set; }
        public bool TieneLimiteEntregas { get; set; }
        public int LimiteEntregasPorAlumno { get; set; }
    }

}