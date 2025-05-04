using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;

namespace ESA_Terra_Argila.Pages
{
    public class PaymentSuccessModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;


        public PaymentSuccessModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(int orderId)
        {
            var domain = $"{Request.Scheme}://{Request.Host}";
            var successUrl = $"{domain}/PaymentSuccess?orderId={orderId}";
            Console.WriteLine($"[DEBUG] Received orderId: {orderId}");

            if (orderId <= 0)
            {
                return RedirectToPage("/Error");
            }

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync($"{domain}/api/pagamento/record/{orderId}", null);

            if (!response.IsSuccessStatusCode)
            {
                // log or handle error here
                return RedirectToPage("/Error");
            }

            return Page();
        }
    }
}
