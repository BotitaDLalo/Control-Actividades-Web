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

            var materias = await Db.tbMaterias
    .Where(a => lsMateriasUsuario.Contains(a.MateriaId))
    .ToListAsync(); // 🔥 ejecutamos primero

            var lsMateriasSinGrupo = materias.Select(a => new MateriaCARes
            {
                MateriaId = a.MateriaId,
                NombreMateria = a.NombreMateria,
                Descripcion = a.Descripcion,
                CodigoAcceso = a.CodigoAcceso,

                Actividades = Db.tbActividades
                    .Where(b => b.MateriaId == a.MateriaId)
                    .Select(b => new ActividadCARes
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
                    })
                    .ToList(),

                Avisos = Db.tbAvisos
                    .Where(aviso => aviso.MateriaId == a.MateriaId && aviso.GrupoId == null)
                    .Join(Db.tbDocentes,
                          aviso => aviso.DocenteId,
                          docente => docente.DocenteId,
                          (aviso, docente) => new { aviso, docente })
                    .ToList() // 🔥 ejecutamos antes de deserializar
                    .Select(x => new AvisoCARes
                    {
                        AvisoId = x.aviso.AvisoId,
                        Titulo = x.aviso.Titulo,
                        Descripcion = x.aviso.Descripcion,
                        NombresDocente = x.docente.Nombre,
                        ApePaternoDocente = x.docente.ApellidoPaterno,
                        ApeMaternoDocente = x.docente.ApellidoMaterno,
                        FechaCreacion = x.aviso.FechaCreacion,
                        FechaInicio = x.aviso.FechaInicio,
                        FechaFin = x.aviso.FechaFin,

                        Enlaces = string.IsNullOrEmpty(x.aviso.Enlaces)
                            ? new List<string>()
                            : JsonConvert.DeserializeObject<List<string>>(x.aviso.Enlaces),

                        FrecuenciaDias = x.aviso.FrecuenciaDias,
                        GrupoId = x.aviso.GrupoId ?? 0,
                        MateriaId = x.aviso.MateriaId ?? 0
                    })
                    .ToList()

            }).ToList();




            return lsMateriasSinGrupo;
        }

    }
}