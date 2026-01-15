using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using System.Threading.Tasks;

public class NotificacionesHub : Hub
{
    public override Task OnConnected()
    {
        var userId = Context.User.Identity.Name;
        Debug.WriteLine("Usuario conectado a SignalR: " + userId);

        return base.OnConnected();
    }
}