using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ESA_Terra_Argila.Controllers
{
    [Authorize(Roles = "Supplier")]
    public class SupplierDashboardController : Controller
    {
        private readonly SupplierDashboardService _dashboardService;
        private readonly UserManager<User> _userManager;

        public SupplierDashboardController(SupplierDashboardService dashboardService, UserManager<User> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var viewModel = await _dashboardService.GetDashboardDataAsync(user);
            return View(viewModel);
        }

    }
}
