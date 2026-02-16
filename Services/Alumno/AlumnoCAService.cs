using ControlActividades.Exceptions;
using ControlActividades.Interfaces.Alumnos;
using ControlActividades.Models;
using ControlActividades.Models.db;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ControlActividades.Services.Alumno
{
    public class AlumnoCAService : IAlumnoService, IDisposable
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

        public async Task<UnirseAClaseMRespuesta> UnirseAClase(int alumnoId, string codigoAcceso)
        {
            var codigoNormalizado = codigoAcceso.Trim().ToUpper();
            var grupo = await Db.tbGrupos
                .FirstOrDefaultAsync(g => g.CodigoAcceso.ToUpper() == codigoNormalizado);

            if (grupo != null)
            {
                var docente = await Db.tbDocentes
                    .FirstOrDefaultAsync(d => d.DocenteId == grupo.DocenteId);

                if (docente == null)
                {
                    throw new AlumnosException("Docente no encontrado. El grupo no tiene un docente asociado válido.", "");
                }

                var alumnoYaEnGrupo = await Db.tbAlumnosGrupos
                    .AnyAsync(ag => ag.AlumnoId == alumnoId && ag.GrupoId == grupo.GrupoId);

                if (alumnoYaEnGrupo)
                {
                    // El alumno ya está registrado en este grupo
                    throw new AlumnosException($"Ya estás registrado en el grupo '{grupo.NombreGrupo}'. No puedes unirte nuevamente.", "");
                }

                // 6. Obtener materias del grupo
                var lsMateriasId = await Db.tbGruposMaterias
                    .Where(gm => gm.GrupoId == grupo.GrupoId)
                    .Select(gm => gm.MateriaId)
                    .ToListAsync();

                var lsMaterias = await Db.tbMaterias
                    .Where(m => lsMateriasId.Contains(m.MateriaId))
                    .Select(m => new MateriaRes
                    {
                        MateriaId = m.MateriaId,
                        NombreMateria = m.NombreMateria,
                        Descripcion = m.Descripcion,
                        Actividades = Db.tbActividades
                            .Where(a => a.MateriaId == m.MateriaId)
                            .Select(a => new ActividadRes
                            {
                                ActividadId = a.ActividadId,
                                NombreActividad = a.NombreActividad,
                                Descripcion = a.Descripcion,
                                FechaCreacion = a.FechaCreacion,
                                FechaLimite = a.FechaLimite,
                                //Puntaje = a.Puntaje
                            })
                            .ToList()
                    })
                    .ToListAsync();

                // 7. Crear respuesta del grupo
                var grupoRes = new GrupoRes()
                {
                    GrupoId = grupo.GrupoId,
                    NombreGrupo = grupo.NombreGrupo,
                    Descripcion = grupo.Descripcion,
                    CodigoAcceso = grupo.CodigoAcceso,
                    CodigoColor = string.IsNullOrEmpty(grupo.CodigoColor) ? "#2196F3" : grupo.CodigoColor,
                    Materias = lsMaterias
                };

                // 8. Crear relación alumno-grupo
                var nuevaRelacion = new tbAlumnosGrupos
                {
                    AlumnoId = alumnoId,
                    GrupoId = grupo.GrupoId
                };

                Db.tbAlumnosGrupos.Add(nuevaRelacion);
                await Db.SaveChangesAsync();

                // 9. Retornar respuesta exitosa
                var respuesta = new UnirseAClaseMRespuesta()
                {
                    Grupo = grupoRes,
                    EsGrupo = true
                };

                return respuesta;
            }

            // 10. Si no es grupo, buscar materia con comparación case-insensitive
            var materia = await Db.tbMaterias
                .FirstOrDefaultAsync(m => m.CodigoAcceso.ToUpper() == codigoNormalizado);

            if (materia != null)
            {
                // 11. Validar que el docente existe
                var docente = await Db.tbDocentes
                    .FirstOrDefaultAsync(d => d.DocenteId == materia.DocenteId);

                if (docente == null)
                {
                    //return Content(HttpStatusCode.NotFound, new
                    //{
                    //    mensaje = "Docente no encontrado. La materia no tiene un docente asociado válido."
                    //});

                    throw new AlumnosException("Docente no encontrado. La materia no tiene un docente asociado válido.", "");
                }

                // 12. ✅ VALIDAR si el alumno YA ESTÁ registrado en esta materia
                var alumnoYaEnMateria = await Db.tbAlumnosMaterias
                    .AnyAsync(am => am.AlumnoId == alumnoId && am.MateriaId == materia.MateriaId);

                if (alumnoYaEnMateria)
                {
                    // El alumno ya está registrado en esta materia
                    //return Content(HttpStatusCode.Conflict, new
                    //{
                    //    mensaje = $"Ya estás registrado en la materia '{materia.NombreMateria}'. No puedes unirte nuevamente.",
                    //    materiaId = materia.MateriaId,
                    //    nombreMateria = materia.NombreMateria,
                    //    esGrupo = false
                    //});

                    throw new AlumnosException($"Ya estás registrado en la materia '{materia.NombreMateria}'. No puedes unirte nuevamente.", "");
                }

                // 13. Crear respuesta de la materia
                var materiaRes = new MateriaRes()
                {
                    MateriaId = materia.MateriaId,
                    NombreMateria = materia.NombreMateria,
                    Descripcion = materia.Descripcion,
                    Actividades = await Db.tbActividades
                        .Where(a => a.MateriaId == materia.MateriaId)
                        .Select(a => new ActividadRes
                        {
                            ActividadId = a.ActividadId,
                            NombreActividad = a.NombreActividad,
                            Descripcion = a.Descripcion,
                            FechaCreacion = a.FechaCreacion,
                            FechaLimite = a.FechaLimite,
                            //Puntaje = a.Puntaje
                        })
                        .ToListAsync()
                };

                // 14. Crear relación alumno-materia
                var nuevaRelacion = new tbAlumnosMaterias
                {
                    AlumnoId = alumnoId,
                    MateriaId = materia.MateriaId
                };

                Db.tbAlumnosMaterias.Add(nuevaRelacion);
                await Db.SaveChangesAsync();

                // 15. Retornar respuesta exitosa
                var respuesta = new UnirseAClaseMRespuesta()
                {
                    Materia = materiaRes,
                    EsGrupo = false
                };

                return respuesta;
            }

            throw new AlumnosException("Código de acceso inválido o inexistente. Verifica que el código sea correcto.", "");
        }

    }
}