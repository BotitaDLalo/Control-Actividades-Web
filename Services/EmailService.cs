using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

namespace ControlActividades.Services
{
    public class EmailService
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var fromConf = "";
                var serverConf = "";
                var portConf = "";
                var passwordConf = "";
             
                if (serverConf == null || portConf == null)
                {
                    throw new Exception("Hubo un error en el envio de correo");
                }

                var emailGenerado = new MimeMessage();
                emailGenerado.From.Add(MailboxAddress.Parse(fromConf));
                emailGenerado.To.Add(MailboxAddress.Parse(email));
                emailGenerado.Subject = subject; //TODO: Preparar el titulo del correo
                emailGenerado.Body = new TextPart(TextFormat.Html) { Text = htmlMessage }; //TODO: poner codigo para cambio de password

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(serverConf, int.Parse(portConf), false);
                    smtp.Authenticate(fromConf, passwordConf);
                    smtp.Send(emailGenerado);
                    smtp.Disconnect(true);
                };

                return Task.CompletedTask;
            }
            catch (Exception)
            {
                throw new Exception("Hubo un error en el envio de correo");
            }
        }
    }
}