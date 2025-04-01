using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using ESA_Terra_Argila.Models;
using Microsoft.Extensions.Options;

namespace ESA_Terra_Argila.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly EmailConfiguration _emailConfig;

        public EmailSender(IConfiguration configuration, IOptions<EmailConfiguration> emailConfig)
        {
            _configuration = configuration;
            _emailConfig = emailConfig.Value ?? throw new ArgumentNullException(nameof(emailConfig));
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            using (var client = new SmtpClient(_emailConfig.Host, _emailConfig.Port))
            {
                client.Credentials = new NetworkCredential(_emailConfig.UserName, _emailConfig.Password);
                client.EnableSsl = _emailConfig.EnableSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailConfig.SenderEmail, _emailConfig.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
            }
        }


        public async Task SendConfirmationLinkAsync(User user, string confirmationLink, string token)
        {
            var subject = "Confirmação de e-mail";
            var message = $@"
                <p>Olá {user.UserName},</p>
                <p>Por favor, confirme seu e-mail clicando no link abaixo:</p>
                <p><a href='{confirmationLink}?token={token}'>Confirmar E-mail</a></p>
            ";

            await SendEmailAsync(user.Email, subject, message);
        }

        public async Task SendPasswordResetLinkAsync(User user, string resetLink, string token)
        {
            var subject = "Redefinição de senha";
            var message = $@"
                <p>Olá {user.UserName},</p>
                <p>Para redefinir sua senha, clique no link abaixo:</p>
                <p><a href='{resetLink}?token={token}'>Redefinir Senha</a></p>
            ";

            await SendEmailAsync(user.Email, subject, message);
        }

        public async Task SendEmailChangeConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Confirmação de Alteração de E-mail";
            var message = $@"
        <p>Olá,</p>
        <p>Para confirmar a alteração do seu e-mail, clique no link abaixo:</p>
        <p><a href='{confirmationLink}'>Confirmar Alteração de E-mail</a></p>
    ";
            Console.WriteLine("Email de confirmação de alteração de e-mail enviado com sucesso.");
            await SendEmailAsync(email, subject, message);
            
        }


        public async Task SendPasswordResetCodeAsync(User user, string resetCode, string provider)
        {
            var subject = "Código de redefinição de senha";
            var message = $@"
                <p>Olá {user.UserName},</p>
                <p>Use este código para redefinir sua senha: <strong>{resetCode}</strong></p>
                <p>Provedor: {provider}</p>
            ";

            await SendEmailAsync(user.Email, subject, message);
        }
    }
}
