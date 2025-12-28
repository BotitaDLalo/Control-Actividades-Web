
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;


namespace ControlActividades
{
    public static class Roles
    {
        public static string DOCENTE => "Docente";
        public static string ALUMNO => "Alumno";
        public static string ADMINISTRADOR => "Administrador";
    }
    public static class EstatusAutorizacion
    {
        public static string AUTORIZADO => "Autorizado";
        public static string DENEGADO => "Denegado";
        public static string PENDIENTE => "Pendiente";
    }

    public static class EstatusEnvioCorreoDocente
    {
        public static string NO_ENVIADO => "Sin enviar";
        public static string ENVIADO => "Enviado";
    }

    public static class RequiereDatosAdicionales
    {
        public static bool REQUERIDO => true;
        public static bool NO_REQUERIDO => false;
    }

    public static class SessionKeys
    {
        public static string Email => "Email";
    }


    public enum ErrorCodigos
    {
        CredencialesInvalidas = 1001,
        CorreoUsuarioExistente = 1002,
        tokenInvalido = 1003,
        codigoAutorizacionInvalido = 1004,
        codigoAutorizacionExpirado = 1005,
        nombreUsuarioUsado = 1006,
        emailNoValido = 1007,
    }

    public static class Errores
    {
        private static readonly Dictionary<ErrorCodigos, string> DiccionarioErrores = new Dictionary<ErrorCodigos, string>()
    {
        { ErrorCodigos.CredencialesInvalidas, "Correo o contraseña son incorrectos." },
        { ErrorCodigos.CorreoUsuarioExistente, "Este correo ya está asociado a otra cuenta." },
        { ErrorCodigos.tokenInvalido, "Token de autenticacion invalido." },
        { ErrorCodigos.nombreUsuarioUsado, "Nombre de usuario ya esta en uso." },
        { ErrorCodigos.emailNoValido, "El correo ya no es valido." }
    };

        public static string GetMensajeError(ErrorCodigos errorCodigos)
        {
            string message;
            if (DiccionarioErrores.TryGetValue(errorCodigos, out message))
            {
                return message;
            }
            return "Error desconocido";
        }
    }

    public enum TiposNotificaciones
    {
        Aviso = 1,
        Evento = 2,
        ActividadCreada = 3
    }
}
