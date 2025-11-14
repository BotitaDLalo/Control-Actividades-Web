using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using ControlActividades.Models;
using ControlActividades.Models.db;
using ControlActividades.Recursos;
using ControlActividades.Services;
using Google.Apis.Auth;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.IdentityModel.Tokens;

namespace ControlActividades.Controllers
{
    [RoutePrefix("api/Login")]
    public class AccountApiController : ApiController
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private ApplicationDbContext _db;
        private FuncionalidadesGenerales _fg;
        private NotificacionesService _notifServ;

        public AccountApiController()
        {
        }

        public AccountApiController(ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext DbContext,
            NotificacionesService notifServ
            )
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;
            Db = DbContext;
            Ns = notifServ;
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
                _fg = new FuncionalidadesGenerales();
            }
        }

        public NotificacionesService Ns
        {
            get
            {
                return _notifServ ?? (_notifServ = new NotificacionesService(Db,new FCMService()));
            }
            private set
            {
                _notifServ = value;
            }
        }

        #region Login
        [HttpPost]
        [Route("VerificarTokenFcm")]
        public async Task<IHttpActionResult> VerificarTokenFcm(int id, string fcmToken, string role)
        {
            try
            {
                //bool existeToken = await Db.tbAlumnosTokens.AnyAsync(a => a.Token == fcmToken);

                bool existeToken = Db.tbUsuariosFcmTokens.Any(a => a.Token == fcmToken);
                if (existeToken)
                {
                    return Ok(new { Mensaje = $"El token del alumno con Id ${id} existe" });
                }
                else
                {
                    var identityUserId = "";

                    if (role == Role.Docente.ToString())
                    {
                        identityUserId = Db.tbDocentes.Where(a => a.DocenteId == id).Select(a => a.UserId).FirstOrDefault();
                    }
                    else if (role == Role.Alumno.ToString())
                    {
                        identityUserId = Db.tbAlumnos.Where(a => a.AlumnoId == id).Select(a => a.UserId).FirstOrDefault();
                    }

                    if (identityUserId == null) return Content(HttpStatusCode.BadRequest, new { });

                    await Ns.RegistrarFcmTokenUsuario(identityUserId, fcmToken);

                    return Ok(new { Mensaje = $"El token del usuario con Id ${id} existe" });
                }

            }
            catch (Exception)
            {
                //return Content(HttpStatusCode.BadRequest(new { Mensaje = "No se pudo verificar el token." });
                return Content(HttpStatusCode.BadRequest, new { Mensaje = "No se pudo verificar el token." });
            }
        }

        [HttpPost]
        [Route("RegistroUsuario")]
        public async Task<IHttpActionResult> RegistroUsuario([FromBody] UsuarioRegistro model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var nombreUsuario = model.Correo;
                    var nombreUsuarioEncontrado = await UserManager.FindByNameAsync(nombreUsuario);

                    if (nombreUsuarioEncontrado != null)
                    {
                        return Content(HttpStatusCode.BadRequest, new
                        {
                            ErrorCode = ErrorCodigos.nombreUsuarioUsado,
                            ErrorMessage = Errores.GetMensajeError(ErrorCodigos.nombreUsuarioUsado)
                        });
                    }


                    var userName = model.NombreUsuario;
                    var email = model.Correo;
                    var rol = model.TipoUsuario;

                    var user = new ApplicationUser { UserName = email, Email = email };
                    var result = await UserManager.CreateAsync(user, model.Clave);
                    if (!result.Succeeded)
                    {
                        return Content(HttpStatusCode.BadRequest, new { });
                    }

                    if (!await RoleManager.RoleExistsAsync(model.TipoUsuario.ToString()))
                    {
                        await RoleManager.CreateAsync(new IdentityRole(model.TipoUsuario.ToString()));
                    }

                    var userFound = UserManager.FindByEmail(email);
                    var asignarRol = await UserManager.AddToRoleAsync(user.Id, rol.ToString());
                    if (!asignarRol.Succeeded)
                    {
                        return Content(HttpStatusCode.BadRequest, new { });
                    }


                    var fcmToken = model.FcmToken;

                    if (fcmToken == null) return Content(HttpStatusCode.BadRequest, new { });
                    switch (rol)
                    {
                        case Role.Docente:

                            DateTime fechaExpiracionCodigo = DateTime.UtcNow.AddMinutes(59);
                            string codigo = Fg.GenerarCodigoAleatorio();


                            tbDocentes docentes = new tbDocentes()
                            {
                                ApellidoPaterno = model.ApellidoPaterno,
                                ApellidoMaterno = model.ApellidoMaterno,
                                Nombre = model.Nombre,
                                UserId = userFound.Id,
                                CodigoAutorizacion = codigo,
                                FechaExpiracionCodigo = fechaExpiracionCodigo,
                            };
                            Db.tbDocentes.Add(docentes);
                            await Db.SaveChangesAsync();

                            await Ns.RegistrarFcmTokenUsuario(userFound.Id, fcmToken);

                            return Ok(new AutenticacionRespuesta
                            {
                                EstaAutorizado = EstatusAutorizacion.PENDIENTE
                            });


                        case Role.Alumno:
                            tbAlumnos alumnos = new tbAlumnos()
                            {
                                ApellidoPaterno = model.ApellidoPaterno,
                                ApellidoMaterno = model.ApellidoMaterno,
                                Nombre = model.Nombre,
                                UserId = userFound.Id,
                            };

                            Db.tbAlumnos.Add(alumnos);
                            await Db.SaveChangesAsync();

                            await Ns.RegistrarFcmTokenUsuario(userFound.Id, fcmToken);

                            //var userFound = await UserManager.FindByIdAsync(identityUserId);

                            //if (userFound == null) return Content(HttpStatusCode.BadRequest();


                            var tokenString = Fg.GenerarJwt(alumnos.AlumnoId, userFound, rol.ToString());

                            return Ok(new AutenticacionRespuesta
                            {
                                Id = alumnos.AlumnoId,
                                UserName = userName,
                                Correo = email,
                                Rol = rol.ToString(),
                                Token = tokenString,
                                EstaAutorizado = EstatusAutorizacion.AUTORIZADO
                            });

                        default:
                            return Ok();
                    }


                }
                return Content(HttpStatusCode.BadRequest, new { Mensaje = "Hubo un error en el registro" });

            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { e.Message });

            }

        }


        [HttpPost]
        [Route("InicioSesionUsuario")]
        public async Task<IHttpActionResult> InicioSesionUsuario([FromBody]UsuarioInicioSesion model)
        {
            try
            {
                //Verificar si existe el usuario
                var userFound = await UserManager.FindByEmailAsync(model.Correo);
                if (userFound == null)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        ErrorCode = ErrorCodigos.CredencialesInvalidas,
                        ErrorMessage = Errores.GetMensajeError(ErrorCodigos.CredencialesInvalidas)
                    });
                }

                //Verificar password
                var isPasswordValid = await UserManager.CheckPasswordAsync(userFound, model.Clave);
                if (!isPasswordValid)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        ErrorCode = ErrorCodigos.CredencialesInvalidas,
                        ErrorMessage = Errores.GetMensajeError(ErrorCodigos.CredencialesInvalidas)
                    });
                }


                //Obteniendo rol del usuario
                var rol = await UserManager.GetRolesAsync(userFound.Id);
                var rolUsuario = rol.FirstOrDefault() ?? throw new Exception("El usuario no posee un rol asignado");

                int idUsuario = 0;


                if (rolUsuario == "Docente")
                {

                    //idUsuario = Db.tbDocentes.Where(a => a.UserId == identityUserId).Select(a => a.DocenteId).FirstOrDefault();
                    var docente = Db.tbDocentes.Where(a => a.UserId == userFound.Id).FirstOrDefault();
                    if (docente != null)
                    {
                        if (docente.estaAutorizado == null)
                        {
                            return Ok(new AutenticacionRespuesta
                            {
                                EstaAutorizado = EstatusAutorizacion.PENDIENTE
                            });
                        }
                        else
                        {
                            if (!docente.estaAutorizado.Value)
                            {
                                return Ok(new AutenticacionRespuesta
                                {
                                    EstaAutorizado = EstatusAutorizacion.DENEGADO
                                });
                            }
                            else
                            {
                                idUsuario = docente.DocenteId;
                            }
                        }
                    }
                }
                else if (rolUsuario == "Alumno")
                {
                    idUsuario = Db.tbAlumnos.Where(a => a.UserId == userFound.Id).Select(a => a.AlumnoId).FirstOrDefault();
                }

                var tokenString = Fg.GenerarJwt(idUsuario, userFound, rolUsuario);


                return Ok(new AutenticacionRespuesta
                {
                    Id = idUsuario,
                    UserName = userFound.UserName,
                    Correo = userFound.Email,
                    Rol = rolUsuario,
                    Token = tokenString,
                    EstaAutorizado = EstatusAutorizacion.AUTORIZADO
                });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { e.Message });
            }

        }


        [HttpPost]
        [Route("ValidarCodigoDocente")]
        public async Task<IHttpActionResult> ValidarCodigoDocente([FromBody] ValidarCodigoDocente datos)
        {
            try
            {
                string email = datos.Email;
                string codigoValidar = datos.CodigoValidar;

                var userFound = await UserManager.FindByEmailAsync(email);
                if (userFound == null)
                {
                    return Content(HttpStatusCode.BadRequest, new
                    {
                        ErrorCode = ErrorCodigos.CredencialesInvalidas,
                        ErrorMessage = Errores.GetMensajeError(ErrorCodigos.CredencialesInvalidas)
                    });
                }

                //Obteniendo rol del usuario
                var rol = await UserManager.GetRolesAsync(userFound.Id);
                var rolUsuario = rol.FirstOrDefault() ?? throw new Exception("El usuario no posee un rol asignado");

                var identityUserId = userFound.Id;

                var docente = Db.tbDocentes.Where(a => a.UserId == identityUserId).FirstOrDefault();
                if (docente != null)
                {
                    int idUsuario = docente.DocenteId;

                    if (codigoValidar != docente.CodigoAutorizacion)
                    {
                        return Content(HttpStatusCode.BadRequest, new { ErrorCode = ErrorCodigos.codigoAutorizacionInvalido });
                    }
                    //5:10                              5:15
                    if (docente.FechaExpiracionCodigo < DateTime.Now)
                    {
                        return Content(HttpStatusCode.BadRequest, new { ErrorCode = ErrorCodigos.codigoAutorizacionExpirado });
                    }

                    docente.FechaExpiracionCodigo = null;
                    docente.CodigoAutorizacion = null;
                    docente.estaAutorizado = true;
                    await Db.SaveChangesAsync();

                    var tokenString = Fg.GenerarJwt(idUsuario, userFound, rolUsuario);
                    return Ok(new
                    {
                        Id = idUsuario,
                        UserName = userFound.UserName,
                        Correo = userFound.Email,
                        Rol = rolUsuario,
                        Token = tokenString,
                        EstaAutorizado = EstatusAutorizacion.AUTORIZADO,
                    });
                }

                return Content(HttpStatusCode.BadRequest, new { mensaje = "El docente no existe." });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { e.Message });
            }
        }

        [HttpGet]
        [Route("VerificarToken")]
        public IHttpActionResult VerificarToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = "El token es requerido" });
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();

                var confSecretKey = "Token para verificar autenticacion del usuario";
                var jwt = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(confSecretKey ?? throw new ArgumentNullException(confSecretKey, "Token no configurado")));
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwt,
                    ValidateLifetime = true,
                    ValidIssuer = "Aprende_Mas",
                    ValidAudience = "Aprende_Mas",
                    ClockSkew = TimeSpan.Zero
                };

                var claimsPrincipal = handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                var idUsuario = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";
                var userName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value ?? "No existe nombre";
                var correo = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value ?? "No existe correo";
                var rol = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value ?? "No existe rol";

                return Ok(new AutenticacionRespuesta
                {
                    Id = int.Parse(idUsuario),
                    UserName = userName,
                    Correo = correo,
                    Rol = rol,
                    Token = token
                });
            }
            catch (SecurityTokenExpiredException)
            {
                return Content(HttpStatusCode.Unauthorized, new { mensaje = "El token ha expirado" });
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.Unauthorized, new { mensaje = "El token es inválido" });
            }

        }

        [HttpPost]
        [Route("VerificarEmailUsuario")]
        public async Task<IHttpActionResult> VerificarEmailUsuario([FromBody] string email)
        {
            try
            {
                var emailEsValido = await UserManager.FindByEmailAsync(email);

                if (emailEsValido == null)
                {
                    // Aquí en Framework 4.8 no tienes HttpContext.Session como en Core
                    // Si necesitas session, deberías usar HttpContext.Current.Session
                    // HttpContext.Current.Session["Email"] = email;

                    return Ok();
                }

                var codigoError = ErrorCodigos.CorreoUsuarioExistente;

                var problemDetails = new ProblemDetails
                {
                    Status = (int)HttpStatusCode.BadRequest,
                    Title = "Error de validación",
                    Detail = "El correo ya está registrado.",
                    Instance = Request.RequestUri.AbsolutePath
                };

                problemDetails.Extensions.Add("errorMessage", Errores.GetMensajeError(codigoError));
                problemDetails.Extensions.Add("errorComment", "¿Desea iniciar sesión?");
                problemDetails.Extensions.Add("errorCode", codigoError);

                return Content(HttpStatusCode.BadRequest, problemDetails);
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.BadRequest, new { error = "Ocurrió un error inesperado." });
            }
        }
        #endregion



        #region Login Google
        [HttpPost]
        [Route("IniciarSesionGoogle")]
        public async Task<IHttpActionResult> IniciarSesionGoogle([FromBody] RegistrarUsuarioGoogle usuario)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(usuario.IdToken);

                if (payload == null)
                {
                    return BadRequest("idToken inválido");
                }

                var userName = (payload.FamilyName ?? "") + (payload.GivenName ?? "");
                var email = payload.Email;
                var token = usuario.IdToken;

                var user = await UserManager.FindByEmailAsync(email);

                if (user != null)
                {
                    var roles = await UserManager.GetRolesAsync(user.Id);
                    var rolUsuario = roles.FirstOrDefault();

                    if (rolUsuario == null)
                    {
                        return Ok(new AutenticacionRespuesta
                        {
                            Correo = email,
                            Token = token,
                            EstaAutorizado = EstatusAutorizacion.PENDIENTE,
                            RequiereDatosAdicionales = RequiereDatosAdicionales.REQUERIDO
                        });
                    }

                    if (rolUsuario == "Docente")
                    {
                        var docente = Db.tbDocentes.FirstOrDefault(a => a.UserId == user.Id);
                        if (docente == null) return BadRequest();

                        AutenticacionRespuesta respuesta;

                        if (docente.estaAutorizado == null)
                        {
                            respuesta = new AutenticacionRespuesta
                            {
                                Id = docente.DocenteId,
                                Correo = email,
                                Token = token,
                                EstaAutorizado = EstatusAutorizacion.PENDIENTE,
                                RequiereDatosAdicionales = RequiereDatosAdicionales.NO_REQUERIDO
                            };
                        }
                        else if (docente.estaAutorizado == false)
                        {
                            respuesta = new AutenticacionRespuesta
                            {
                                EstaAutorizado = EstatusAutorizacion.DENEGADO
                            };
                        }
                        else
                        {
                            respuesta = new AutenticacionRespuesta
                            {
                                Id = docente.DocenteId,
                                UserName = userName,
                                Correo = email,
                                Rol = rolUsuario,
                                Token = token,
                                EstaAutorizado = EstatusAutorizacion.AUTORIZADO,
                                RequiereDatosAdicionales = RequiereDatosAdicionales.NO_REQUERIDO
                            };
                        }

                        return Ok(respuesta);
                    }
                    else if (rolUsuario == "Alumno")
                    {
                        var alumno = Db.tbAlumnos.FirstOrDefault(a => a.UserId == user.Id);
                        if (alumno == null) return BadRequest();

                        return Ok(new AutenticacionRespuesta
                        {
                            Id = alumno.AlumnoId,
                            UserName = userName,
                            Correo = email,
                            Rol = rolUsuario,
                            Token = token,
                            EstaAutorizado = EstatusAutorizacion.AUTORIZADO
                        });
                    }
                }

                // CREAR USUARIO NUEVO
                var nuevoUsuario = new ApplicationUser
                {
                    UserName = userName,
                    Email = email
                };

                var result = await UserManager.CreateAsync(nuevoUsuario);

                if (!result.Succeeded)
                {
                    return BadRequest("Error al crear el usuario");
                }

                return Ok(new AutenticacionRespuesta
                {
                    Correo = email,
                    Token = token,
                    EstaAutorizado = EstatusAutorizacion.PENDIENTE,
                    RequiereDatosAdicionales = RequiereDatosAdicionales.REQUERIDO
                });
            }
            catch (Exception ex)
            {
                // Puedes registrar el error en logs si lo deseas
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("RegistrarDatosFaltantesGoogle")]
        public async Task<IHttpActionResult> RegistrarDatosFaltantesGoogle([FromBody] DatosFaltantesGoogle datos)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                int idUsuario = 0;
                string nombres = datos.Nombres;
                string apellidoPaterno = datos.ApellidoPaterno;
                string apellidoMaterno = datos.ApellidoMaterno;
                string role = datos.Role;
                string token = datos.IdToken;

                var payload = await GoogleJsonWebSignature.ValidateAsync(datos.IdToken);
                if (payload == null)
                    return BadRequest("idToken inválido");

                string email = payload.Email;
                string userName = payload.GivenName;

                var user = await UserManager.FindByEmailAsync(email);
                if (user == null)
                    return BadRequest("Usuario no encontrado");

                // CREAR ROL SI NO EXISTE
                if (!await RoleManager.RoleExistsAsync(role))
                {
                    await RoleManager.CreateAsync(new IdentityRole(role));
                }

                // ASIGNAR ROL
                var tieneRol = await UserManager.IsInRoleAsync(user.Id, role);
                if (!tieneRol)
                {
                    var asignarRol = await UserManager.AddToRoleAsync(user.Id, role);
                    if (!asignarRol.Succeeded)
                        return BadRequest(string.Join(", ", asignarRol.Errors));
                }

                // === SI ES DOCENTE ===
                if (role == "Docente")
                {
                    var fcmToken = datos.FcmToken;
                    if (string.IsNullOrEmpty(fcmToken))
                        return BadRequest("FcmToken requerido");

                    string identityUserId = user.Id;
                    DateTime fechaExpiracionCodigo = DateTime.UtcNow.AddMinutes(59);
                    string codigo = Fg.GenerarCodigoAleatorio();

                    var docente = new tbDocentes
                    {
                        Nombre = nombres,
                        ApellidoPaterno = apellidoPaterno,
                        ApellidoMaterno = apellidoMaterno,
                        UserId = identityUserId,
                        CodigoAutorizacion = codigo,
                        FechaExpiracionCodigo = fechaExpiracionCodigo
                    };

                    Db.tbDocentes.Add(docente);
                    await Db.SaveChangesAsync();

                    await Ns.RegistrarFcmTokenUsuario(identityUserId, fcmToken);

                    return Ok(new
                    {
                        estaAutorizado = EstatusAutorizacion.PENDIENTE,
                        requiereDatosAdicionales = RequiereDatosAdicionales.NO_REQUERIDO
                    });
                }

                // === SI ES ALUMNO ===
                else if (role == "Alumno")
                {
                    var fcmToken = datos.FcmToken;
                    if (string.IsNullOrEmpty(fcmToken))
                        return BadRequest("FcmToken requerido");

                    string identityUserId = user.Id;

                    var alumno = new tbAlumnos
                    {
                        Nombre = nombres,
                        ApellidoPaterno = apellidoPaterno,
                        ApellidoMaterno = apellidoMaterno,
                        UserId = identityUserId
                    };

                    Db.tbAlumnos.Add(alumno);
                    await Db.SaveChangesAsync();

                    idUsuario = alumno.AlumnoId;

                    await Ns.RegistrarFcmTokenUsuario(identityUserId, fcmToken);

                    var emailEncontrado = await UserManager.FindByIdAsync(identityUserId);
                    if (emailEncontrado == null)
                        return BadRequest("Usuario no encontrado tras registro");

                    return Ok(new AutenticacionRespuesta
                    {
                        Id = idUsuario,
                        UserName = userName,
                        Correo = email,
                        Rol = role,
                        Token = token,
                        EstaAutorizado = EstatusAutorizacion.AUTORIZADO
                    });
                }

                return BadRequest("Rol no válido");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost]
        [Route("ValidarCodigoDocenteGoogle")]
        public async Task<IHttpActionResult> ValidarCodigoAutorizacionDocente([FromBody] ValidarCodigoDocente datos)
        {
            try
            {
                string email = datos.Email;
                string codigoValidar = datos.CodigoValidar;
                string token = datos.IdToken ?? "";

                var emailEncontrado = await UserManager.FindByEmailAsync(email);
                if (emailEncontrado == null)
                {
                    return BadRequest(new
                    {
                        ErrorCode = ErrorCodigos.CredencialesInvalidas,
                        ErrorMessage = Errores.GetMensajeError(ErrorCodigos.CredencialesInvalidas)
                    }.ToString());
                }

                // Obtener rol del usuario
                var roles = await UserManager.GetRolesAsync(emailEncontrado.Id);
                var rolUsuario = roles.FirstOrDefault();
                if (rolUsuario == null)
                    throw new Exception("El usuario no posee un rol asignado");

                string identityUserId = emailEncontrado.Id;

                var docente = Db.tbDocentes.FirstOrDefault(a => a.UserId == identityUserId);
                if (docente != null)
                {
                    int idUsuario = docente.DocenteId;

                    // Validar código
                    if (codigoValidar != docente.CodigoAutorizacion)
                    {
                        return BadRequest(new
                        {
                            ErrorCode = ErrorCodigos.codigoAutorizacionInvalido
                        }.ToString());
                    }

                    // Validar expiración
                    if (docente.FechaExpiracionCodigo < DateTime.Now)
                    {
                        return BadRequest(new
                        {
                            ErrorCode = ErrorCodigos.codigoAutorizacionExpirado
                        }.ToString());
                    }

                    // Actualizar estado de autorización
                    docente.FechaExpiracionCodigo = null;
                    docente.CodigoAutorizacion = null;
                    docente.estaAutorizado = true;
                    Db.SaveChanges();

                    var respuesta = new AutenticacionRespuesta
                    {
                        Id = idUsuario,
                        UserName = emailEncontrado.UserName,
                        Correo = emailEncontrado.Email,
                        Rol = rolUsuario,
                        Token = token,
                        EstaAutorizado = EstatusAutorizacion.AUTORIZADO
                    };

                    return Ok(respuesta);
                }

                return BadRequest("El docente no existe.");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpPost]
        [Route("VerificarIdToken")]
        public async Task<IHttpActionResult> VerificarGoogleIdToken([FromBody] VerificarGoogleIdToken token)
        {
            try
            {
                var idToken = token.IdToken;

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
                if (payload != null && payload.ExpirationTimeSeconds > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    string name = payload.Name;
                    string email = payload.Email;

                    var user = await UserManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        return BadRequest("Usuario no encontrado");
                    }

                    int idUsuario = 0;
                    string identityUserId = user.Id;
                    var roles = await UserManager.GetRolesAsync(identityUserId);
                    var rolUsuario = roles.FirstOrDefault();
                    if (rolUsuario == null)
                        throw new Exception("El usuario no posee un rol asignado");

                    if (rolUsuario == "Docente")
                    {
                        idUsuario = Db.tbDocentes
                            .Where(a => a.UserId == identityUserId)
                            .Select(a => a.DocenteId)
                            .FirstOrDefault();
                    }
                    else if (rolUsuario == "Alumno")
                    {
                        idUsuario = Db.tbAlumnos
                            .Where(a => a.UserId == identityUserId)
                            .Select(a => a.AlumnoId)
                            .FirstOrDefault();
                    }

                    var respuesta = new AutenticacionRespuesta
                    {
                        Id = idUsuario,
                        UserName = name,
                        Correo = email,
                        Rol = rolUsuario,
                        Token = idToken
                    };

                    return Ok(respuesta);
                }

                return Content(HttpStatusCode.BadRequest, new { mensaje = "El token no es válido" });
            }
            catch (Exception)
            {
                return Content(HttpStatusCode.BadRequest, new { mensaje = "El token no es válido" });
            }
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
                    _db?.Dispose();
                    _db = null;
                }
            }

            base.Dispose(disposing);
        }
    }
}
