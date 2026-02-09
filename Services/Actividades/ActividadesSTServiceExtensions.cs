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
 // Llamar directamente a la firma existente que espera el rol como string.
 // La versión anterior intentaba convertir a bool y llamar a una sobrecarga que no existe en este proyecto.
 return servicio.ObtenerActividadesPorMateria(materiaId, rol);
 }
 }
}
