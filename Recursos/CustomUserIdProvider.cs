using Microsoft.AspNet.SignalR;
using System.Security.Claims;
using Microsoft.AspNet.SignalR.Infrastructure;
using System.Linq;

namespace ControlActividades.Recursos
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(IRequest request)
        {
            var identity = request.User.Identity as ClaimsIdentity;

            // Aquí tomamos el ID real del usuario (AspNetUsers.Id)
            var userIdClaim = identity?.FindFirst(ClaimTypes.NameIdentifier);

            return userIdClaim?.Value;
        }
    }
}