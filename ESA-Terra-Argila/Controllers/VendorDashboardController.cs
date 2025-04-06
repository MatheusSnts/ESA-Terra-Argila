using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


namespace ESA_Terra_Argila.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public VendorDashboardController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var viewModel = new VendorDashboardViewModel
            {
                TotalProdutos = await _context.Items
                .OfType<Product>()
                .CountAsync(p => p.UserId == user.Id),
                TotalFavoritos = await _context.UserMaterialFavorites
                                    .CountAsync(f => f.UserId == user.Id)
            };

            return View(viewModel);
        }

    }
}

