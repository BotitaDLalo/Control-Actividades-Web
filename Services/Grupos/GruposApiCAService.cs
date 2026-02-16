using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ControlActividades.Interfaces.Grupos;
using ControlActividades.Models;
using ControlActividades.Models.db;
using Newtonsoft.Json;

namespace ControlActividades.Services.Grupos
{
    public class GruposApiCAService : IGruposApiService
    {
        private ApplicationDbContext _db;

        public ApplicationDbContext Db
        {
            get { return _db ?? (_db = new ApplicationDbContext()); }
            private set { _db = value; }
        }

        public async Task<List<GruposCreadoCARes>> ObtenerGruposCreados(int ca_usuarioId, int st_usuarioId)
        {
            try
            {
                return await Db.tbGrupos
                    .Where(a => a.DocenteId == ca_usuarioId)
                    .Select(a => new GruposCreadoCARes
                    {
                        GrupoId = a.GrupoId,
                        NombreGrupo = a.NombreGrupo
                    })
                    .ToListAsync();
            }
            catch
            {
                return new List<GruposCreadoCARes>();
            }
        }

        public async Task<List<GruposCARes>> ObtenerGruposMaterias(int ca_usuarioId, int st_usuarioId, string role)
        {
            try
            {
                List<tbGrupos> lsGrupos;

                if (role == Roles.DOCENTE)
                {
                    lsGrupos = await Db.tbGrupos
                        .Where(a => a.DocenteId == ca_usuarioId)
                        .ToListAsync();
                }
                else
                {
                    var lsGruposAlumnosId = await Db.tbAlumnosGrupos
                        .Where(a => a.AlumnoId == ca_usuarioId)
                        .Select(a => a.GrupoId)
                        .ToListAsync();

                    lsGrupos = await Db.tbGrupos
                        .Where(a => lsGruposAlumnosId.Contains(a.GrupoId))
                        .ToListAsync();
                }

                var listaGruposMaterias = new List<GruposCARes>();

                foreach (var grupo in lsGrupos)
                {
                    var lsMateriasId = await Db.tbGruposMaterias
                        .Where(a => a.GrupoId == grupo.GrupoId)
                        .Select(a => a.MateriaId)
                        .ToListAsync();

                    var lsMaterias = await Db.tbMaterias
                        .Where(m => lsMateriasId.Contains(m.MateriaId))
                        .Select(m => new MateriaCARes
                        {
                            MateriaId = m.MateriaId,
                            NombreMateria = m.NombreMateria,
                            Descripcion = m.Descripcion,
                            CodigoAcceso = m.CodigoAcceso,

                            Actividades = Db.tbActividades
                                .Where(a => a.MateriaId == m.MateriaId)
                                .Select(b => new ActividadCARes
                                {
                                    ActividadId = b.ActividadId,
                                    NombreActividad = b.NombreActividad,
                                    Descripcion = b.Descripcion,
                                    FechaCreacion = b.FechaCreacion,
                                    FechaLimite = b.FechaLimite,
                                    Puntaje = (int)b.Puntaje,
                                    MateriaId = b.MateriaId,
                                    PermitirEntregasTarde = b.PermitirEntregasTarde,
                                    TieneLimiteEntregas = b.TieneLimiteEntregas,
                                    LimiteEntregasPorAlumno = b.LimiteEntregasPorAlumno
                                }).ToList(),

                            Avisos = Db.tbAvisos
                                .Where(aviso => aviso.MateriaId == m.MateriaId
                                              && aviso.GrupoId == grupo.GrupoId)
                                .Select(aviso => new AvisoCARes
                                {
                                    AvisoId = aviso.AvisoId,
                                    Titulo = aviso.Titulo,
                                    Descripcion = aviso.Descripcion,
                                    FechaCreacion = aviso.FechaCreacion,
                                    FechaInicio = aviso.FechaInicio,
                                    FechaFin = aviso.FechaFin,
                                    FrecuenciaDias = aviso.FrecuenciaDias,
                                    GrupoId = aviso.GrupoId ?? 0,
                                    MateriaId = aviso.MateriaId ?? 0,
                                    EnlacesRaw = aviso.Enlaces // ⚠ solo string aquí
                                }).ToList()
                        })
                        .ToListAsync();

                    // 🔥 Deserializar Enlaces fuera del query (correcto para EF)
                    foreach (var materia in lsMaterias)
                    {
                        foreach (var aviso in materia.Avisos)
                        {
                            aviso.Enlaces = string.IsNullOrEmpty(aviso.EnlacesRaw)
                                ? new List<string>()
                                : JsonConvert.DeserializeObject<List<string>>(aviso.EnlacesRaw);
                        }
                    }

                    listaGruposMaterias.Add(new GruposCARes
                    {
                        GrupoId = grupo.GrupoId,
                        NombreGrupo = grupo.NombreGrupo,
                        Descripcion = grupo.Descripcion,
                        CodigoAcceso = grupo.CodigoAcceso,
                        CodigoColor = grupo.CodigoColor,
                        Materias = lsMaterias
                    });
                }

                return listaGruposMaterias;
            }
            catch
            {
                return new List<GruposCARes>();
            }
        }
    }
}
