using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using ESA_Terra_Argila.Services;
using Microsoft.AspNetCore.Identity;

namespace ESA_Terra_Argila.Controllers
{
    [Route("api/pagamento")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;


        private readonly UserManager<User> _userManager;

        public PaymentController(ApplicationDbContext context, IEmailSender emailSender, UserManager<User> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager; 
        }

        [HttpPost("checkout/{orderId}")]
        /// <summary>
        /// Cria uma sessão de pagamento para o pedido especificado e redireciona o usuário para a página de checkout do Stripe.
        /// </summary>
        /// <param name="orderId">O identificador do pedido.</param>
        /// <returns>Um IActionResult que redireciona para o checkout ou retorna um erro se algum pré-requisito não for atendido.</returns>
        public async Task<IActionResult> CriarSessaoPagamento(int orderId)
        {
            // Procura o pedido pelo ID, incluindo os itens do pedido e os dados dos itens associados
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            // Verifica se o pedido foi localizado e se possui pelo menos um item
            if (order == null || !order.OrderItems.Any())
            {
                return NotFound(new { message = "Pedido não encontrado ou sem itens." });
            }

            // Recupera o usuário que está fazendo a requisição (usuário autenticado)
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new { message = "Erro: usuário não encontrado ou não autenticado." });
            }

            // Verifica se o e-mail do usuário está confirmado
            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed)
            {
                // Retorna um erro 400 (Bad Request) com uma mensagem de alerta se o e-mail não estiver confirmado
                return new ObjectResult(new { message = "Necessário confirmar email para prosseguir com a compra." })
                { StatusCode = StatusCodes.Status400BadRequest };
            }

            try
            {
                // Constrói o domínio (scheme e host) para compor as URLs de sucesso e cancelamento
                var domain = $"{Request.Scheme}://{Request.Host}";

                // Monta a lista de itens para a sessão de pagamento
                // Para cada item do pedido, configura as opções de preço e dados do produto
                var lineItems = order.OrderItems.Select(oi => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur", 
                        UnitAmount = (long)(oi.Item.Price * 100), 
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = oi.Item.Name 
                        }
                    },
                    Quantity = (long)oi.Quantity 
                }).ToList();

                // Configura as opções para a sessão de pagamento
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" }, 
                    LineItems = lineItems, 
                    Mode = "payment", 
                    SuccessUrl = $"{domain}/PaymentSuccess?orderId={order.Id}", 
                    CancelUrl = $"{domain}" 
                };

                // Cria a sessão de pagamento usando o serviço do Stripe
                var service = new SessionService();
                var session = service.Create(options);

                // Redireciona o usuário para a URL da sessão de pagamento gerada (página de checkout do Stripe)
                return Redirect(session.Url);
            }
            catch (System.Exception ex)
            {
                // Caso ocorra alguma exceção, retorna um erro 400 com detalhes da mensagem de erro
                return BadRequest(new { message = "Erro ao criar sessão de pagamento.", error = ex.Message });
            }
        }

        [HttpPost("record/{orderId}")]
        public async Task<IActionResult> RecordPayment(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.User) // <- Aqui carregamos o usuário
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Pedido não encontrado." });
            }

            var totalAmount = order.GetTotal();

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = totalAmount,
                PaymentDateTime = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            // Geração de fatura em HTML
            var invoiceBody = $@"
        <p>Olá {order.User.FullName},</p>

        <p>Obrigado pela sua compra! Aqui estão os detalhes da sua fatura:</p>

        <p><strong>Pedido #{order.Id}</strong></p>

        <ul>
                    {string.Join("", order.OrderItems.Select(oi =>
                    $"<li>{oi.Item.Name} x{oi.Quantity} = {(oi.Item.Price * oi.Quantity):C2}</li>"))}
        </ul>

         <p><strong>Total:</strong> {totalAmount:C2}</p>
         <p><strong>Data do pagamento:</strong> {payment.PaymentDateTime:dd/MM/yyyy HH:mm}</p>

        <p>Se tiver alguma dúvida, entre em contato conosco.</p>

        <p>Cumprimentos, <br/>ESA Terra Argila</p>";


            // Envio do e-mail
            if (!string.IsNullOrWhiteSpace(order.User.Email))
            {
                await _emailSender.SendEmailAsync(
                    order.User.Email,
                    $"Fatura - Pedido #{order.Id}",
                    invoiceBody
                );
            }

            return Ok(new { message = "Pagamento guardado com sucesso e fatura enviada por e-mail." });
        }

    }
}