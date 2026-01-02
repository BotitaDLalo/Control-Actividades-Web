using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ControlActividades.Models.db;

namespace ControlActividades.Models
{
    public class AsignarCalificacionPeticion
    {
        public int EntregaId { get; set; }

        public int EntregableId { get; set; }

        public int Calificacion { get; set; }
    }
    public class ProblemDetails
    {
        public int Status { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Instance { get; set; }
        public Dictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();
    }

    public class AlumnoGMRegistroCodigo
    {
        [Required]
        public int AlumnoId { get; set; }

        [Required]
        public string CodigoAcceso { get; set; }
    }

    public class AlumnoGMRegistroDocente
    {
        [Required]
        public List<string> Emails { get; set; }

        public int MateriaId { get; set; } = 0;
        public int GrupoId { get; set; } = 0;
    }

    public class AutenticacionRespuesta
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Correo { get; set; }
        public string Rol { get; set; }
        public string Token { get; set; }
        public string EstaAutorizado { get; set; }
        public bool? RequiereDatosAdicionales { get; set; }
    }

    public class CancelarEnvioActividadAlumno
    {
        [Required]
        public int AlumnoActividadId { get; set; }

        [Required]
        public int ActividadId { get; set; }

        [Required]
        public int AlumnoId { get; set; }
    }

    public class DatosFaltantesGoogle
    {
        [Required]
        public string Nombres { get; set; }

        [Required]
        public string ApellidoPaterno { get; set; }

        [Required]
        public string ApellidoMaterno { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public string IdToken { get; set; }

        public string FcmToken { get; set; }
    }

    public class DocentesValidacion
    {
        public string UserId { get; set; }

        public int DocenteId { get; set; }

        [Required]
        public string ApellidoPaterno { get; set; }

        [Required]
        public string ApellidoMaterno { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Email { get; set; }

        public string Autorizado { get; set; }
        public string EnvioCorreo { get; set; }
    }

    public class EmailConfiguration
    {
        [Required]
        public string From { get; set; }

        [Required]
        public string SMTPServer { get; set; }

        public int Port { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class EmailVerificadoAlumno
    {
        [Required]
        public string Email { get; set; }

        public string UserName { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
    }

    public class EntregableAlumno
    {
        [Required]
        public int ActividadId { get; set; }

        [Required]
        public int AlumnoId { get; set; }

        public string Respuesta { get; set; }

        [Required]
        public string FechaEntrega { get; set; }

        public List<string> Enlaces { get; set; }
    }

    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }

    public class GrupoMateriasRegistro
    {
        [Required]
        public int DocenteId { get; set; }

        [Required]
        public string NombreGrupo { get; set; }

        public string Descripcion { get; set; }

        public string CodigoAcceso { get; set; }

        [Required]
        public List<MateriasP> Materias { get; set; }
    }

    public class MateriasP
    {
        [Required]
        public string NombreMateria { get; set; }

        public string Descripcion { get; set; }
    }

    public class Indices
    {
        public int GrupoId { get; set; } = 0;
        public int MateriaId { get; set; } = 0;
    }

    public class PeticionAlumnosEntregables
    {
        public int ActividadId { get; set; }
    }

    public class ValidarCodigoDocente
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string CodigoValidar { get; set; }

        public string IdToken { get; set; }
    }

    public class MateriaConGrupo
    {
        [Required]
        public string NombreMateria { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public int DocenteId { get; set; }

        [Required]
        public List<int> Grupos { get; set; }
    }

    public class PeticionConsultarAvisos
    {
        public int GrupoId { get; set; } = 0;
        public int MateriaId { get; set; } = 0;
    }

    public class PeticionCrearAviso
    {
        public int DocenteId { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public string Descripcion { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int? GrupoId { get; set; }
        public int? MateriaId { get; set; }
    }


    public class RegistrarUsuarioGoogle
    {
        [Required]
        public string IdToken { get; set; }
    }

    public class RespuestaAlumnosEntregables
    {
        public int ActividadId { get; set; }
        public int Puntaje { get; set; }
        public int TotalEntregados { get; set; }
        public List<AlumnoEntregable> AlumnosEntregables { get; set; }
    }


    public class AlumnoEntregable
    {
        public int EntregaId { get; set; }
        public int AlumnoId { get; set; }
        public string NombreUsuario { get; set; }
        public string Nombres { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
        public DateTime FechaEntrega { get; set; }
        public string Respuesta { get; set; }
        public int Calificacion { get; set; }
    }


    public class RespuestaConsultarAvisos
    {
        public int AvisoId { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public string NombresDocente { get; set; }

        public string ApePaternoDocente { get; set; }

        public string ApeMaternoDocente { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int GrupoId { get; set; }

        public int MateriaId { get; set; }
    }


    public class TipoUsuario
    {
        [Key]
        public int TipoUsuarioId { get; set; }

        [Required]
        public string Usuario { get; set; } // obligatorio, usar RequiredAttribute
    }

    public class UsuarioInicioSesion
    {
        [Required]
        public string Correo { get; set; }

        [Required]
        public string Clave { get; set; }
    }

    public class UsuarioRegistro
    {
        public string NombreUsuario { get; set; }

        [Required]
        public string ApellidoPaterno { get; set; }

        [Required]
        public string ApellidoMaterno { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Correo { get; set; }

        [Required]
        public string Clave { get; set; }

        [Required]
        //public string TipoUsuario { get; set; }
        public Role TipoUsuario { get; set; }

        public string FcmToken { get; set; }
    }

    public class VerificarGoogleIdToken
    {
        [Required]
        public string IdToken { get; set; }
    }

    public class UnirseAClaseRequest
    {
        public int AlumnoId { get; set; }

        [Required]
        public string CodigoAcceso { get; set; }
    }

    public class GrupoRes
    {
        public int GrupoId { get; set; }

        public string NombreGrupo { get; set; }

        public string Descripcion { get; set; }

        public string CodigoAcceso { get; set; }

        public string CodigoColor { get; set; }

        public List<MateriaRes> Materias { get; set; }
    }

    public class MateriaRes
    {
        public int MateriaId { get; set; }

        public string NombreMateria { get; set; }

        public string Descripcion { get; set; }

        public List<tbActividades> Actividades { get; set; }
    }

    public class UnirseAClaseMRespuesta
    {
        public GrupoRes Grupo { get; set; }

        public MateriaRes Materia { get; set; }

        public bool? EsGrupo { get; set; }
    }

    public class Notificacion
    {
        public int NotificacionId { get; set; }
        public string UserId { get; set; }

        public string MessageId { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public string TipoNotificacion { get; set; }
        public int TipoId { get; set; }

        public DateTime FechaRecibido { get; set; }
    }

    public class ElementosNotificacion
    {
        [Required]
        public List<UsuarioFcmToken> LsUsuariosFcmTokens { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public string Descripcion { get; set; }
    }

    public class UsuarioFcmToken
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string FcmToken { get; set; }
    }

    public class CalificacionDto
    {
        public int EntregaId { get; set; }
        public int Calificacion { get; set; }
        public string Comentario { get; set; }
    }



    public class EvaluacionRequest
    {
        public List<AlumnoDTO> Alumnos { get; set; }
        public int ActividadId { get; set; }
    }

    public class AlumnoDTO
    {
        public int AlumnoId { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string ApellidoMaterno { get; set; }
    }

    public class EventoEditarDTO
    {
        public int EventoId { get; set; }

        [Required]
        public string Titulo { get; set; }

        [Required]
        public string Descripcion { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFinal { get; set; }

        [Required]
        public string Color { get; set; }

        public List<int> GruposSeleccionados { get; set; }
        public List<int> MateriasSeleccionadas { get; set; }

    }

    public class AsociarMateriasRequest
    {
        public int GrupoId { get; set; }
        public List<int> MateriaIds { get; set; }
    }

    public class Notification
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
    }
    public class TokenDispositivo
    {
        public string Token { get; set; }
    }


    public class EnvioRes
    {
        //        EntregaId = datosEntregable.EntregaId,
        //        AlumnoActividadId = entregaActividadId,
        //        Respuesta = datosEntregable?.Respuesta ?? "",
        //        Status = datosAlumnoActividad.EstatusEntrega,
        //        FechaEntrega = fechaEntrega,
        //        Calificacion = calificacion
        public int  EntregaActividadAlumnoId { get; set; } 
        
        public int EntregableId {  get; set; }

        public string Contenido { get; set; }

        public int EstadoEntregaId { get; set; }

        public DateTime FechaEntrega { get; set; }

        public string Calificacion { get; set; }

        public bool EstadoEntrega { get; set; }    
    }

    public class GrupoViewModel
    {
        public int GrupoId { get; set; }

        public string NombreGrupo { get; set; }

        public string Descripcion { get; set; }

        public string CodigoColor { get; set; }

        public string CodigoAcceso { get; set; }
    }

}