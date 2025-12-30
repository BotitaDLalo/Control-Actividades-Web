using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/Notificaciones")]
    public class NotificacionesApiController : ApiController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private NotificacionesService _notifServ;
        public NotificacionesApiController()
        {
            _notifServ = new NotificacionesService(new ApplicationDbContext());
        }
        public NotificacionesApiController(ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext DbContext,
            FuncionalidadesGenerales fg,
            NotificacionesService notificacionesService
            )
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
            Ns = notificacionesService;
        }
        #region Propiedades
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
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

        public RoleManager<IdentityRole> RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.Current.GetOwinContext().Get<RoleManager<IdentityRole>>();
            }
            private set
            {
                _roleManager = value;
            }
        }

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

        public FuncionalidadesGenerales Fg
        {
            get
            {
                return _fg ?? (_fg = new FuncionalidadesGenerales());
            }
            private set
            {
                _fg = value;
            }
        }

        public NotificacionesService Ns
        {
            get
            {
                return _notifServ ?? (_notifServ = new NotificacionesService(_db));
            }
            private set
            {
                _notifServ = value;
            }
        }
        #endregion

        [HttpPost]
        [Route("RegistrarToken")]
        [Authorize]
        public async Task<IHttpActionResult> RegistrarTokenDispositivo([FromBody] TokenDispositivo tokenDispositivo)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return BadRequest("Usuario no encontrado");
            }
            if(tokenDispositivo == null || string.IsNullOrEmpty(tokenDispositivo.Token))
            {
                return BadRequest("Token inválido");
            }
            
            //Token Duplicado
            var tokenExistente = Db.tbUsuariosFcmTokens
                .FirstOrDefault(t => t.UserId == userId && t.Token == tokenDispositivo.Token);
            
            if (tokenExistente != null)
            {
                return Ok("Token ya registrado");
            }

            //Registrar nuevo token
            tbUsuariosFcmTokens nuevoToken = new tbUsuariosFcmTokens
            {
                UserId = userId,
                Token = tokenDispositivo.Token
            };
            Db.tbUsuariosFcmTokens.Add(nuevoToken);
            await Db.SaveChangesAsync();

            return Ok("Token registrado correctamente");
        }


        [HttpPost]
        [Authorize]
        [Route("RegistrarNotificacion")]
        public async Task<IHttpActionResult> RegistrarNotificacionRecibida([FromBody] Notificacion notificacion)
        {
            tbNotificaciones nuevaNotificacion = new tbNotificaciones
            {
                UserId = notificacion.UserId,
                MessageId = notificacion.MessageId,
                Title = notificacion.Title,
                Body = notificacion.Body,
                FechaRecibido = notificacion.FechaRecibido
            };

            Db.tbNotificaciones.Add(nuevaNotificacion);
            await Db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Authorize]
        [Route("ObtenerNotificaciones")]
        public IHttpActionResult ObtenerNotificacionesUsuario()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return BadRequest("Usuario no encontrado");
            }
            
            var notificaciones = Db.tbNotificaciones
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.FechaRecibido)
                .Select(n => new Notificacion
                {
                    NotificacionId = n.NotificacionId,
                    UserId = n.UserId,
                    MessageId = n.MessageId,
                    Title = n.Title,
                    Body = n.Body,
                    TipoId = n.TipoId,
                    TipoNotificacion = n.cTipoNotificacion.Nombre,
                    FechaRecibido = n.FechaRecibido
                })
                .ToList();
            return Ok(notificaciones);
        }


        //[Authorize]
        [HttpDelete]
        [Route("EliminarNotificacion/{id}")]
        public async Task<IHttpActionResult> EliminarNotificacion(int id)
        {
            var noti = await Db.tbNotificaciones.FindAsync(id);
            if (noti == null) {
                return NotFound();
            }

            Db.tbNotificaciones.Remove(noti);
            await Db.SaveChangesAsync();

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }

                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }

                if (_notifServ != null)
                {
                    _notifServ.Dispose();
                    _notifServ = null;
                }
            }

            base.Dispose(disposing);
        }

    }
}
