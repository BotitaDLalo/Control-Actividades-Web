using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Exceptions;
using ControlActividades.Interfaces.Actividades;
using ControlActividades.Models;
using ControlActividades.Models.db;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace ControlActividades.Services.Actividades
{
    public class ActividadesApiCAService : IActividadesApiService, IDisposable
    {
        #region Propiedades
        private ApplicationUserManager _userManager;
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
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
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
        public async Task<ActividadesDTO> ActualizarActividad(int id, tbActividades updatedActivity)
        {
            var actividad = await Db.tbActividades.FindAsync(id);
            //if (actividad is null) return Content(HttpStatusCode.NotFound, "Actividad no encontrada");
            if (actividad is null) throw new ActividadException("Actividad no encontrada");


            actividad.NombreActividad = updatedActivity.NombreActividad ?? actividad.NombreActividad;
            actividad.Descripcion = updatedActivity.Descripcion ?? actividad.Descripcion;
            actividad.FechaLimite = updatedActivity.FechaLimite != default(DateTime) ? updatedActivity.FechaLimite : actividad.FechaLimite;
            actividad.Puntaje = updatedActivity.Puntaje;

            actividad.Enviado = updatedActivity.Enviado ?? actividad.Enviado;
            actividad.FechaProgramada = updatedActivity.FechaProgramada ?? actividad.FechaProgramada;

            await Db.SaveChangesAsync();

            var actividadesDTO = new ActividadesDTO
            {
                ActividadId = actividad.ActividadId,
                NombreActividad = actividad.NombreActividad,
                Descripcion = actividad.Descripcion,
                FechaCreacion = actividad.FechaCreacion,
                FechaLimite = actividad.FechaLimite,
                Puntaje = actividad.Puntaje,
                MateriaId = actividad.MateriaId,
                PermitirEntregasTarde = actividad.PermitirEntregasTarde,
                Enviado = actividad.Enviado,
                FechaProgramada = actividad.FechaProgramada,
                LimiteEntregasPorAlumno = actividad.LimiteEntregasPorAlumno,
                TieneLimiteEntregas = actividad.TieneLimiteEntregas
            };


            return actividadesDTO;

        }

        public async Task AsignarCalificacion(int entregableId, decimal calificacion)
        {
            var entregaAlumno = Db.tbEntregaActividadAlumno.FirstOrDefault(a => a.EntregaActividadAlumnoId == entregableId);
            if (entregaAlumno == null) throw new Exception();

            entregaAlumno.Calificacion = calificacion;
            entregaAlumno.FechaCalificado = DateTime.Now;

            Db.Entry(entregaAlumno).State = EntityState.Modified;
            await Db.SaveChangesAsync();
        }

        public async Task CrearActividad(tbActividades nuevaActividad)
        {
            int materiaId = nuevaActividad.MateriaId;

            // Verificar si la materia existe
            var materia = await Db.tbMaterias.FindAsync(materiaId);
            if (materia == null)
            {
                throw new ActividadException("La materia asociada no existe.");
            }

            // Validar campos no nulos o con valores incorrectos
            if (string.IsNullOrWhiteSpace(nuevaActividad.NombreActividad))
            {
                throw new ActividadException("El nombre de la actividad es obligatorio.");
            }

            if (nuevaActividad.FechaLimite == default(DateTime))
            {
                throw new ActividadException("La fecha límite de la actividad es inválida.");
            }

            // Generar automáticamente la fecha de creación
            nuevaActividad.FechaCreacion = DateTime.Now;


            nuevaActividad.Enviado = true;


            //nuevaActividad.TipoActividadId = 1;

            // Guardar la actividad en la base de datos
            Db.tbActividades.Add(nuevaActividad);
            await Db.SaveChangesAsync();
        }

        public async Task EliminarActividad(int id)
        {
            var activity = await Db.tbActividades.FirstOrDefaultAsync(a => a.ActividadId == id);

            if (activity is null) throw new ActividadException("Actividad no encontrada");

            //NO PERMITIR QUE SE ELIMINE LA ACTIVIDAD SI YA TIENE:
            /*
             ->Entrega del alumno
            -> Calificacion
             */


            var existeEntrega = Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == activity.ActividadId).Any();
            if (existeEntrega)
                //return BadRequest();
                throw new ActividadException("El alumno ya ha realizado una entrega previamente.");


            Db.tbActividades.Remove(activity);
            await Db.SaveChangesAsync();
        }

        public async Task<List<ObtenerActividadesPorMateriaRes>> ObtenerActividadesPorMateria(int materiaId)
        {
            var actividades = await Db.tbActividades.Where(a => a.MateriaId == materiaId).ToListAsync();


            var listaActividades = actividades.Select(a => new ObtenerActividadesPorMateriaRes
            {
                ActividadId = a.ActividadId,
                NombreActividad = a.NombreActividad,
                DescripcionActividad = a.Descripcion,
                FechaCreacionActividad = a.FechaCreacion.ToString("yyyy-MM-ddTHH:mm:ss"),
                FechaLimiteActividad = a.FechaLimite.ToString("yyyy-MM-ddTHH:mm:ss"),
                Puntaje = a.Puntaje,
                Enviado = a.Enviado,
                FechaProgramada = a.FechaProgramada,
                MateriaId = a.MateriaId
            }).ToList();

            return listaActividades;
        }

        public async Task<RespuestaAlumnosEntregables> ObtenerAlumnosEntregables(int actividadId)
        {
            List<AlumnoEntregable> lsEntregables = new List<AlumnoEntregable>();
            RespuestaAlumnosEntregables respuestaAlumnos = new RespuestaAlumnosEntregables();


            var lsAlumnosActividades = await Db.tbEntregaActividadAlumno.Where(a => a.ActividadId == actividadId && a.EstadoEntregaId == 1)
                .Include(a => a.tbAlumnos)
                .Include(a => a.tbEntregables)
                .ToListAsync();

            var puntaje = await Db.tbActividades.Where(a => a.ActividadId == actividadId).Select(a => a.Puntaje).FirstOrDefaultAsync();

            int totalEntregados = lsAlumnosActividades.Count;

            respuestaAlumnos.ActividadId = actividadId;
            respuestaAlumnos.Puntaje = puntaje;
            respuestaAlumnos.TotalEntregados = totalEntregados;

            foreach (var alumnoActividad in lsAlumnosActividades)
            {
                AlumnoEntregable alumnoEntregable = new AlumnoEntregable();

                var alumno = alumnoActividad.tbAlumnos;
                var entregableAlumno = alumnoActividad.tbEntregables;

                var alumnoId = alumno.AlumnoId;
                var userId = alumno.UserId;
                var nombres = alumno.Nombre;
                var apellidoPaterno = alumno.ApellidoPaterno;
                var apellidoMaterno = alumno.ApellidoMaterno;
                var user = await UserManager.FindByIdAsync(userId ?? "");




                foreach (var entregable in entregableAlumno.ToList())
                {
                    var userName = user.UserName;
                    alumnoEntregable.AlumnoId = alumnoId;
                    alumnoEntregable.NombreUsuario = userName ?? "";
                    alumnoEntregable.Nombres = nombres ?? "";
                    alumnoEntregable.ApellidoPaterno = apellidoPaterno ?? "";
                    alumnoEntregable.ApellidoMaterno = apellidoMaterno ?? "";
                    alumnoEntregable.FechaEntrega = alumnoActividad.FechaEntrega;
                    //alumnoEntregable.EntregaId = entregable.EntregableId;
                    alumnoEntregable.EntregaId = entregable.EntregaActividadAlumnoId;
                    alumnoEntregable.Calificacion = entregable.Calificacion ?? 0;
                    alumnoEntregable.FechaCalificado = entregable.tbEntregaActividadAlumno.FechaCalificado;

                    string contenidoRaw = entregable.Contenido ?? "";
                    try
                    {
                        if (!string.IsNullOrEmpty(contenidoRaw))
                        {
                            contenidoRaw = contenidoRaw.Replace("\\\"", "\"");
                            var contenidoObj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(contenidoRaw);

                            string texto = contenidoObj?.texto ?? "";
                            var enlaces = contenidoObj?.enlaces != null ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(contenidoObj.enlaces.ToString()) : new List<string>();
                            var archivos = contenidoObj?.archivos != null ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<object>>(contenidoObj.archivos.ToString()) : new List<object>();

                            alumnoEntregable.Texto = texto;
                            alumnoEntregable.Enlaces = enlaces;
                            alumnoEntregable.Archivos = archivos;
                            alumnoEntregable.FechaEntregaContenido = contenidoObj?.fechaEntrega != null ? DateTime.Parse(contenidoObj.fechaEntrega.ToString()) : (DateTime?)null;
                            alumnoEntregable.TotalArchivos = contenidoObj?.totalArchivos ?? 0;
                            alumnoEntregable.TotalEnlaces = contenidoObj?.totalEnlaces ?? 0;
                            alumnoEntregable.Respuesta = contenidoRaw;
                        }
                        else
                        {
                            alumnoEntregable.Respuesta = "";
                            alumnoEntregable.Texto = "";
                            alumnoEntregable.Enlaces = new List<string>();
                            alumnoEntregable.Archivos = new List<object>();
                            alumnoEntregable.TotalArchivos = 0;
                            alumnoEntregable.TotalEnlaces = 0;
                        }
                    }
                    catch
                    {
                        alumnoEntregable.Respuesta = contenidoRaw;
                        alumnoEntregable.Texto = "";
                        alumnoEntregable.Enlaces = new List<string>();
                        alumnoEntregable.Archivos = new List<object>();
                        alumnoEntregable.TotalArchivos = 0;
                        alumnoEntregable.TotalEnlaces = 0;
                    }

                    lsEntregables.Add(alumnoEntregable);
                }

            }

            respuestaAlumnos.AlumnosEntregables = lsEntregables;

            return respuestaAlumnos;
        }

        public async Task<ObtenerEnviosActividadesAlumnoRes> ObtenerEnviosActividadesAlumno(int ActividadId, int AlumnoId)
        {
            var datosAlumnoActividad = await Db.tbEntregaActividadAlumno.FirstOrDefaultAsync(a => a.ActividadId == ActividadId && a.AlumnoId == AlumnoId);
            if (datosAlumnoActividad == null)
                throw new EntregableNoEncontradoException();


            var entregaActividadId = datosAlumnoActividad.EntregaActividadAlumnoId;
            var fechaEntrega = datosAlumnoActividad?.FechaEntrega;
            var estadoEntregaId = datosAlumnoActividad.EstadoEntregaId;

            var lsEntregas = await Db.tbEntregables.Where(a => a.EntregaActividadAlumnoId == entregaActividadId)
                .Select(e => new Entregables
                {
                    EntregableId = e.EntregableId,
                    TipoEntregaId = e.TipoEntregaId,
                    Contenido = e.Contenido,
                    // FechaCalificado puede no existir en la BD en algunas instalaciones; omitimos su lectura aquí
                    Calificacion = e.Calificacion ?? 0,
                    Comentario = e.Comentario
                }).ToListAsync();


            var res = new ObtenerEnviosActividadesAlumnoRes()
            {
                EntregaActividadAlumnoId = entregaActividadId,
                FechaEntrega = fechaEntrega,
                EstadoEntregaId = estadoEntregaId,
                Entregables = lsEntregas
            };

            return res;
        }

        public async Task QuitarCalificacion(int entregableId)
        {
            var entregaAlumno = Db.tbEntregaActividadAlumno.FirstOrDefault(a => a.EntregaActividadAlumnoId == entregableId);

            if (entregaAlumno == null) throw new Exception();

            entregaAlumno.Calificacion = 0;
            entregaAlumno.FechaCalificado = null;

            Db.Entry(entregaAlumno).State = EntityState.Modified;
            await Db.SaveChangesAsync();
        }
    }
}