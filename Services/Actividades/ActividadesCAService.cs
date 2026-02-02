using ControlActividades.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Services.Actividades
{
    public class ActividadesCAService
    {
        private ApplicationDbContext _db;

        public ApplicationDbContext Db
        {
            get
            {
                return _db ?? (_db = new ApplicationDbContext());
            }
            private set
            {
                _db = value;
            }
        }

        public async Task<List<ActividadRes>> ObtenerActividadesPorMateria(int materiaId, bool esDocente)
        {
            try
            {
                var query = Db.tbActividades.Where(a => a.MateriaId == materiaId);
                if (!esDocente)
                {
                    // Para alumnos: mostrar solo publicadas o programadas cuya fecha ya llegó
                    query = query.Where(a => a.Enviado == true ||
                                            (a.Enviado == null &&
                                             a.FechaProgramada.HasValue &&
                                             a.FechaProgramada.Value <= DateTime.Now
                                           )
                    );
                }


                // Ordenar por fecha de creación descendente para que lo más reciente aparezca primero
                var actividadesEntities = await query
                    .OrderByDescending(a => a.FechaCreacion)
                    .Select(a => new ActividadRes
                    {
                        ActividadId = a.ActividadId,
                        NombreActividad = a.NombreActividad,
                        Descripcion = a.Descripcion,
                        FechaCreacion = a.FechaCreacion,
                        FechaLimite = a.FechaLimite,
                        Puntaje = a.Puntaje
                    })
                    .ToListAsync();

                if (!actividadesEntities.Any())
                {
                    throw new KeyNotFoundException("No se encontraron actividades para la materia especificada.");
                }

                return actividadesEntities;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener las actividades: " + ex.Message);
            }
        }

        public async Task<ActividadDetallesRes> ObtenerActividadPorId(int actividadId)
        {
            
            try
            {
                // Cargar la entidad y mapear a DTO en memoria para evitar errores de traducción de EF
                var entidad = await Db.tbActividades.FirstOrDefaultAsync(a => a.ActividadId == actividadId);
                if (entidad == null)
                {
                    throw new KeyNotFoundException("No se encontró la actividad con el id especificado");
                }

                return new ActividadDetallesRes
                {
                    ActividadId = entidad.ActividadId,
                    NombreActividad = entidad.NombreActividad,
                    Descripcion = entidad.Descripcion,
                    MateriaId = entidad.MateriaId,
                    FechaCreacion = entidad.FechaCreacion,
                    FechaLimite = entidad.FechaLimite,
                    Puntaje = entidad.Puntaje,
                    Enviado = entidad.Enviado,
                    // PermitirEntregasTarde no está mapeado en la BD (NotMapped); devolver valor por defecto
                    PermitirEntregasTarde = entidad.PermitirEntregasTarde,
                    FechaProgramada = entidad.FechaProgramada
                };

            } 
            catch(Exception ex)
            {
                throw new Exception("Error al obtener la actividad: " + ex.Message);
            }
        }

        public async Task EliminarActividadAsync(int id)
        {
            try
            {
                var activity = await Db.tbActividades.FirstOrDefaultAsync(a => a.ActividadId == id);
                if (activity == null)
                {
                    throw new KeyNotFoundException("Actividad no encontrada.");
                }

                //var alumnoActividad = await Db.tbAlumnosActividades.FirstOrDefaultAsync(a => a.ActividadId == activity.ActividadId);
                var existenEntregas = await Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == activity.ActividadId).AnyAsync();
                if (existenEntregas)
                {
                    throw new InvalidOperationException("No se puede eliminar la actividad porque ya tiene entregas de alumnos.");
                }

                Db.tbActividades.Remove(activity);
                await Db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar la actividad: " + ex.Message);
            }

        }

    }
}