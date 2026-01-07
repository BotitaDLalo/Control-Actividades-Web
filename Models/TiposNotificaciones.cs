using System;

namespace ControlActividades.Models
{
    // Enumeración para los tipos de notificaciones usados por NotificacionesService
    public enum TiposNotificaciones
    {
        ActividadCalificada = 1,
        ActividadCreada = 2,
        ActividadEntregada = 3,
        Aviso = 4,
        Evento = 5,
        GrupoAsignado = 6,
        MateriaAsignada = 7
    }
}
