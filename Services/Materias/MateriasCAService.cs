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

    }
}