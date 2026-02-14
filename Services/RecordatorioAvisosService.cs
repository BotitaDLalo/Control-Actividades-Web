using ControlActividades.Models;
using ControlActividades.Models.db;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class RecordatorioAvisosService
{
    public async Task EjecutarRecordatoriosDiarios()
    {
        using (var db = new ApplicationDbContext())
        {
            var hoy = DateTime.Today;

            var avisosActivos = db.tbAvisos
                .Where(a =>
                    a.FechaInicio <= hoy &&
                    a.FechaFin >= hoy)
                .ToList();

            foreach (var aviso in avisosActivos)
            {
                var diasDesdeInicio = (hoy - aviso.FechaInicio.Date).Days;

                if (diasDesdeInicio % aviso.FrecuenciaDias != 0)
                    continue;

                // Verificar si ya se envió hoy
                bool yaEnviado = db.tbAvisosEnvios
                    .Any(e => e.AvisoId == aviso.AvisoId && e.FechaEnvio == hoy);

                if (yaEnviado)
                    continue;

                // IMPLEMENTACIÓN DE NOTIFICACIONES(Después)
                System.Diagnostics.Debug.WriteLine($"ENVIANDO recordatorio aviso {aviso.AvisoId}");

                // Registrar envío
                db.tbAvisosEnvios.Add(new tbAvisosEnvios
                {
                    AvisoId = aviso.AvisoId,
                    FechaEnvio = hoy
                });

                await db.SaveChangesAsync();
            }
        }
    }
}

