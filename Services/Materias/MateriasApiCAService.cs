using ControlActividades.Interfaces.Materias;
using ControlActividades.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ControlActividades.Services.Materias
{
    public class    MateriasApiCAService : IMateriasApiService
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
                    Puntaje = b.Puntaje,
                    MateriaId = b.MateriaId,
                    PermitirEntregasTarde = b.PermitirEntregasTarde,
                    TieneLimiteEntregas = b.TieneLimiteEntregas,
                    LimiteEntregasPorAlumno = b.LimiteEntregasPorAlumno
                }).ToList(),

                Avisos = (from aviso in Db.tbAvisos
                          join docente in Db.tbDocentes
                          on aviso.DocenteId equals docente.DocenteId into gj
                          from subdocente in gj.DefaultIfEmpty()
                          where aviso.MateriaId == a.MateriaId && aviso.GrupoId == null
                          select new AvisoCARes
                          {
                              AvisoId = aviso.AvisoId,
                              Titulo = aviso.Titulo,
                              Descripcion = aviso.Descripcion,

                              NombresDocente = subdocente != null ? subdocente.Nombre : "",
                              ApePaternoDocente = subdocente != null ? subdocente.ApellidoPaterno : "",
                              ApeMaternoDocente = subdocente != null ? subdocente.ApellidoMaterno : "",

                              FechaCreacion = aviso.FechaCreacion,
                              FechaInicio = aviso.FechaInicio,
                              FechaFin = aviso.FechaFin,

                              Enlaces = string.IsNullOrEmpty(aviso.Enlaces)
                                    ? new List<string>()
                                    : JsonConvert.DeserializeObject<List<string>>(aviso.Enlaces),

                              FrecuenciaDias = aviso.FrecuenciaDias,
                              GrupoId = aviso.GrupoId ?? 0,
                              MateriaId = aviso.MateriaId ?? 0
                          }).ToList(),

            }).ToList();



            return lsMateriasSinGrupo;
        }

    }
}