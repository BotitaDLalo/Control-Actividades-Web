using ControlActividades.Models;
using ControlActividades.Models.db;
using Google.Apis.Auth;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ControlActividades.Recursos
{
    public class FuncionalidadesGenerales
    {
        public string GenerarCodigoAleatorio()
        {
            int length = 5;
            const string chars = "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string GenerarJwt(int idUsuario, IdentityUser emailEncontrado, string rolUsuario)
        {
            var handler = new JwtSecurityTokenHandler();
            var confSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
            if (string.IsNullOrEmpty(confSecretKey))
            {
                throw new Exception("JwtSecret no configurado en variables de entorno.");
            }

            var jwt = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(confSecretKey ?? throw new ArgumentNullException(confSecretKey, "Token no configurado")));
            var credentials = new SigningCredentials(jwt, SecurityAlgorithms.HmacSha256);

            var issuer = "https://controlactividades20251017143449sx.azurewebsites.net"; // Startup.Auth.cs
            var audience = "https://controlactividades20251017143449sx.azurewebsites.net";

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
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

        public static async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(string idToken)
        {
            try
            {
                var clientId = Environment.GetEnvironmentVariable("GoogleClientId")
                ?? ConfigurationManager.AppSettings["GoogleClientId"];

                // Verifica el token con las claves públicas de Google
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { "GoogleClientId" }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return payload;
            }
            catch
            {
                return null; // token inválido o expirado
            }
        }
    }
}