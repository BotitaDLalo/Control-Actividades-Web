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
    public class MateriasApiCAService : IMateriasApiService
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



        public async Task<List<MateriaCARes>> ObtenerMaterias(int ca_usuarioId, int st_usuarioId, string role)
        {

            List<int> lsMateriasUsuario = new List<int>();

            if (role == Roles.ALUMNO)
            {
                lsMateriasUsuario = Db.tbAlumnosMaterias.Where(a => a.AlumnoId == ca_usuarioId).Select(a => a.MateriaId).ToList(); ;
            }
            else if (role == Roles.DOCENTE)
            {
                List<int> lsMateriasId = await Db.tbMaterias.Where(a => a.DocenteId == ca_usuarioId).Select(a => a.MateriaId).ToListAsync();

                List<int> lsGruposMateriasId = await Db.tbGruposMaterias.Where(a => lsMateriasId.Contains(a.MateriaId)).Select(a => a.MateriaId).ToListAsync();

                lsMateriasId = lsMateriasId.Where(a => !lsGruposMateriasId.Contains(a)).ToList();

                lsMateriasUsuario = lsMateriasId;
            }

            var lsMateriasSinGrupo = Db.tbMaterias.Where(a => lsMateriasUsuario.Contains(a.MateriaId)).Select(a => new MateriaCARes
            {
                MateriaId = a.MateriaId,
                NombreMateria = a.NombreMateria,
                Descripcion = a.Descripcion,
                CodigoAcceso = a.CodigoAcceso,
                Actividades = Db.tbActividades.Where(b => b.MateriaId == a.MateriaId).Select(b => new ActividadCARes
                {
                    ActividadId = b.ActividadId,
                    NombreActividad = b.NombreActividad,
                    Descripcion = b.Descripcion,
                    FechaCreacion = b.FechaCreacion,
                    FechaLimite = b.FechaLimite,
                    Puntaje = (int)b.Puntaje,
                    MateriaId = b.MateriaId
                }).ToList()
            }).ToList();



            return lsMateriasSinGrupo;
        }

    }
}