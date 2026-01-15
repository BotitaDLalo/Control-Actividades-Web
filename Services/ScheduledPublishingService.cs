using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;
using ControlActividades.Models.db;
using ControlActividades.Models;

namespace ControlActividades.Services
{
    public static class ScheduledPublishingService
    {
        private static Timer _timer;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
        private static readonly object _lock = new object();

        public static void Start()
        {
            if (_timer != null) return;
            _timer = new Timer(async _ => await Tick(), null, TimeSpan.FromSeconds(10), Interval);
        }

        public static void Stop()
        {
            lock (_lock)
            {
                _timer?.Dispose();
                _timer = null;
            }
        }

        private static async Task Tick()
        {
            if (!Monitor.TryEnter(_lock)) return;
            try
            {
                using (var db = new ApplicationDbContext())
                {
                    var now = DateTime.Now;
                    var pendientes = await db.tbActividades
                        .Where(a => a.Enviado == null && a.FechaProgramada.HasValue && a.FechaProgramada.Value <= now)
                        .ToListAsync();

                    foreach (var act in pendientes)
                    {
                        act.Enviado = true;
                        var alumnoIds = await db.tbAlumnosMaterias
                            .Where(am => am.MateriaId == act.MateriaId)
                            .Select(am => am.AlumnoId)
                            .ToListAsync();

                        foreach (var alumnoId in alumnoIds)
                        {
                            //var exists = await db.tbAlumnosActividades.AnyAsync(aa => aa.ActividadId == act.ActividadId && aa.AlumnoId == alumnoId);
                            //if (!exists)
                            //{
                            //    db.tbAlumnosActividades.Add(new tbAlumnosActividades
                            //    {
                            //        ActividadId = act.ActividadId,
                            //        AlumnoId = alumnoId,
                            //        FechaEntrega = DateTime.Now,
                            //        EstatusEntrega = false
                            //    });
                            //}
                        }
                    }

                    //await db.SaveChangesAsync();
                }
            }
            catch { }
            finally { try { Monitor.Exit(_lock); } catch { } }
        }
    }
}
