using ControlActividades.Filters;
using ControlActividades.Models;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace ControlActividades.Controllers
{
    [Authorize(Roles = "Administrador")]
    [NoCache]
    public class AdministradorController : BaseController
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

        public ActionResult VerDocentes()
        {
            List<DocentesValidacion> lsDocentesAdministrar = new List<DocentesValidacion>();
            var lsDocentes = Db.tbDocentes.ToList();
            foreach (var d in lsDocentes)
            {
                DocentesValidacion docente = new DocentesValidacion()
                {
                    DocenteId = d.DocenteId,
                    ApellidoPaterno = d.ApellidoPaterno,
                    ApellidoMaterno = d.ApellidoMaterno,
                    Nombre = d.Nombre,
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
                    var fechaLimite = docente.FechaExpiracionCodigo;

                    if (fechaLimite < DateTime.Now || string.IsNullOrEmpty(codigoDocente))
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

                            var templatePath = HostingEnvironment.MapPath("~/Templates/Emails/CodigoDocente.html");
                            var html = System.IO.File.ReadAllText(templatePath);

                            //Se reemplaza link en el archivo html por el link real
                            html = html.Replace("{{codigoDocente}}", codigoDocente);

                            var emailService = new Services.EmailService();
                            await emailService.SendEmailAsync(
                                user.Email,
                                "Código de verificación",
                                html
                            );

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

                    if (fechaLimite < DateTime.Now || string.IsNullOrEmpty(codigoDocente))
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

                            var templatePath = HostingEnvironment.MapPath("~/Templates/Emails/CodigoDocente.html");
                            var html = System.IO.File.ReadAllText(templatePath);
                            
                            //Se reemplaza link en el archivo html por el link real
                            html = html.Replace("{{codigoDocente}}", codigoDocente);

                            var emailService = new Services.EmailService();
                            await emailService.SendEmailAsync(
                                user.Email, 
                                "Código de verificación", 
                                html
                            );

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
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> IngresarComoDocente(string userId)
        {
            //Validar que el userId no esté vacío
            if (string.IsNullOrEmpty(userId))
            {
                //MENSAJE DE ERROR
                return RedirectToAction("Index");
            }
           
            string adminId = User.Identity.GetUserId();
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Index");
            }

            // Obtener docente
            var docente = await UserManager.FindByIdAsync(userId);
            if(docente == null)
            {
                return RedirectToAction("Index");
            }

            //Verificar rol
            if(!await UserManager.IsInRoleAsync(userId, "Docente"))
            {
                return RedirectToAction("Index");
            }

            //Crear identidad del dcoente
            var identity = await UserManager.CreateIdentityAsync(
                docente,
                DefaultAuthenticationTypes.ApplicationCookie
            );

            string adminEmail = User.Identity.Name;

            //Claims de impersonación
            identity.AddClaim(new Claim("IsImpersonating", "true"));
            identity.AddClaim(new Claim("AdminId", adminId));
            identity.AddClaim(new Claim("AdminEmail", adminEmail));

            // Reemplazar cookie
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            AuthenticationManager.SignIn(
                new AuthenticationProperties
                {
                    IsPersistent = false
                },
                identity
            );

            //Redirigir al home del docente
            return RedirectToAction("Index", "Grupos");

        }


        [AllowAnonymous]
        public async Task<ActionResult> SalirImpersonacion()
        {
            var principal = (ClaimsPrincipal)User;

            if(!principal.HasClaim("IsImpersonating", "true"))
                return RedirectToAction("Index", "Home");

            var adminId = principal.FindFirst("AdminOriginalId")?.Value;
            
            if(adminId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var admin = await UserManager.FindByIdAsync(adminId);
            if(admin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            await SignInManager.SignInAsync(
                admin, isPersistent: false, rememberBrowser: false
            );

            return RedirectToAction("Index", "Administrador");
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
