using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.HtmlControls;

namespace ControlActividades.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext DbContext, FuncionalidadesGenerales fg)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Fg = fg;
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

        #region Web 
        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Role role;

            //buscar registro si existe, bool
           // var engoogle = Db.UsersLogins.Where(u => u.Email == model.Email && u.LoginProvider == "Google").Any();



            var usuario = await UserManager.FindByEmailAsync(model.Email);
            if (usuario == null)
            {
                return View(model);
            }

            //usuario registrado pero sin rol
            var getRole = await UserManager.GetRolesAsync(usuario.Id);
            if (string.IsNullOrEmpty(getRole.FirstOrDefault()) || !Enum.IsDefined(typeof(Role), getRole.FirstOrDefault()))
            {
                return View(model);
            }

            role = (Role)Enum.Parse(typeof(Role), getRole.FirstOrDefault());

            //if (role == Role.Docente && !usuario.EmailConfirmed)
            if (role == Role.Docente && !usuario.EmailConfirmed)
            {
                Session[SessionKeys.Email.ToString()] = model.Email;
                return RedirectToAction("ConfirmEmail");
            }

            // No cuenta los errores de inicio de sesión para el bloqueo de la cuenta
            // Para permitir que los errores de contraseña desencadenen el bloqueo de la cuenta, cambie a shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl, role);
                //case SignInStatus.LockedOut:
                //    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Intento de inicio de sesión no válido.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult ValidateEmail()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ValidateEmail(ValidateEmailViewModel model)
        {
            try
            {
                string email = model.Email;

                var emailEsValido = await UserManager.FindByEmailAsync(email);

                if (emailEsValido == null)
                {
                    Session[SessionKeys.Email.ToString()] = email;
                    return RedirectToAction("Register");
                }
                ModelState.AddModelError("", "");
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "No se pudo validar el correo.");
                return View();
            }
        }


        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Requerir que el usuario haya iniciado sesión con nombre de usuario y contraseña o inicio de sesión externo
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // El código siguiente protege de los ataques por fuerza bruta a los códigos de dos factores. 
            // Si un usuario introduce códigos incorrectos durante un intervalo especificado de tiempo, la cuenta del usuario 
            // se bloqueará durante un período de tiempo especificado. 
            // Puede configurar el bloqueo de la cuenta en IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                //return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Código no válido.");
                    return View(model);
            }
        }





        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string email = Session["Email"] as string ?? "";

                    if (email == "")
                    {
                        return View();
                    }

                    var nombre = model.Nombres;
                    var apellidoPaterno = model.ApellidoPaterno;
                    var apellidoMaterno = model.ApellidoMaterno;
                    var password = model.Password;
                    var role = model.Role;


                    var user = new ApplicationUser { UserName = email, Email = email };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        string roleStr = model.Role.ToString();

                        //Si el rol no existe, este es creado
                        if (!await RoleManager.RoleExistsAsync(roleStr))
                            await RoleManager.CreateAsync(new IdentityRole(roleStr));

                        var userFound = UserManager.FindByEmail(email);

                        var rolAsignado = await UserManager.AddToRoleAsync(user.Id, roleStr);

                        if (!rolAsignado.Succeeded)
                        {
                            return View();
                        }


                        string controller;
                        switch (role)
                        {
                            case Role.Docente:
                                controller = Role.Docente.ToString();
                                DateTime fechaExpiracionCodigo = DateTime.UtcNow.AddMinutes(59);
                                string codigo = Fg.GenerarCodigoAleatorio();

                                tbDocentes docente = new tbDocentes()
                                {
                                    ApellidoPaterno = apellidoPaterno,
                                    ApellidoMaterno = apellidoMaterno,
                                    Nombre = nombre,
                                    UserId = userFound.Id,
                                    CodigoAutorizacion = codigo,
                                    FechaExpiracionCodigo = fechaExpiracionCodigo,
                                };

                                Db.tbDocentes.Add(docente);

                                user.LockoutEndDateUtc = DateTime.MaxValue;
                                await UserManager.UpdateAsync(user);
                                await Db.SaveChangesAsync();

                                return RedirectToAction("ConfirmEmail");

                            case Role.Alumno:
                                controller = Role.Alumno.ToString();
                                tbAlumnos alumno = new tbAlumnos()
                                {
                                    ApellidoPaterno = apellidoPaterno,
                                    ApellidoMaterno = apellidoPaterno,
                                    Nombre = nombre,
                                    UserId = user.Id,
                                };

                                Db.tbAlumnos.Add(alumno);
                                await Db.SaveChangesAsync();
                                break;

                            default:
                                return View(model);
                        }

                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                        // Enviar un correo electrónico con este vínculo
                        // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        //await UserManager.SendEmailAsync(user.Id, "Confirmar la cuenta", "Para confirmar su cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

                        return RedirectToAction("Index", controller);
                    }
                    AddErrors(result);
                }
                return View(model);
            }
            catch (Exception)
            {
                // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
                return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult ConfirmEmail()
        {
            return View();
        }

        //
        // POST: /Account/ConfirmEmail
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ConfirmEmail(ConfirmEmailViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View("Error");
                }
                string email = Session[SessionKeys.Email.ToString()].ToString();
                string code = model.Code1 + model.Code2 + model.Code3 + model.Code4 + model.Code5;

                var user = UserManager.FindByEmail(email);

                var docente = Db.tbDocentes.Where(a => a.UserId == user.Id).FirstOrDefault();
                if (docente != null)
                {
                    int docenteId = docente.DocenteId;

                    if (code != docente.CodigoAutorizacion)
                    {
                        return View();
                    }

                    if (docente.FechaExpiracionCodigo < DateTime.Now)
                    {
                        return View();
                    }

                    docente.FechaExpiracionCodigo = null;
                    docente.CodigoAutorizacion = null;
                    //docente.estaAutorizado = true;

                    user.LockoutEnabled = false;
                    user.LockoutEndDateUtc = null;
                    user.EmailConfirmed = true;
                    await UserManager.UpdateAsync(user);
                    await Db.SaveChangesAsync();


                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    return RedirectToAction("Index", "Docente");
                }

                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // No revelar que el usuario no existe o que no está confirmado
                    return View("ForgotPasswordConfirmation");
                }

                // Para obtener más información sobre cómo habilitar la confirmación de cuentas y el restablecimiento de contraseña, visite https://go.microsoft.com/fwlink/?LinkID=320771
                var token = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                
                var resetlink = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { userId = user.Id,
                        code = token },
                    protocol: Request.Url.Scheme
                );

                //PLANTILLA HTML
                var templatePath = HostingEnvironment.MapPath("~/Templates/Emails/ResetPassword.html");
                var html = System.IO.File.ReadAllText(templatePath);

                //Se reemplaza link en el archivo html por el link real
                html = html.Replace("{{link}}", resetlink);

                //Enviar correo
                var emailService = new Services.EmailService();
                await emailService.SendEmailAsync(
                    user.Email, 
                    "Restablecer contraseña",
                    html    
                );

                // *usando identity* await UserManager.SendEmailAsync(user.Id, "Restablecer contraseña", "Para restablecer la contraseña, haga clic <a href=\"" + resetlink + "\">aquí</a>");

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error");
            }

            var model = new ResetPasswordViewModel
            {
                Email = user.Email,
                Code = code
            };

            return View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // 
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Solicitar redireccionamiento al proveedor de inicio de sesión externo
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generar el token y enviarlo
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Si el usuario ya tiene un inicio de sesión, iniciar sesión del usuario con este proveedor de inicio de sesión externo
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);

            switch (result)
            {
                case SignInStatus.Success:
                    
                    var usuario = await UserManager.FindByEmailAsync(loginInfo.Email);
                    if (usuario == null)
                    {
                        return RedirectToAction("Login");
                    }

                    var getRole = await UserManager.GetRolesAsync(usuario.Id);
                    if (string.IsNullOrEmpty(getRole.FirstOrDefault()) || !Enum.IsDefined(typeof(Role), getRole.FirstOrDefault()))
                    {
                        return View("ExternalLoginFailure");
                    }

                    var role = (Role)Enum.Parse(typeof(Role), getRole.FirstOrDefault());
                    return RedirectToLocal(returnUrl, role);
                    


                case SignInStatus.LockedOut:
                    return View("Lockout");

                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                
                case SignInStatus.Failure:
                default:

                    var email = loginInfo.Email;

                    var existingUser = await UserManager.FindByEmailAsync(email);
                    // ya hay cuenta pero sin vincular a google
                    if(existingUser != null)
                    {
                        ViewBag.ReturnUrl = returnUrl;
                        ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                        return View("ExternalLoginLink", new ExternalLoginLinkViewModel { Email = email, LoginProvider = loginInfo.Login.LoginProvider });
                    }

                    // Si el usuario no tiene ninguna cuenta, solicitar que cree una
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginLink
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginLink(ExternalLoginLinkViewModel model, string link, string returnUrl)
        {
            if (link == "no")
            {
                return RedirectToAction("Login");
            }

            var info = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return View("ExternalLoginFailure");
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var addLoginResult = await UserManager.AddLoginAsync(user.Id, info.Login);
            if (addLoginResult.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                var existingRole = (await UserManager.GetRolesAsync(user.Id)).FirstOrDefault();
                return RedirectToLocal(returnUrl, (Role)Enum.Parse(typeof(Role), existingRole));
            }

            AddErrors(addLoginResult);
            return View("ExternalLoginFailure");
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            /*if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }   
            */
            if (ModelState.IsValid)
            {
                // Obtener datos del usuario del proveedor de inicio de sesión externo
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                // Crear nuevo usuario en ApplicationUser
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //Crear rol
                    var rolstring = model.Role.ToString();
                    if (!await RoleManager.RoleExistsAsync(rolstring))
                    {
                        await RoleManager.CreateAsync(new IdentityRole(rolstring));
                    }
                    await UserManager.AddToRoleAsync(user.Id, rolstring);

                    // Crear usuario según rol
                    switch (model.Role)
                    {
                        case Role.Docente:
                            DateTime fechaExpiracionCodigo = DateTime.UtcNow.AddMinutes(59);
                            string codigo = Fg.GenerarCodigoAleatorio();

                            Db.tbDocentes.Add(new tbDocentes
                            {
                                ApellidoPaterno = model.Paterno,
                                ApellidoMaterno = model.Materno,
                                Nombre = model.Nombre,
                                UserId = user.Id,
                                CodigoAutorizacion = codigo,
                                FechaExpiracionCodigo = fechaExpiracionCodigo,
                            });

                            user.LockoutEndDateUtc = DateTime.MaxValue;
                            await UserManager.UpdateAsync(user);
                            await Db.SaveChangesAsync();
                            break;

                        case Role.Alumno:
                            Db.tbAlumnos.Add(new tbAlumnos
                            {
                                ApellidoPaterno = model.Paterno,
                                ApellidoMaterno = model.Materno,
                                Nombre = model.Nombre,
                                UserId = user.Id
                            });
                            await Db.SaveChangesAsync();
                            break;
                    }

                    // Asociar login externo
                    var addLoginResult = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (addLoginResult.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl, (Role)Enum.Parse(typeof(Role), rolstring));
                    }

                    AddErrors(addLoginResult);
                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }
                AddErrors(result);
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
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
            }

            base.Dispose(disposing);
        }

        #region Aplicaciones auxiliares
        // Se usa para la protección XSRF al agregar inicios de sesión externos
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl, Role role)
        {
           /* if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
           */

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/")
            {
                return Redirect(returnUrl);
            }

            switch (role)
            {
                case Role.Docente:
                    return RedirectToAction("Index", "Docente");
                case Role.Alumno:
                    return RedirectToAction("Index", "Alumno");
                case Role.Administrador:
                    return RedirectToAction("Index", "Administrador");
                default:
                    return RedirectToAction("Login", "Account");
            }

        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        #endregion

    }
}