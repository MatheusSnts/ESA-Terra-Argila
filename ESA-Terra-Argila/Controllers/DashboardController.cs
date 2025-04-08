using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ESA_Terra_Argila.Controllers
{
    /// <summary>
    /// Controller responsável pelo painel de controle do sistema.
    /// Gerencia o redirecionamento dos usuários para suas respectivas áreas baseado em suas funções.
    /// </summary>
    [Authorize(Policy = "AcceptedByAdmin")]
    public class DashboardController : Controller
    {
        /// <summary>
        /// Redireciona o usuário para sua área específica baseado em sua função no sistema.
        /// Vendedores são direcionados para o seu dashboard, fornecedores para o seu dashboard,
        /// e administradores para o painel administrativo.
        /// </summary>
        /// <returns>Redirecionamento para a área apropriada baseado na função do usuário.</returns>
        public IActionResult Item()
        {
            if (User.IsInRole("Vendor"))
            {
                return RedirectToAction("Index", "VendorDashboard");
            }
            else if (User.IsInRole("Supplier"))
            {
                return RedirectToAction("Index", "SupplierDashboard");
            }
            else if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Exibe a página principal do painel de controle.
        /// </summary>
        /// <returns>A view padrão do dashboard.</returns>
        public IActionResult Index()
        {
            return View();
        }
    }
}

