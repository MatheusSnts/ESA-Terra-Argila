using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using ESA_Terra_Argila.Models;

namespace ESA_Terra_Argila.Services
{
    public interface IEmailService
    {
        Task SendOrderConfirmationEmailAsync(string to, string orderNumber, decimal totalAmount, List<OrderItem> items);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
            _smtpServer = _configuration["EmailSettings:Host"];
            _smtpPort = int.Parse(_configuration["EmailSettings:Port"]);
            _smtpUsername = _configuration["EmailSettings:UserName"];
            _smtpPassword = _configuration["EmailSettings:Password"];
            _fromEmail = _configuration["EmailSettings:UserName"];
            _fromName = "ESA Terra Argila";
        }

        public async Task SendOrderConfirmationEmailAsync(string to, string orderNumber, decimal totalAmount, List<OrderItem> items)
        {
            using var client = new SmtpClient(_smtpServer, _smtpPort)
            {
                EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSSL"]),
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            var message = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = $"Confirmação de Pedido #{orderNumber} - ESA Terra Argila",
                Body = GenerateOrderEmailBody(orderNumber, totalAmount, items),
                IsBodyHtml = true
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
        }

        private string GenerateOrderEmailBody(string orderNumber, decimal totalAmount, List<OrderItem> items)
        {
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
                        th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
                        th {{ background-color: #f8f9fa; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Confirmação de Pedido</h1>
                            <p>Pedido #{orderNumber}</p>
                        </div>
                        
                        <div class='content'>
                            <p>Olá,</p>
                            <p>Seu pedido foi recebido com sucesso! Abaixo estão os detalhes do seu pedido:</p>
                            
                            <table>
                                <tr>
                                    <th>Produto</th>
                                    <th>Quantidade</th>
                                    <th>Preço Unitário</th>
                                    <th>Total</th>
                                </tr>";

            foreach (var item in items)
            {
                body += $@"
                    <tr>
                        <td>{item.Item.Name}</td>
                        <td>{item.Quantity}</td>
                        <td>R$ {item.Item.Price:F2}</td>
                        <td>R$ {item.GetTotal():F2}</td>
                    </tr>";
            }

            body += $@"
                            </table>
                            
                            <p style='text-align: right; font-weight: bold;'>
                                Total do Pedido: R$ {totalAmount:F2}
                            </p>
                            
                            <p>Agradecemos sua preferência!</p>
                        </div>
                        
                        <div class='footer'>
                            <p>Este é um email automático, por favor não responda.</p>
                        </div>
                    </div>
                </body>
                </html>";

            return body;
        }
    }
} 