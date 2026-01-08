using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ControlActividades.Models;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace ControlActividades.Controllers
{
    public class AdministradorController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private Services.EmailService _emailService;

        #region Constantes
        public AdministradorController() { }

        public AdministradorController(ApplicationUserManager userManager, 
            ApplicationSignInManager signInManager, 
            RoleManager<IdentityRole> roleManager, 
            ApplicationDbContext DbContext, 
            FuncionalidadesGenerales fg, 
            Services.EmailService emailService)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
            EmailService = emailService;
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
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
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
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
                return _roleManager ?? HttpContext.GetOwinContext().Get<RoleManager<IdentityRole>>();
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

        public Services.EmailService EmailService
        {
            get
            {
                return _emailService ?? (_emailService = new Services.EmailService());
            }
            private set
            {
                _emailService = value;
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        #endregion


        public async Task<ActionResult> Index()
        {
            List<DocentesValidacion> lsDocentesAdministrar = new List<DocentesValidacion>();
            var lsDocentes = Db.tbDocentes.ToList();

            foreach (var d in lsDocentes)
            {
                string email = await ObtenerCorreoDocente(d.DocenteId);
                var autorizado = EstadoAutorizado(d.estaAutorizado);
                var envioCorreo = EnvioCorreo(d.seEnvioCorreo);

                DocentesValidacion docente = new DocentesValidacion()
                {
                    DocenteId = d.DocenteId,
                    ApellidoPaterno = d.ApellidoPaterno,
                    ApellidoMaterno = d.ApellidoMaterno,
                    Nombre = d.Nombre,
                    Email = email,
                    Autorizado = autorizado,
                    EnvioCorreo = envioCorreo,
                    UserId = d.UserId
                };
                lsDocentesAdministrar.Add(docente);
            }

            return View(lsDocentesAdministrar);
        }

        #region Metodos de la tabla
        private static string EstadoAutorizado(bool? status)
        {
            if (status == null)
            {
                return EstatusAutorizacion.PENDIENTE;
            }
            else
            {
                if (status.Value)
                {
                    return EstatusAutorizacion.AUTORIZADO;
                }
                else
                {
                    return EstatusAutorizacion.DENEGADO;
                }
            }
        }

        private static string EnvioCorreo(bool status)
        {
            if (status)
            {
                return EstatusEnvioCorreoDocente.ENVIADO;
            }
            else
            {
                return EstatusEnvioCorreoDocente.NO_ENVIADO;
            }
        }

        private async Task<string> ObtenerCorreoDocente(int docenteId)
        {
            var docenteUserId = Db.tbDocentes
                .Where(a => a.DocenteId == docenteId)
                .Select(a => a.UserId)
                .FirstOrDefault();

            var user = await UserManager.FindByIdAsync(docenteUserId ?? "");

            if (user != null)
            {
                var email = user.Email;
                return email ?? "";
            }
            return "";
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AutorizarDocente(int docenteId)
        {
            try
            {
                var docente = Db.tbDocentes
                    .Where(a => a.DocenteId == docenteId)
                    .FirstOrDefault();

                if (docente != null)
                {
                    var userId = docente.UserId;
                    var codigoDocente = docente.CodigoAutorizacion;

                    var user = await UserManager.FindByIdAsync(userId);

                    if (user != null)
                    {
                        try
                        {
                            var email = user.Email;

                            await EmailService.SendEmailAsync(email ?? "", "Código de verificación", codigoDocente ?? "");

                            docente.seEnvioCorreo = true;
                            Db.SaveChanges();

                            return Json(new { mensaje = "Código de verificación enviado con éxito." }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception)
                        {
                            return Json(new { mensaje = "No se pudo mandar código de verificación." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DenegarDocente(int docenteId)
        {
            try
            {
                var docente = Db.tbDocentes
                    .Where(a => a.DocenteId == docenteId)
                    .FirstOrDefault();

                if (docente != null)
                {
                    docente.estaAutorizado = false;
                    docente.seEnvioCorreo = false;
                    docente.CodigoAutorizacion = null;
                    docente.FechaExpiracionCodigo = null;

                    Db.SaveChanges();
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ReenviarCodigo(int docenteId)
        {
            try
            {
                var docente = Db.tbDocentes
                    .Where(a => a.DocenteId == docenteId)
                    .FirstOrDefault();

                if (docente != null)
                {
                    var userId = docente.UserId;
                    var codigoDocente = docente.CodigoAutorizacion;
                    var fechaLimite = docente.FechaExpiracionCodigo;

                    if (fechaLimite < DateTime.Now)
                    {
                        bool existeCodigo = false;

                        do
                        {
                            existeCodigo = Db.tbDocentes.Any(a => a.CodigoAutorizacion == codigoDocente);

                            if (existeCodigo)
                            {
                                codigoDocente = Fg.GenerarCodigoAleatorio();
                            }
                        }
                        while (existeCodigo);

                        DateTime fechaExpiracionCodigo = DateTime.UtcNow.AddMinutes(59);
                        docente.FechaExpiracionCodigo = fechaExpiracionCodigo;
                        docente.CodigoAutorizacion = codigoDocente;

                        Db.SaveChanges();
                    }

                    var user = await UserManager.FindByIdAsync(userId);

                    if (user != null)
                    {
                        try
                        {
                            var email = user.Email;

                            await EmailService.SendEmailAsync(email ?? "", "Código de verificación", codigoDocente ?? "");

                            docente.seEnvioCorreo = true;
                            Db.SaveChanges();

                            return Json(new { mensaje = "Código de verificación enviado con éxito." }, JsonRequestBehavior.AllowGet);
                        }
                        catch (Exception)
                        {
                            return Json(new { mensaje = "No se pudo mandar código de verificación." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }

                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }
        #endregion

        #region Ingreso como docente
        [HttpPost]
        //[Authorize(Roles = "Administrador")]
        public async Task<ActionResult> IngresarComoDocente(string userId)
        {
            // SOLO para prueba
            if (string.IsNullOrEmpty(userId))
            {
                //Poner mensaje de error "El docente no existe, etc"
                return RedirectToAction("Index");
            }
            /*
            //Evita impersonaciones dobles
            if (Session["IsImpersonating"] != null && Session["ImpersonatedUserId"]?.ToString() != userId)
            {
                return RedirectToAction("Index");
            }*/

            string adminId = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Index");
            }

            //Guardar la sesión del admin
            Session["AdminOriginalId"] = adminId;
            Session["IsImpersonating"] = true;
            Session["ImpersonateUserId"] = userId;

            // CERRAR SESIÓN DEL ADMINISTRADOR E INICIAR COMO DOCENTE                      
            
            // Obtener docente
            var docente = await UserManager.FindByIdAsync(userId);
            if(docente == null)
            {
                return RedirectToAction("Index");
            }

            if(!await UserManager.IsInRoleAsync(userId, "Docente"))
            {
                return RedirectToAction("Index");
            }

            //Cerrar sesión del admin
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            // Iniciar sesión como docente sin contraseña
            await SignInManager.SignInAsync(
                docente,
                isPersistent: false,
                rememberBrowser: false
            );

            //Redirigir al home del docente
            return RedirectToAction("Index", "Docente");
        }
        #endregion
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
            }

            base.Dispose(disposing);
        }
    }
}
