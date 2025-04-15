using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controller responsável pela página inicial e funcionalidades básicas do site.
    /// Gerencia a exibição da página inicial, política de privacidade e tratamento de erros.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private string? userId;
        private readonly UserManager<User> _userManager;

        /// <summary>
        /// Inicializa uma nova instância do controller da página inicial.
        /// </summary>
        /// <param name="context">Contexto do banco de dados da aplicação.</param>
        /// <param name="userManager">Gerenciador de usuários do Identity.</param>
        /// <param name="logger">Serviço de log da aplicação.</param>
        public HomeController(ApplicationDbContext context, UserManager<User> userManager, ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Exibe a página inicial do site com os 4 produtos mais vendidos.
        /// </summary>
        /// <returns>View com os produtos mais populares.</returns>
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
        /// Exibe a página de política de privacidade do site.
        /// </summary>
        /// <returns>View da política de privacidade.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Exibe a página de erro quando ocorre uma exceção no sistema.
        /// </summary>
        /// <returns>View de erro com informações sobre a requisição que falhou.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Adicionar um método temporário para testes
        [Authorize]
        public async Task<IActionResult> TestLockout()
        {
            var user = await _userManager.GetUserAsync(User);
            
            if (user != null)
            {
                // Verifique se o usuário está bloqueado
                if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
                {
                    ViewBag.LockoutStatus = "Usuário ESTÁ bloqueado até " + user.LockoutEnd.Value.ToString();
                }
                else
                {
                    ViewBag.LockoutStatus = "Usuário NÃO está bloqueado";
                }

                ViewBag.UserDetails = new {
                    UserName = user.UserName,
                    Email = user.Email,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd
                };
            }
            else
            {
                ViewBag.LockoutStatus = "Usuário não encontrado";
            }

            return View();
        }
    }
}
