using System;

namespace ControlActividades.Models
{
    /// <summary>
    /// Respuesta estándar para operaciones exitosas
    /// </summary>
    public class SuccessResponse
    {
        public string Mensaje { get; set; }
        public string Codigo { get; set; } = "EXITO";
        public object Datos { get; set; }
    }

    /// <summary>
    /// Respuesta estándar para errores
    /// </summary>
    public class ErrorResponse
    {
        public string Mensaje { get; set; }
        public string Codigo { get; set; }
        public string Detalles { get; set; }
    }

    /// <summary>
    /// Códigos de error para eliminación de materias
    /// </summary>
    public static class MateriaErrorCodes
    {
        public const string MATERIA_NO_ENCONTRADA = "MATERIA_NO_ENCONTRADA";
        public const string MATERIA_CON_ALUMNOS = "MATERIA_CON_ALUMNOS";
        public const string MATERIA_CON_ACTIVIDADES = "MATERIA_CON_ACTIVIDADES";
        public const string MATERIA_CON_AVISOS = "MATERIA_CON_AVISOS";
        public const string ERROR_INTERNO = "ERROR_INTERNO";
    }

    /// <summary>
    /// Códigos de error para eliminación de grupos
    /// </summary>
    public static class GrupoErrorCodes
    {
        public const string GRUPO_NO_ENCONTRADO = "GRUPO_NO_ENCONTRADO";
        public const string GRUPO_CON_ALUMNOS = "GRUPO_CON_ALUMNOS";
        public const string GRUPO_CON_ACTIVIDADES = "GRUPO_CON_ACTIVIDADES";
        public const string GRUPO_MATERIAS_CON_AVISOS = "GRUPO_MATERIAS_CON_AVISOS";
        public const string GRUPO_CON_AVISOS = "GRUPO_CON_AVISOS";
        public const string ERROR_INTERNO = "ERROR_INTERNO";
    }

    /// <summary>
    /// Códigos de error para eliminación de alumnos
    /// </summary>
    public static class AlumnoErrorCodes
    {
        public const string ALUMNO_NO_ENCONTRADO = "ALUMNO_NO_ENCONTRADO";
        public const string ALUMNO_CON_ENTREGAS = "ALUMNO_CON_ENTREGAS";
        public const string ALUMNO_CON_CALIFICACIONES = "ALUMNO_CON_CALIFICACIONES";
        public const string ERROR_INTERNO = "ERROR_INTERNO";
    }
}
