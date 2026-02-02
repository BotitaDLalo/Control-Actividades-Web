using System;
namespace ControlActividades.Models
{
    /// <summary>
    /// Códigos de error específicos para operaciones con materias.
    /// </summary>
    // Nombres únicos para evitar colisiones con otros tipos en el proyecto.
    public enum ApiMateriaErrorCodes
    {
        MATERIA_NO_ENCONTRADA = 1001,
        MATERIA_CON_ALUMNOS = 1002,
        MATERIA_CON_ACTIVIDADES = 1003,
        MATERIA_CON_AVISOS = 1004,
        ERROR_INTERNO = 1500
    }

    /// <summary>
    /// Estructura estándar para errores devueltos por la API.
    /// </summary>
    public class ApiErrorResponse
    {
        public string Mensaje { get; set; }
        public object Codigo { get; set; }
        public string Detalles { get; set; }
        public object Data { get; set; }
    }

    /// <summary>
    /// Respuesta estándar de éxito para operaciones.
    /// </summary>
    public class ApiSuccessResponse
    {
        public string Mensaje { get; set; }
        public string Codigo { get; set; }
        public object Datos { get; set; }
    }

    /// <summary>
    /// Resultado genérico para endpoints que devuelven un único objeto.
    /// </summary>
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
    }
}
