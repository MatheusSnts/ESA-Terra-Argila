using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Collections.Generic;

namespace ESA_Terra_Argila.Controllers
{
    [Route("api/pagamento")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        [HttpPost("checkout")]
        public IActionResult CriarSessaoPagamento()
        {
            var domain = "https://localhost:7197/";

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
                            UnitAmount = 500, 
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Produto Teste"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{domain}/sucesso?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/cancelado"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Ok(new { sessionId = session.Id, url = session.Url });
        }
    }
}
