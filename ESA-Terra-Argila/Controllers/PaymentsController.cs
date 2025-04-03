using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESA_Terra_Argila.Controllers
{
    [Route("api/pagamento")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
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
                var domain = "https://localhost:7197";
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
                    SuccessUrl = $"{domain}/PaymentSuccess",
                    CancelUrl = $"{domain}/"
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
    }
}