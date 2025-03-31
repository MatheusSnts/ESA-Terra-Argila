using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Localization;
using System.Net;

namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controlador responsável pela página inicial e funcionalidades gerais do site.
    /// Gerencia a navegação principal e funções básicas como configuração de cultura.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Inicializa uma nova instância do controlador Home.
        /// </summary>
        /// <param name="logger">O serviço de logging para registrar eventos.</param>
        /// <param name="context">O contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">O gerenciador de usuários para autenticação.</param>
        public HomeController(ApplicationDbContext context, UserManager<User> userManager, ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Exibe a página inicial do site.
        /// </summary>
        /// <returns>A view da página inicial.</returns>
        public IActionResult Index()
        {
            var topProductIds = _context.OrderItems
                .Where(oi => oi.Item is Product)
                .GroupBy(oi => oi.ItemId)
                .Select(g => new
                {
                    ItemId = g.Key.Value,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .Select(x => x.ItemId)
                .ToList();

            var top4Products = _context.Items
                .OfType<Product>()
                .Where(p => topProductIds.Contains(p.Id))
                .Include(p => p.Images)
                .Include(p => p.User)
                .ToList();

            return View(top4Products);
        }

        /// <summary>
        /// Exibe a página de política de privacidade.
        /// </summary>
        /// <returns>A view da política de privacidade.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Exibe a página de erro quando ocorre uma exceção não tratada.
        /// </summary>
        /// <returns>A view de erro com detalhes da exceção se em ambiente de desenvolvimento.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Define a cultura (idioma) selecionada pelo usuário.
        /// </summary>
        /// <param name="culture">O código da cultura a ser definida.</param>
        /// <param name="returnUrl">A URL para retornar após a configuração.</param>
        /// <returns>Redireciona para a URL de retorno.</returns>
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}
