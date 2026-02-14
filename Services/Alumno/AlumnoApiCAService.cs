using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Interfaces.Alumnos;
using ControlActividades.Models;

namespace ControlActividades.Services.Alumno
{
    public class AlumnoApiCAService : IAlumnoApiService, IDisposable
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

        public Task RegistrarEnvioActividadAlumnoConEnlaces(int actividadId, int alumnoId, int tipoEntrega, string fechaEntrega, string respuestaRaw, string enlacesJson)
        {
            throw new NotImplementedException();
        }
    }
}