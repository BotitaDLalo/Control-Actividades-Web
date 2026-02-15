using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        public int ActividadId { get; set; }
        public string View { get; set; }
    }

    public class ActividadDetallesRes
    {
        public int ActividadId { get; set; }
        public string NombreActividad { get; set; }
        public string Descripcion { get; set; }
        public int MateriaId { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaLimite { get; set; }
        public int Puntaje { get; set; }
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
        public int Puntaje { get; set; }
        public int MateriaId { get; set; }
        public bool? Enviado { get; set; }
        public DateTime? FechaProgramada { get; set; }
    }



    #region Actividades
    public class ObtenerEnviosActividadesAlumnoReq
    {
        public int AlumnoId { get; set; }
        public int ActividadId { get; set; }
    }
    public class ObtenerEnviosActividadesAlumnoRes
    {
        public int EntregaActividadAlumnoId { get; set; }

        public DateTime? FechaEntrega { get; set; }

        public int EstadoEntregaId { get; set; }

        public List<Entregables> Entregables { get; set; }
    }

    public class Entregables
    {
        public int EntregableId { get; set; }

        public int TipoEntregaId { get; set; }

        public string Contenido { get; set; }

        public decimal Calificacion { get; set; }

        public string Comentario { get; set; }
    }

    public class ObtenerActividadesPorMateriaReq
    {
        public int MateriaId { get; set; }
    }
    public class ObtenerActividadesPorMateriaRes
    {
        public int ActividadId { get; set; }

        public string NombreActividad { get; set; }

        public string DescripcionActividad { get; set; }

        public string FechaCreacionActividad { get; set; }

        public string FechaLimiteActividad { get; set; }

        public decimal Puntaje { get; set; }

        public bool? Enviado { get; set; }

        public DateTime? FechaProgramada { get; set; }

        public int MateriaId { get; set; }
    }

    public class ActividadesDTO
    {
        public int ActividadId { get; set; }

        public string NombreActividad { get; set; }

        public string Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaLimite { get; set; }

        public Decimal Puntaje { get; set; }

        public int MateriaId { get; set; }

        public bool PermitirEntregasTarde { get; set; }

        public bool? Enviado { get; set; }

        public DateTime? FechaProgramada { get; set; }

        public int LimiteEntregasPorAlumno { get; set; }

        public bool TieneLimiteEntregas { get; set; }
    }
    #endregion

    #region Alumnos
    public class RegistrarEnvioActividadRes
    {
        public int EntregaActividadAlumnoId { get; set; }

        public int AlumnoId { get; set; }

        public int EntregableId { get; set; }

        public int ActividadId { get; set; }

        public DateTime FechaEntrega { get; set; }

        public string Contenido { get; set; }

        public decimal Calificacion { get; set; }

        public int EstadoEntregaId { get; set; }

        public int TipoEntrega { get; set; }
    }

    public class RegistrarAlumnoGrupoMateriaDocenteRes
    {
        public bool AlumnoRegistradoGrupo {  get; set; }

        public bool AlumnoRegistradoMateria { get; set; }

        public List<EmailVerificadoAlumno> Alumnos { get; set; }
    }

    #endregion


    #region Actividades
    public class ActividadDetalleViewModel
    {
        public string NombreActividad { get; set; }

        public decimal Puntaje { get; set; }

        public DateTime? FechaLimite { get; set; }

        List<string> Enlaces { get; set; }

        public decimal Calificacion { get; set; }


    }
    #endregion
}