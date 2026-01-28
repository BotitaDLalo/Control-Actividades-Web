using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using ControlActividades.Interfaces;
using ControlActividades.Models;

namespace ControlActividades.Services
{
    public class GruposCAService : IGruposService
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

        public List<GrupoViewModel> ObtenerGruposPorUsuario(string rol, int usuarioId)
        {
            List<GrupoViewModel> grupos = new List<GrupoViewModel>();

            if (rol == Roles.DOCENTE)
            {
                grupos = Db.tbGrupos.Where(g => g.DocenteId == usuarioId)
                        .Select(g => new GrupoViewModel
                        {
                            GrupoId = g.GrupoId,
                            NombreGrupo = g.NombreGrupo,
                            Descripcion = g.Descripcion,
                            CodigoColor = g.CodigoColor,
                            CodigoAcceso = g.CodigoAcceso,
                            ApellidoPaternoDocente = g.Docentes.ApellidoPaterno,
                            ApellidoMaternoDocente = g.Docentes.ApellidoMaterno,
                            NombresDocente = g.Docentes.Nombre,
                        })
                        .ToList();
            }
            else if (rol == Roles.ALUMNO)
            {
                var lsAlumnoGruposId = Db.tbAlumnosGrupos.Where(a => a.AlumnoId == usuarioId).Select(a => a.GrupoId).ToList();

                var lsAlumnoMateriasId = Db.tbAlumnosMaterias.Where(a => a.AlumnoId == usuarioId).Select(a => a.MateriaId).ToList();

                var lsMateriasGrupoId = Db.tbGruposMaterias.Where(a => lsAlumnoMateriasId.Contains(a.MateriaId)).Select(a => a.GrupoId).Distinct().ToList();

                lsAlumnoGruposId.AddRange(lsMateriasGrupoId);

                grupos = Db.tbGrupos.Where(g => lsAlumnoGruposId.Contains(g.GrupoId))
                        .Select(g => new GrupoViewModel
                        {
                            GrupoId = g.GrupoId,
                            NombreGrupo = g.NombreGrupo,
                            Descripcion = g.Descripcion,
                            CodigoColor = g.CodigoColor,
                            CodigoAcceso = g.CodigoAcceso,
                            ApellidoPaternoDocente = g.Docentes.ApellidoPaterno,
                            ApellidoMaternoDocente = g.Docentes.ApellidoMaterno,
                            NombresDocente = g.Docentes.Nombre
                        })
                        .ToList();
            }

            return grupos;
        }

        public List<MateriaViewModel> ObtenerMateriasPorGrupo(int grupoId, int usuarioId, string rol)
        {
            var materiasIds = Db.tbGruposMaterias
                .Where(gm => gm.GrupoId == grupoId)
                .Select(gm => gm.MateriaId)
                .ToList();

            var docenteGrupo = Db.tbGrupos.Where(a => a.GrupoId == grupoId).Select(a => new { a.Docentes.ApellidoPaterno, a.Docentes.ApellidoMaterno, a.Docentes.Nombre }).FirstOrDefault();

            if (rol == Roles.ALUMNO)
            {
                //var usuarioId = Fg.ObtenerUsuarioId(User);
                var alumnoPerteneceGrupo = Db.tbAlumnosGrupos.Where(a => a.AlumnoId == usuarioId && a.GrupoId == grupoId).Any();

                if (!alumnoPerteneceGrupo)
                {
                    materiasIds = materiasIds.Where(a => Db.tbAlumnosMaterias.Any(am => am.AlumnoId == usuarioId && am.MateriaId == a)).ToList();
                }
            }

            var lsMaterias = Db.tbMaterias
                .Where(m => materiasIds.Contains(m.MateriaId))
                .Select(m => new MateriaViewModel
                {
                    MateriaId = m.MateriaId,
                    GrupoId = grupoId,
                    NombreMateria = m.NombreMateria,
                    Descripcion = m.Descripcion,
                    CodigoColor = m.CodigoColor,
                    ApellidoPaternoDocente = docenteGrupo.ApellidoPaterno,
                    ApellidoMaternoDocente = docenteGrupo.ApellidoMaterno,
                    NombresDocente = docenteGrupo.Nombre
                })
                .ToList();

            return lsMaterias;
        }

        public bool TieneGrupos(string role, int usuarioId)
        {
            if (role == Roles.DOCENTE)
            {
                var docenteTieneGrupos = Db.tbGrupos.Where(a => a.DocenteId == usuarioId).Any();
                return docenteTieneGrupos;

            }
            else if (role == Roles.ALUMNO)
            {
                var alumnoTieneGrupos = Db.tbAlumnosGrupos.Where(a => a.AlumnoId == usuarioId).Any();
                return alumnoTieneGrupos;
            }
            return false;
        }

        public bool TieneMaterias(string role, int usuarioId)
        {
            if (role == Roles.DOCENTE)
            {
                var docenteTieneMaterias = Db.tbMaterias.Where(a => a.DocenteId == usuarioId).Any();
                return docenteTieneMaterias;
            }
            else if (role == Roles.ALUMNO)
            {
                var alumnoTieneMaterias = Db.tbAlumnosMaterias.Where(a => a.AlumnoId == usuarioId).Any();
                return alumnoTieneMaterias;
            }

            return false;
        }
    }
}