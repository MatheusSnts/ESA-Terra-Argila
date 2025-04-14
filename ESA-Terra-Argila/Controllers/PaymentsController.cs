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

namespace ESA_Terra_Argila.Controllers
{
    [Route("api/pagamento")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;


        public PaymentController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        [HttpPost("checkout/{orderId}")]
        public async Task<IActionResult> CriarSessaoPagamento(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || !order.OrderItems.Any())
            {
                return NotFound(new { message = "Pedido não encontrado ou sem itens." });
            }

            try
            {
                var domain = $"{Request.Scheme}://{Request.Host}";
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

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = lineItems,
                    Mode = "payment",
                  
                    SuccessUrl = $"{domain}/PaymentSuccess?orderId={order.Id}",

                    CancelUrl = $"{domain}"
                };

                var service = new SessionService();
                var session = service.Create(options);

                return Redirect(session.Url);
            }
            catch (System.Exception ex)
            {
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

            // Atualiza o status do pedido para Delivered (entregue/finalizado)
            order.Status = Enums.OrderStatus.Delivered;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Verificar se já existe um carrinho vazio para o usuário
            var existingCart = await _context.Orders
                .FirstOrDefaultAsync(o => o.UserId == order.UserId && o.Status == Enums.OrderStatus.Draft);

            // Só cria um novo carrinho se não existir um
            if (existingCart == null)
            {
                var newCart = new Order
                {
                    UserId = order.UserId,
                    Status = Enums.OrderStatus.Draft,
                    OrderItems = new List<OrderItem>(),
                    CreatedAt = DateTime.UtcNow // Garantir que a data seja recente
                };
                _context.Orders.Add(newCart);
                await _context.SaveChangesAsync();
            }

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