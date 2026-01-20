using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ControlActividades.Models;
using ControlActividades.Models.db;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.IdentityModel.Tokens;

namespace ControlActividades.Recursos
{
    public class FuncionalidadesGenerales
    {
        private ApplicationDbContext _db;

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
        public string GenerarCodigoAleatorio()
        {
            int length = 5;
            const string chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #region JWT
        public string GenerarJwt(int idUsuario, IdentityUser emailEncontrado, string rolUsuario)
        {
            var handler = new JwtSecurityTokenHandler();
            var confSecretKey = "Token para verificar autenticacion del usuario";
            var jwt = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(confSecretKey ?? throw new ArgumentNullException(confSecretKey, "Token no configurado")));
            var credentials = new SigningCredentials(jwt, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "Aprende_Mas",
                Audience = "Aprende_Mas",
                SigningCredentials = credentials,
                Expires = DateTime.UtcNow.AddDays(7),
                Subject = GenerarClaims(idUsuario, emailEncontrado, rolUsuario),
            };

            var token = handler.CreateToken(tokenDescriptor);

            var tokenString = handler.WriteToken(token);

            return tokenString;
        }


        private static ClaimsIdentity GenerarClaims(int idUsuario, IdentityUser usuario, string rol)
        {
            var claims = new ClaimsIdentity();

            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString() ?? ""));
            claims.AddClaim(new Claim(ClaimTypes.Name, usuario.UserName ?? ""));
            claims.AddClaim(new Claim(ClaimTypes.Email, usuario.Email ?? ""));
            claims.AddClaim(new Claim(ClaimTypes.Role, rol ?? ""));

            return claims;
        }

        #endregion

        #region Usuario
        public int ObtenerUsuarioId(IPrincipal User)
        {
            // Determinar el id del usuario consultando la BD en lugar de confiar en las claims/roles
            if (User == null || User.Identity == null || string.IsNullOrEmpty(User.Identity.GetUserId()))
                return 0;

            var userId = User.Identity.GetUserId();

            // Preferir registro en tbDocentes
            var docente = Db.tbDocentes.FirstOrDefault(a => a.UserId == userId);
            if (docente != null)
                return docente.DocenteId;

            // Luego buscar en tbAlumnos
            var alumno = Db.tbAlumnos.FirstOrDefault(a => a.UserId == userId);
            if (alumno != null)
                return alumno.AlumnoId;

            return 0;
        }
        
        public string ObtenerRolUsuario(IPrincipal User)
        {
            if (User == null || User.Identity == null || string.IsNullOrEmpty(User.Identity.GetUserId()))
            {
                // fallback a claims si no hay userId
                var identityFallback = User?.Identity as ClaimsIdentity;
                return identityFallback?.FindFirst(ClaimTypes.Role)?.Value;
            }

            var userId = User.Identity.GetUserId();

            // Verificar en la base de datos para evitar roles inconsistentes en las claims
            if (Db.tbDocentes.Any(d => d.UserId == userId))
                return Roles.DOCENTE;

            if (Db.tbAlumnos.Any(a => a.UserId == userId))
                return Roles.ALUMNO;

            // Fallback a claim si no hay registro en tablas específicas
            var identity = User.Identity as ClaimsIdentity;
            return identity?.FindFirst(ClaimTypes.Role)?.Value;
        }
        #endregion
    }
}