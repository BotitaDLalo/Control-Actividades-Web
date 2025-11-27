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



        [HttpPost]
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


        //ENDPOINT DE PRUEBA PARA ENVIAR NOTIFICACIONES PUSH
        //PRIMERA PRUEBA
        [HttpPost]
        [Route("Test")]
        public async Task<IHttpActionResult> TestPush(TestPushRequest model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Token))
                    return BadRequest("Token requerido");

                var fcm = new FCMService();

                var ok = await fcm.SendNotificationAsync(model.Token, model.Title, model.Body);

                if (ok)
                    return Ok("Notificación enviada correctamente");
                else
                    return BadRequest("Falló el envío de la notificación");
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        public class TestPushRequest
        {
            public string Token { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
        }

        //PRUEBA EDNPOINT DE AVISOS
        [HttpPost]
        [Route("prueba-aviso")]
        public async Task<IHttpActionResult> PruebaAviso(int materiaId)
        {
            try
            {
                
                tbAvisos aviso = new tbAvisos
                {
                    DocenteId = 4, // ID de prueba
                    Titulo = "Tienes un aviso máster",
                    Descripcion = "Aviso de prueba 3 desde el backend",
                    MateriaId = materiaId,
                    GrupoId = null,
                    FechaCreacion = DateTime.Now
                };

                //  Llamada a servicio de notificaciones
                await Ns.NotificacionCrearAviso(aviso, null, materiaId);
                
                return Ok(new
                {
                    ok = true,
                    mensaje = "Notificación de aviso enviada correctamente.",
                    materiaId = materiaId
                });
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
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
