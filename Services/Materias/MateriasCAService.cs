using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Materias;
using ControlActividades.Models;

namespace ControlActividades.Services.Materias
{
    public class MateriasCAService : IMateriasService
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

        public async Task<MateriaViewModel> ObtenerMateriaDetalles(int materiaId, int docenteId)
        {
            var materiaDetalles = await Db.tbMaterias
                .Where(m => m.MateriaId == materiaId && m.DocenteId == docenteId)
                .Select(m => new MateriaViewModel
                {
                    NombreMateria = m.NombreMateria,
                    CodigoAcceso = m.CodigoAcceso,
                    CodigoColor = m.CodigoColor,
                    DocenteId = m.DocenteId
                }).FirstOrDefaultAsync();

            return materiaDetalles;
        }

        public List<MateriaViewModel> ObtenerMateriasSinGrupoPorUsuario(int usuarioId, string role)
        {
            try
            {
                List<MateriaViewModel> materiasSinGrupo = new List<MateriaViewModel>();

                if (role == Roles.DOCENTE)
                {
                    materiasSinGrupo = Db.tbMaterias
                    .Where(m => m.DocenteId == usuarioId && !Db.tbGruposMaterias.Any(gm => gm.MateriaId == m.MateriaId))
                    .Select(a => new MateriaViewModel
                    {
                        MateriaId = a.MateriaId,
                        GrupoId = a.GruposMaterias.Where(gm => gm.MateriaId == a.MateriaId).Select(gm => gm.GrupoId).FirstOrDefault(),
                        NombreMateria = a.NombreMateria,
                        Descripcion = a.Descripcion,
                        ApellidoPaternoDocente = a.Docentes.ApellidoPaterno,
                        ApellidoMaternoDocente = a.Docentes.ApellidoMaterno,
                        NombresDocente = a.Docentes.Nombre
                    })
                    .ToList();

                }
                else if (role == Roles.ALUMNO)
                {
                    var lsMateriasAlumno = Db.tbAlumnosMaterias.Where(a => a.AlumnoId == usuarioId).Select(a => a.MateriaId).ToList();

                    lsMateriasAlumno = lsMateriasAlumno.Where(a => !Db.tbGruposMaterias.Any(gm => gm.MateriaId == a)).ToList();

                    materiasSinGrupo = Db.tbMaterias.Where(a => lsMateriasAlumno.Contains(a.MateriaId)).Select(a => new MateriaViewModel
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
                return new List<MateriaViewModel>();
            }
        }

        public Task<ActividadRes> CrearActividadAsync(ActividadDTO actividad)
        {
            throw new NotImplementedException();
        }

    }
}