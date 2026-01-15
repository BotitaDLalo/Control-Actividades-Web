using FirebaseAdmin.Messaging;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services.Description;
using ControlActividades.Controllers;
using ControlActividades.Models;
using ControlActividades.Models.db;
using static Google.Apis.Requests.RequestError;
using Microsoft.AspNet.SignalR;

namespace ControlActividades.Services
{
    public class NotificacionesService
    {
        private ApplicationDbContext _db;
        private FCMService _fCMService;
        private bool disposed = false;

        #region
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
        #endregion

        #region Creación general de notificaciones
        //REGISTRO DE TOKENS Y MÉTODOS PARA EL ENVÍO DE NOTIFICACIONES
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


        public async Task ColaDeNotificaciones(string userId)
        {
            const int maxNotificaciones = 20;   //modificar también en headerNotifications.js para eliminado en DOM en tiempo real
            // Verificar el número de notificaciones existentes para el usuario
            var notificaciones = await _db.tbNotificaciones
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.FechaRecibido)
                .ToListAsync();

            if (notificaciones.Count <= maxNotificaciones)
            {
                return;
            }

            var notificacionesAEliminar = notificaciones
                .Skip(maxNotificaciones)
                .ToList();
            _db.tbNotificaciones.RemoveRange(notificacionesAEliminar);
            await _db.SaveChangesAsync();
        }

        public async Task GuardarNotificacionAsync(string userId, string messageId,
                                                   string title, string body,
                                                   TiposNotificaciones tipo,
                                                   int? materiaId = null, int? grupoId = null)
        {
            try
            {

                var noti = new tbNotificaciones
                {
                    UserId = userId,
                    MessageId = messageId,
                    Title = title,
                    Body = body,
                    FechaRecibido = DateTime.Now,
                    TipoId = (int)tipo,
                    MateriaId = materiaId,
                    GrupoId = grupoId
                };

                _db.tbNotificaciones.Add(noti);
                await _db.SaveChangesAsync();

                //Meter la notificación en cola
                await ColaDeNotificaciones(userId);

                //Enviar notificación en tiempo real
                EnviaNotificacionTiempoReal(userId, noti, tipo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al guardar notificación: " + ex.Message);
            }
            
        }

        public void EnviaNotificacionTiempoReal(string userId, tbNotificaciones notificacion, TiposNotificaciones tipo) { 
        
            var hub = GlobalHost.ConnectionManager.GetHubContext<NotificacionesHub>();

            hub.Clients.User(userId).nuevaNotificacion(new
            {
                NotificacionId = notificacion.NotificacionId,
                Title = notificacion.Title,
                Body = notificacion.Body,
                Tipo = tipo,
                FechaRecibido = notificacion.FechaRecibido.ToString("O")
            });
        }


        #endregion

        #region Notificaciones particulares
        //OBTENER TOKENS DE LOS DESTINATARIOS
        private async Task<(List<string> usuarios, List<UsuarioFcmToken> tokens)> ObtenerDestinatarios(int? grupoId, int? materiaId)
        {
            List<int> alumnosId = new List<int>();

            if (grupoId != null)
            {
                alumnosId = await Db.tbAlumnosGrupos
                    .Where(a => a.GrupoId == grupoId)
                    .Select(a => a.AlumnoId)
                    .ToListAsync();
            }
            else if (materiaId != null)
            {
                alumnosId = await Db.tbAlumnosMaterias
                    .Where(a => a.MateriaId == materiaId)
                    .Select(a => a.AlumnoId)
                    .ToListAsync();
            }

            var usuariosIds = await Db.tbAlumnos
                .Where(a => alumnosId.Contains(a.AlumnoId))
                .Select(a => a.UserId)
                .ToListAsync();

            var tokens = await Db.tbUsuariosFcmTokens
                .Where(a => usuariosIds.Contains(a.UserId))
                .Select(a => new UsuarioFcmToken { FcmToken = a.Token, UserId = a.UserId })
                .ToListAsync();

            return (usuariosIds, tokens);
        }


        //NOTIFICACIÓN GENERAL PARA TODAS LAS ACCIONES
        public async Task ProcesarNotificacion(List<string> destinatariosUserId,
                                               List<UsuarioFcmToken> tokens, string titulo, string cuerpo, TiposNotificaciones tipo,
                                               int? materiaId = null, int? grupoId = null
                                               )
        {

            //Enviar tokens FCM
            await FCM.SendBatchNotificationsAsync(tokens.Select(t => t.FcmToken).ToList(),
                                                  titulo,
                                                  cuerpo
            );

            var messageId = Guid.NewGuid().ToString();

            //Guardar una notificación por usuario
            foreach (var userId in destinatariosUserId)
            {
                await GuardarNotificacionAsync(userId, messageId, titulo, cuerpo, tipo, materiaId);
            }

        }
 
        //TIPOS DE NOTIFICACIONES
        
        //SECCIÓN DE NOTIFICACIONES PARA -ALUMNOS- CUANDO EL DOCENTE HACE UNA ACCIÓN

        // Notificación cuando el docente crea una actividad
        public async Task NotificacionCrearActividad(tbActividades actividad, int materiaId)
        {
            var (usuariosIds, tokens) = await ObtenerDestinatarios(null, materiaId);

            await ProcesarNotificacion(
                usuariosIds,
                tokens,
                actividad.NombreActividad,
                actividad.Descripcion,
                TiposNotificaciones.ActividadCreada,
                materiaId
            );
        }

        // Notificación cuando el docente crea un aviso
        public async Task NotificacionCrearAviso(tbAvisos aviso, int? grupoId, int? materiaId)
        {
            var (usuariosIds, tokens) = await ObtenerDestinatarios(grupoId, materiaId);

            await ProcesarNotificacion(
                usuariosIds,
                tokens,
                aviso.Titulo,
                aviso.Descripcion,
                TiposNotificaciones.Aviso,
                materiaId
            );


        }

        
        // Notificación cuando el docente registra un alumno(s)
        public async Task NotificacionRegistrarAlumnoClase(List<int> lsAlumnosId, int docenteId, int grupoId = -1, int materiaId = -1)
        {
            if(lsAlumnosId == null || !lsAlumnosId.Any()) {
                return;
            }

            // Obtener UserIds y tokens de los alumnos
            var usuariosIds = await _db.tbAlumnos
               .Where(a => lsAlumnosId.Contains(a.AlumnoId))
               .Select(a => a.UserId)
               .ToListAsync();

                    if (!usuariosIds.Any())
                        return;

            // Obtener tokens FCM
            var tokens = await _db.tbUsuariosFcmTokens
                .Where(t => usuariosIds.Contains(t.UserId))
                .Select(t => new UsuarioFcmToken
                {
                    UserId = t.UserId,
                    FcmToken = t.Token
                })
                .ToListAsync();

            //Texto
            string titulo;
            string cuerpo;
            TiposNotificaciones tipo;

            if (materiaId > 0)
            {
                var materia = await _db.tbMaterias
                    .Where(m => m.MateriaId == materiaId)
                    .Select(m => m.NombreMateria)
                    .FirstOrDefaultAsync();

                titulo = "Nueva materia asignada";
                cuerpo = $"Has sido asignado a la materia {materia}";
                tipo = TiposNotificaciones.MateriaAsignada;
            }
            else
            {
                var grupo = await _db.tbGrupos
                    .Where(g => g.GrupoId == grupoId)
                    .Select(g => g.NombreGrupo)
                    .FirstOrDefaultAsync();

                titulo = "Nuevo grupo asignado";
                cuerpo = $"Has sido asignado al grupo {grupo}";
                tipo = TiposNotificaciones.GrupoAsignado;
            }

            int? materiaIdNullable = materiaId > 0 ? materiaId : (int?)null;
            int? grupoIdNullable = grupoId > 0 ? grupoId : (int?)null;

            await ProcesarNotificacion(
                usuariosIds,
                tokens,
                titulo,
                cuerpo,
                tipo,
                materiaIdNullable,
                grupoIdNullable
            );


        }

        // Notificación cuando el docente crea un evento
        public async Task NotificacionCrearEvento(tbEventosAgenda evento, int? grupoId, int? materiaId)
        {
            try
            {
                //Obtener destinatarios dependiendo del grupo o materia
                var (usuariosIds, tokens) = await ObtenerDestinatarios(grupoId, materiaId);

                if(usuariosIds == null || usuariosIds.Count == 0) {
                    return; 
                }

                string messageId = Guid.NewGuid().ToString();

                string titulo = evento.Titulo;
                string descripcion = evento.Descripcion;

                await ProcesarNotificacion(
                    usuariosIds,
                    tokens,
                    titulo,
                    descripcion,
                    TiposNotificaciones.Evento
                );

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al enviar notificación de creación de evento: " + ex.Message);
            }
            
        }

        // Notificación cuando el docente califica una tarea


        //SECCIÓN DE NOTIFICACIONES PARA -DOCENTES- CUANDO EL ALUMNO HACE UNA ACCIÓN

        // Notificación cuando el alumno sube su tarea

        // Notificación cuando el alumno deja un comentario (posible implementación)


        #endregion

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