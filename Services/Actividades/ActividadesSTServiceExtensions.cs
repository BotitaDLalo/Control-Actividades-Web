using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ControlActividades.Models;

namespace ControlActividades.Services.Actividades
{
 // Extensión para mantener compatibilidad con llamadas que pasan el rol como string
 public static class ActividadesSTServiceExtensions
 {
 public static Task<List<ActividadRes>> ObtenerActividadesPorMateria(this ActividadesSTService servicio, int materiaId, string rol)
 {
 bool esDocente = false;
 try
 {
 if (!string.IsNullOrWhiteSpace(rol))
 {
 var r = rol.Trim();
 esDocente = r.Equals("Docente", StringComparison.OrdinalIgnoreCase) || r.Equals("Administrador", StringComparison.OrdinalIgnoreCase) || r.Equals("DOCENTE", StringComparison.OrdinalIgnoreCase);
 }
 }
 catch
 {
 esDocente = false;
 }

 return servicio.ObtenerActividadesPorMateria(materiaId, esDocente);
 }
 }
}
