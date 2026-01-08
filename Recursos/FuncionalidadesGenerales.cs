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
            int usuarioId = 0;
            string userId = User.Identity.GetUserId();

            if (User.IsInRole(Roles.DOCENTE))
            {
                usuarioId = Db.tbDocentes.Where(a => a.UserId == userId).Select(a => a.DocenteId).FirstOrDefault();
            }
            else if (User.IsInRole(Roles.ALUMNO))
            {
                usuarioId = Db.tbAlumnos.Where(a => a.UserId == userId).Select(a => a.AlumnoId).FirstOrDefault();
            }
            return usuarioId;
        }
        
        public string ObtenerRolUsuario(IPrincipal User)
        {
            var identity = User.Identity as ClaimsIdentity;

            var rolClaim = identity?.FindFirst(ClaimTypes.Role);

            return rolClaim?.Value;
        }
        #endregion
    }
}