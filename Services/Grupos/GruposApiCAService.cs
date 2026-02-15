using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Grupos;
using ControlActividades.Models;
using ControlActividades.Models.db;

namespace ControlActividades.Services.Grupos
{
    public class GruposApiCAService : IGruposApiService, IDisposable
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

        public async Task<List<GruposCreadoCARes>> ObtenerGruposCreados(int ca_usuarioId, int st_usuarioId)
        {
            try
            {
                var lsGrupos = await Db.tbGrupos.Where(a => a.DocenteId == ca_usuarioId)
                    .Select(a => new GruposCreadoCARes
                    {
                       GrupoId = a.GrupoId,
                        NombreGrupo = a.NombreGrupo
                    }).ToListAsync();

                return lsGrupos;
            }
            catch (Exception ex)
            {
                return new List<GruposCreadoCARes>();
            }
        }

        public async Task<List<GruposCARes>> ObtenerGruposMaterias(int ca_usuarioId, int st_usuarioId, string role)
        {
            List<tbGrupos> lsGrupos = new List<tbGrupos>();
            try
            {
                if (role == Roles.DOCENTE)
                {
                    lsGrupos = await Db.tbGrupos.Where(a => a.DocenteId == ca_usuarioId).ToListAsync();
                }
                else if (role == Roles.ALUMNO)
                {
                    var lsGruposAlumnosId = await Db.tbAlumnosGrupos.Where(a => a.AlumnoId == ca_usuarioId).Select(a => a.GrupoId).ToListAsync();

                    lsGrupos = await Db.tbGrupos.Where(a => lsGruposAlumnosId.Contains(a.GrupoId)).ToListAsync();
                }


                var listaGruposMaterias = new List<GruposCARes>();
                foreach (var grupo in lsGrupos)
                {
                    var lsMateriasId = await Db.tbGruposMaterias.Where(a => a.GrupoId == grupo.GrupoId).Select(a => a.MateriaId).ToListAsync();

                    var lsMaterias = await Db.tbMaterias.Where(a => lsMateriasId.Contains(a.MateriaId)).Select(m => new MateriaCARes
                    {
                        MateriaId = m.MateriaId,
                        NombreMateria = m.NombreMateria,
                        Descripcion = m.Descripcion,
                        Actividades = Db.tbActividades.Where(a => a.MateriaId == m.MateriaId).Select(b => new ActividadCARes
                        {
                            ActividadId = b.ActividadId,
                            NombreActividad = b.NombreActividad,
                            Descripcion = b.Descripcion,
                            FechaCreacion = b.FechaCreacion,
                            FechaLimite = b.FechaLimite,
                            //b.TipoActividadId,
                            //Puntaje = b.Puntaje,
                            MateriaId = b.MateriaId,
                        }).ToList()
                    }).ToListAsync();


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
            catch (Exception)
            {
                return new List<GruposCARes>();
            }
        }
    }
}