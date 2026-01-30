namespace ControlActividades.Models.Enums
{
    // Enum para tipos de actividad. Los valores por defecto intentan coincidir
    // con los tipos que normalmente se insertan en la tabla `cTiposActividades`.
    // Si la base de datos contiene valores distintos, la aplicación usará
    // primero los registros de la BD (ver EnumHelpers.ObtenerTiposActividad).
    public enum TipoActividadEnum
    {
        Tarea = 1,
        Examen = 2,
        Cuestionario = 3
    }
}
