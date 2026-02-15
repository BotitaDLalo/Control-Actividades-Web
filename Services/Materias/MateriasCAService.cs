using ControlActividades.Interfaces.Materias;
using ControlActividades.Models;
using ControlActividades.Models.db;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ControlActividades.Services.Materias
{
    public class MateriasCAService : IMateriasService, IDisposable
    {
        #region Propiedades
        private ApplicationDbContext _db;
        private bool _disposed = false;

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }
                _disposed = true;
            }
        }
        #endregion

        public async Task<List<AlumnoCorreo>> BuscarAlumnosPorCorreo(string query)
        {
            var alumnosConCorreo = await (from a in Db.tbAlumnos
                                         join u in Db.Users on a.UserId equals u.Id
                                         where a.Nombre.Contains(query)
                                               || a.ApellidoPaterno.Contains(query)
                                               || a.ApellidoMaterno.Contains(query)
                                               || u.Email.Contains(query)
                                         orderby a.ApellidoPaterno, a.ApellidoMaterno, a.Nombre
                                         select new AlumnoCorreo
                                         {
                                             Email = u.Email,
                                             Nombre = a.Nombre,
                                             ApellidoPaterno = a.ApellidoPaterno,
                                             ApellidoMaterno = a.ApellidoMaterno
                                         }).Take(25).ToListAsync();
            return alumnosConCorreo;
        }

        public async Task< MateriaCARes> ObtenerMateriaDetalles(int materiaId, int grupoId, string role, int ca_usuarioId, int st_usuarioId)
        {
            try
            {
                var materiaDetalles = await Db.tbMaterias
                    .Where(m => m.MateriaId == materiaId)
                    .Select(m => new MateriaCARes
                    {
                        NombreMateria = m.NombreMateria,
                        CodigoAcceso = m.CodigoAcceso,
                        CodigoColor = m.CodigoColor,
                        DocenteId = m.DocenteId
                    }).FirstOrDefaultAsync();

                return materiaDetalles;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<MateriaCARes>> ObtenerMateriasSinGrupoPorUsuario(int usuarioId, int st_usuarioId, string role)
        {
            try
            {
                List< MateriaCARes> materiasSinGrupo = new List< MateriaCARes>();

                if (role == Roles.DOCENTE)
                {
                    materiasSinGrupo = await Db.tbMaterias
                    .Where(m => m.DocenteId == usuarioId && !Db.tbGruposMaterias.Any(gm => gm.MateriaId == m.MateriaId))
                    .Select(a => new  MateriaCARes
                    {
                        MateriaId = a.MateriaId,
                        GrupoId = a.GruposMaterias.Where(gm => gm.MateriaId == a.MateriaId).Select(gm => gm.GrupoId).FirstOrDefault(),
                        NombreMateria = a.NombreMateria,
                        Descripcion = a.Descripcion,
                        ApellidoPaternoDocente = a.Docentes.ApellidoPaterno,
                        ApellidoMaternoDocente = a.Docentes.ApellidoMaterno,
                        NombresDocente = a.Docentes.Nombre
                    })
                    .ToListAsync();

                }
                else if (role == Roles.ALUMNO)
                {
                    var lsMateriasAlumno = Db.tbAlumnosMaterias.Where(a => a.AlumnoId == usuarioId).Select(a => a.MateriaId).ToList();

                    lsMateriasAlumno = lsMateriasAlumno.Where(a => !Db.tbGruposMaterias.Any(gm => gm.MateriaId == a)).ToList();

                    materiasSinGrupo =  Db.tbMaterias.Where(a => lsMateriasAlumno.Contains(a.MateriaId)).Select(a => new  MateriaCARes
                    {
                        MateriaId = a.MateriaId,
                        GrupoId = a.GruposMaterias.Where(gm => gm.MateriaId == a.MateriaId).Select(gm => gm.GrupoId).FirstOrDefault(),
                        NombreMateria = a.NombreMateria,
                        Descripcion = a.Descripcion,
                        ApellidoPaternoDocente = a.Docentes.ApellidoPaterno,
                        ApellidoMaternoDocente = a.Docentes.ApellidoMaterno,
                        NombresDocente = a.Docentes.Nombre
                    }).ToList();
                }

                return materiasSinGrupo;
            }
            catch (Exception)
            {
                return new List< MateriaCARes>();
            }
        }

        public async Task<ActividadRes> CrearActividadAsync(ActividadDTO actividadDto)
        {
            try
            {
                // Verificar que la materia exista en la base de datos
                var materiaExiste = await Db.tbMaterias.AnyAsync(m => m.MateriaId == actividadDto.MateriaId);
                if (!materiaExiste)
                {
                    throw new Exception("La materia especificada no existe.");
                }

                // Crear la nueva actividad
                var nuevaActividad = new tbActividades
                {
                    NombreActividad = actividadDto.NombreActividad,
                    Descripcion = actividadDto.Descripcion,
                    FechaCreacion = DateTime.Now,
                    FechaLimite = actividadDto.FechaLimite,
                    Puntaje = actividadDto.Puntaje,
                    MateriaId = actividadDto.MateriaId,
                    Enviado = actividadDto.Enviado,
                    FechaProgramada = actividadDto.FechaProgramada
                };

                Db.tbActividades.Add(nuevaActividad);
                await Db.SaveChangesAsync(); // Guarda la actividad y genera el ID

                // Solo asignar a alumnos si la actividad está publicada inmediatamente
                // o si está programada y la fecha programada ya pasó
                bool publicarAhora = nuevaActividad.Enviado == true;
                bool programadaYA = nuevaActividad.Enviado == null &&
                                    nuevaActividad.FechaProgramada.HasValue &&
                                    nuevaActividad.FechaProgramada.Value <= DateTime.Now;

                if (publicarAhora || programadaYA)
                {
                    // Obtener los alumnos que pertenecen a la materia
                    var alumnosMateria = await Db.tbAlumnosMaterias
                        .Where(am => am.MateriaId == actividadDto.MateriaId)
                        .Select(am => am.AlumnoId)
                        .ToListAsync();

                    // Crear registros en la tabla AlumnoActividad para cada alumno
                    foreach (var alumnoId in alumnosMateria)
                    { /*
                        var alumnoActividad = new tbAlumnosActividades
                        {
                            ActividadId = nuevaActividad.ActividadId,
                            AlumnoId = alumnoId,
                            FechaEntrega = DateTime.Now, // Inicialmente la fecha de creación
                            EstatusEntrega = false
                        };
                        */
                        //Db.tbAlumnosActividades.Add(alumnoActividad);
                    }

                    // Guardar los cambios en la tabla AlumnoActividad
                    //await Db.SaveChangesAsync();
                }

                // Guardar los cambios en la tabla AlumnoActividad
                await Db.SaveChangesAsync();

                //Retorna el dto
                return new ActividadRes
                {
                    ActividadId = nuevaActividad.ActividadId,
                    NombreActividad = nuevaActividad.NombreActividad,
                    Descripcion = nuevaActividad.Descripcion,
                    FechaCreacion = nuevaActividad.FechaCreacion,
                    FechaLimite = nuevaActividad.FechaLimite,
                   // Puntaje = nuevaActividad.Puntaje
                };
            }
            catch (Exception ex)
            {
               var detalle = ex.InnerException?.InnerException?.Message
               ?? ex.InnerException?.Message
               ?? ex.Message;

                throw new Exception(detalle, ex);
            }
           

        }

    }
}