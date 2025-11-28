using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services.Description;
using ControlActividades.Models;
using ControlActividades.Models.db;
using static Google.Apis.Requests.RequestError;

namespace ControlActividades.Services
{
    public class NotificacionesService
    {
        private ApplicationDbContext _db;
        private FCMService _fCMService;
        private bool disposed = false;
        public NotificacionesService()
        {
            _db = new ApplicationDbContext();
            _fCMService = new FCMService();
        }
        public NotificacionesService(ApplicationDbContext dbContext, FCMService fCMService=null)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _fCMService = fCMService ?? new FCMService();
        }

        public ApplicationDbContext Db => _db;

        public FCMService FCM
        {
            get
            {
                return _fCMService ?? (_fCMService = new FCMService());
            }
            set
            {
                _fCMService = value;
            }
        }

        public async Task RegistrarFcmTokenUsuario(string identityUserId, string fcmToken)
        {
            try
            {
                tbUsuariosFcmTokens usuarioToken = new tbUsuariosFcmTokens()
                {
                    UserId = identityUserId,
                    Token = fcmToken,
                };

                Db.tbUsuariosFcmTokens.Add(usuarioToken);
                await Db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }


        public async Task NotificacionCrearAviso(tbAvisos aviso, int? grupoId, int? materiaId)
        {
            List<int> lsAlumnosId = new List<int>();

            if (grupoId != null)
            {
                lsAlumnosId = await Db.tbAlumnosGrupos
                    .Where(a => a.GrupoId == grupoId)
                    .Select(a => a.AlumnoId)
                    .ToListAsync();
            }
            else if (materiaId != null)
            {
                lsAlumnosId = await Db.tbAlumnosMaterias
                    .Where(a => a.MateriaId == materiaId)
                    .Select(a => a.AlumnoId).ToListAsync();
            }

            //Lista de alumnos a notificar
            var lsAlumnosUserIds = await Db.tbAlumnos
                .Where(a => lsAlumnosId.Contains(a.AlumnoId))
                .Select(a => a.UserId)
                .ToListAsync();

            var lsFcmTokens = await Db.tbUsuariosFcmTokens
                .Where(a => lsAlumnosUserIds.Contains(a.UserId))
                .Select(a => new UsuarioFcmToken { FcmToken = a.Token, UserId = a.UserId })
                .ToListAsync();

            ElementosNotificacion notificacion = new ElementosNotificacion()
            {
                LsUsuariosFcmTokens = lsFcmTokens,
                Titulo = aviso.Titulo,
                Descripcion = aviso.Descripcion,
            };

            foreach (var t in lsFcmTokens)
            {
                System.Diagnostics.Debug.WriteLine("TOKEN: " + t.FcmToken);
            }

            await FCM.SendBatchNotificationsAsync(
                lsFcmTokens.Select(a => a.FcmToken).ToList(),
                notificacion.Titulo,
                notificacion.Descripcion
            );
            //await DetonarNotificaciones(notificacion);
        }


        public async Task NotificacionRegistrarAlumnoClase(List<int> lsAlumnosId, int docenteId, int grupoId = -1, int materiaId = -1)
        {
            List<UsuarioFcmToken> lsAlumnosFcmTokens = new List<UsuarioFcmToken>();
            string descrip = "";
            string nombreClase = "";
            foreach (var alumnoId in lsAlumnosId)
            {
                var userId = await Db.tbAlumnos.Where(a => a.AlumnoId == alumnoId).Select(a => a.UserId).FirstOrDefaultAsync();

                if (userId != null)
                {
                    var alumnoFcmTokens = await Db.tbUsuariosFcmTokens.Where(a => a.UserId == userId).ToListAsync();

                    alumnoFcmTokens.ForEach(a =>
                        lsAlumnosFcmTokens.Add(new UsuarioFcmToken
                        {
                            UserId = a.UserId,
                            FcmToken = a.Token,
                        })
                    );
                }
            }

            if (grupoId != -1)
            {
                descrip = "al grupo";
                nombreClase = await Db.tbGrupos.Where(a => a.GrupoId == grupoId).Select(a => a.NombreGrupo).FirstOrDefaultAsync() ?? "";

            }
            else if (materiaId != -1)
            {
                descrip = "a la materia";
                nombreClase = await Db.tbMaterias.Where(a => a.MateriaId == materiaId).Select(a => a.NombreMateria).FirstOrDefaultAsync() ?? "";
            }

            string nombreCompletoDocente = await Db.tbDocentes
                .Where(a => a.DocenteId == docenteId)
                .Select(a =>
                    (a.ApellidoPaterno ?? "") + " " +
                    (a.ApellidoMaterno ?? "") + " " +
                    (a.Nombre ?? ""))
                .FirstOrDefaultAsync() ?? "";

            ElementosNotificacion notificacion = new ElementosNotificacion()
            {
                LsUsuariosFcmTokens = lsAlumnosFcmTokens,
                Titulo = "Asignado",
                Descripcion = "El docente " + nombreCompletoDocente + " te asignó " + descrip + nombreClase
            };

            //await DetonarNotificaciones(notificacion);
        }

        public async Task NotificacionCrearActividad(tbActividades actividad)
        {
            var titulo = actividad.NombreActividad;
            var materiaId = actividad.MateriaId;

            var lsAlumnosIds = await Db.tbAlumnosMaterias.Where(a => a.MateriaId == materiaId).Select(a => a.AlumnoId).ToListAsync();

            var lsAlumnosUsersIds = await Db.tbAlumnos.Where(a => lsAlumnosIds.Contains(a.AlumnoId)).Select(a => a.UserId).ToListAsync();

            //List<string> lsFcmTokens = await Db.tbUsuariosFcmTokens.Where(a => lsAlumnosUsersIds.Contains(a.UserId)).Select(a => a.Token).ToListAsync();

            List<UsuarioFcmToken> lsUsuariosFcmTokens = await Db.tbUsuariosFcmTokens.Where(a => lsAlumnosUsersIds.Contains(a.UserId)).Select(a => new UsuarioFcmToken { UserId = a.UserId, FcmToken = a.Token }).ToListAsync();

            ElementosNotificacion notificacion = new ElementosNotificacion()
            {
                LsUsuariosFcmTokens = lsUsuariosFcmTokens,
                Titulo = "Nueva tarea: " + titulo,
                Descripcion = ""
            };

           // await DetonarNotificaciones(notificacion);
        }
        /*
        private async Task DetonarNotificaciones(ElementosNotificacion notificacion)
        {
            var lsUsuariosFcmTokens = notificacion.LsUsuariosFcmTokens;

            foreach (var usuariotoken in lsUsuariosFcmTokens)
            {
                try
                {
                    await FCM.SendNotificationAsync(
                        usuariotoken.FcmToken,
                        notificacion.Titulo,
                        notificacion.Descripcion
                    );
                }
                catch (Exception ex)
                {
                    // Ignora tokens inválidos
                    if (ex.Message.Contains("Requested entity was not found"))
                    {
                        System.Diagnostics.Debug.WriteLine($"Token inválido ignorado: {usuariotoken.FcmToken}");
                        continue; // seguir con el siguiente token
                    }

                    //Otro tipo de error 
                    throw;
                }
            }
        }

        */

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

            if (!disposed)
            {
                if (disposing)
                {
                    _db?.Dispose();
                    _db = null;
                }
                disposed = true;
            }

        }
        ~NotificacionesService()
        {
            Dispose(false);
        }
    }
}