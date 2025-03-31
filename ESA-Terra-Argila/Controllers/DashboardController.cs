using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controlador responsável por gerenciar o painel de controle (dashboard) do sistema.
    /// Redireciona usuários para diferentes áreas baseado em suas funções.
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        /// <summary>
        /// Redireciona o usuário para a área apropriada baseado em sua função.
        /// </summary>
        /// <returns>
        /// - Vendedores são redirecionados para a lista de produtos
        /// - Fornecedores são redirecionados para a lista de materiais
        /// - Administradores são redirecionados para a aprovação de usuários
        /// - Outros usuários são redirecionados para a página inicial
        /// </returns>
        public IActionResult Item()
        {
            if (User.IsInRole("Vendor"))
            {
                return RedirectToRoute(new { controller = "Products", action = "Index" });
            }
            else if (User.IsInRole("Supplier"))
            {
                return RedirectToRoute(new { controller = "Materials", action = "Index" });
            }
            else if (User.IsInRole("Admin"))
            {
                return RedirectToRoute(new { controller = "Admin", action = "AcceptUsers" });
            }
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        /// <summary>
        /// Exibe a página principal do dashboard.
        /// </summary>
        /// <returns>A view principal do dashboard.</returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}
