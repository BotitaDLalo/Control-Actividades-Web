using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces;
using ControlActividades.Models;

namespace ControlActividades.Services
{
    public class GruposCAService : IGruposService, IDisposable
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

        public async Task<List<GruposCARes>> ObtenerGruposPorUsuario(string role, int ca_usuarioId, int st_usuarioId)
        {
            List<GruposCARes> grupos = new List<GruposCARes>();

            if (role == Roles.DOCENTE)
            {
                grupos = await Db.tbGrupos.Where(g => g.DocenteId == ca_usuarioId)
                        .Select(g => new GruposCARes
                        {
                            GrupoId = g.GrupoId,
                            NombreGrupo = g.NombreGrupo,
                            Descripcion = g.Descripcion,
                            CodigoColor = g.CodigoColor,
                            CodigoAcceso = g.CodigoAcceso,
                            ApellidoPaternoDocente = g.Docentes.ApellidoPaterno,
                            ApellidoMaternoDocente = g.Docentes.ApellidoMaterno,
                            NombresDocente = g.Docentes.Nombre,
                        }).ToListAsync();
            }
            else if (role == Roles.ALUMNO)
            {
                var lsAlumnoGruposId = Db.tbAlumnosGrupos.Where(a => a.AlumnoId == ca_usuarioId).Select(a => a.GrupoId).ToList();

                var lsAlumnoMateriasId = Db.tbAlumnosMaterias.Where(a => a.AlumnoId == ca_usuarioId).Select(a => a.MateriaId).ToList();

                var lsMateriasGrupoId = Db.tbGruposMaterias.Where(a => lsAlumnoMateriasId.Contains(a.MateriaId)).Select(a => a.GrupoId).Distinct().ToList();

                lsAlumnoGruposId.AddRange(lsMateriasGrupoId);

                grupos = await Db.tbGrupos.Where(g => lsAlumnoGruposId.Contains(g.GrupoId))
                        .Select(g => new GruposCARes
                        {
                            GrupoId = g.GrupoId,
                            NombreGrupo = g.NombreGrupo,
                            Descripcion = g.Descripcion,
                            CodigoColor = g.CodigoColor,
                            CodigoAcceso = g.CodigoAcceso,
                            ApellidoPaternoDocente = g.Docentes.ApellidoPaterno,
                            ApellidoMaternoDocente = g.Docentes.ApellidoMaterno,
                            NombresDocente = g.Docentes.Nombre
                        }).ToListAsync();
            }

            return grupos;
        }

        public async Task<List<MateriaCARes>> ObtenerMateriasPorGrupo(int grupoId, int ca_usuarioId, int st_usuarioId, string role)
        {
            var materiasIds = await Db.tbGruposMaterias
                .Where(gm => gm.GrupoId == grupoId)
                .Select(gm => gm.MateriaId)
                .ToListAsync();

            var docenteGrupo = await Db.tbGrupos.Where(a => a.GrupoId == grupoId).Select(a => new { a.Docentes.ApellidoPaterno, a.Docentes.ApellidoMaterno, a.Docentes.Nombre }).FirstOrDefaultAsync();

            if (role == Roles.ALUMNO)
            {
                //var usuarioId = Fg.ObtenerUsuarioId(User);
                var alumnoPerteneceGrupo = await Db.tbAlumnosGrupos.Where(a => a.AlumnoId == ca_usuarioId && a.GrupoId == grupoId).AnyAsync();

                if (!alumnoPerteneceGrupo)
                {
                    materiasIds = materiasIds.Where(a => Db.tbAlumnosMaterias.Any(am => am.AlumnoId == ca_usuarioId && am.MateriaId == a)).ToList();
                }
            }

            var lsMaterias = await Db.tbMaterias
                .Where(m => materiasIds.Contains(m.MateriaId))
                .Select(m => new MateriaCARes
                {
                    MateriaId = m.MateriaId,
                    GrupoId = grupoId,
                    NombreMateria = m.NombreMateria,
                    Descripcion = m.Descripcion,
                    CodigoColor = m.CodigoColor,
                    ApellidoPaternoDocente = docenteGrupo.ApellidoPaterno,
                    ApellidoMaternoDocente = docenteGrupo.ApellidoMaterno,
                    NombresDocente = docenteGrupo.Nombre
                }).ToListAsync();

            return lsMaterias;
        }

        public async Task<bool> TieneGrupos(string role, int ca_usuarioId, int st_usuarioId)
        {
            if (role == Roles.DOCENTE)
            {
                var docenteTieneGrupos = await Db.tbGrupos.Where(a => a.DocenteId == ca_usuarioId).AnyAsync();
                return docenteTieneGrupos;

            }
            else if (role == Roles.ALUMNO)
            {
                var alumnoTieneGrupos = await Db.tbAlumnosGrupos.Where(a => a.AlumnoId == ca_usuarioId).AnyAsync();
                return alumnoTieneGrupos;
            }
            return false;
        }

        public async Task<bool> TieneMaterias(string role, int ca_usuarioId, int st_usuarioId)
        {
            if (role == Roles.DOCENTE)
            {
                var docenteTieneMaterias = await Db.tbMaterias.Where(a => a.DocenteId == ca_usuarioId).AnyAsync();
                return docenteTieneMaterias;
            }
            else if (role == Roles.ALUMNO)
            {
                var alumnoTieneMaterias = await Db.tbAlumnosMaterias.Where(a => a.AlumnoId == ca_usuarioId).AnyAsync();
                return alumnoTieneMaterias;
            }

            return false;
        }
    }
}