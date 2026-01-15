using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
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
                var fromConf = Environment.GetEnvironmentVariable("SMTP_EMAIL");
                var serverConf = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "smtp.gmail.com";
                var portConf = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "465"; //465 para SSL, 587 TLS
                var passwordConf = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

                if (serverConf == null || portConf == null)
                {
                    throw new Exception("Hubo un error en el envio de correo");
                }

                var emailGenerado = new MimeMessage();
                emailGenerado.From.Add(MailboxAddress.Parse(fromConf));
                emailGenerado.To.Add(MailboxAddress.Parse(email));
                emailGenerado.Subject = subject; //TODO: Preparar el titulo del correo
                emailGenerado.Body = new TextPart(TextFormat.Html) { 
                    Text = htmlMessage
                }; //TODO: poner codigo para cambio de password

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(serverConf, int.Parse(portConf), SecureSocketOptions.Auto);
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