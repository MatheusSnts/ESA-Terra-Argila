using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe.Checkout;
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using System.Collections.Generic;
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

        [HttpPost("checkout/{productId}")]
        public IActionResult CriarSessaoPagamento(int productId)
        {
            var product = _context.Items.OfType<Product>().FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                return NotFound(new { message = "Produto não encontrado." });
            }

            var domain = "https://localhost:7197";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
        {
            new()
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "eur",
                    UnitAmount = (long)(product.Price * 100), // Stripe usa centavos
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = product.Name
                    }
                },
                Quantity = 1
            }
        },
                Mode = "payment",
                SuccessUrl = $"{domain}/sucesso",
                CancelUrl = $"{domain}/cancelado"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new { sessionId = session.Id, url = session.Url });
        }

    }
}
