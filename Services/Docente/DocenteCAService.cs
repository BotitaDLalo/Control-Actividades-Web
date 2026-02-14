using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ControlActividades.Interfaces.Docente;
using ControlActividades.Models;

namespace ControlActividades.Services.Docente
{
    public class DocenteCAService : IDocentesService, IDisposable
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
    }
}