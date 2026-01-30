using System.Collections.Generic;
using System.Linq;
using ControlActividades.Models;
using ControlActividades.Models.Enums;
using ControlActividades.Models.db;

namespace ControlActividades.Recursos
{
    public static class EnumHelpers
    {
        // Devuelve un diccionario con los tipos de actividad existentes en BD
        // si no hay registros, devuelve los valores del enum por defecto.
        public static Dictionary<int, string> ObtenerTiposActividad(ApplicationDbContext db)
        {
            if (db == null) return ((TipoActividadEnum[])System.Enum.GetValues(typeof(TipoActividadEnum))).ToDictionary(k => (int)k, v => v.ToString());

            var set = db.Set<ControlActividades.Models.db.cTiposActividades>();
            if (set.Any())
            {
                return set.ToList().ToDictionary(t => t.TipoActividadId, t => t.Nombre);
            }

            return ((TipoActividadEnum[])System.Enum.GetValues(typeof(TipoActividadEnum))).ToDictionary(k => (int)k, v => v.ToString());
        }
    }
}
