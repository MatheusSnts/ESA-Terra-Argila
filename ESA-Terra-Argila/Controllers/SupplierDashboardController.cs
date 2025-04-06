using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
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
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public SupplierDashboardController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var materiais = _context.Items
                .OfType<Material>()
                .Where(m => m.UserId == user.Id);

            var totalMateriais = await materiais.CountAsync();
            var stockTotal = await materiais.SumAsync(m => m.Stock);

            var materialMaisFavorito = await _context.UserMaterialFavorites
                .Where(f => materiais.Select(m => m.Id).Contains(f.MaterialId))
                .GroupBy(f => f.MaterialId)
                .OrderByDescending(g => g.Count())
                .Select(g => new
                {
                    MaterialId = g.Key,
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            var materialFavoritoNome = materialMaisFavorito != null
                ? await _context.Items.OfType<Material>()
                    .Where(m => m.Id == materialMaisFavorito.MaterialId)
                    .Select(m => m.Name)
                    .FirstOrDefaultAsync()
                : "Nenhum";

            var materialMaisVendido = await _context.StockMovements
                .Where(sm => sm.Type == "Venda" && materiais.Select(m => m.Id).Contains(sm.MaterialId))
                .GroupBy(sm => sm.MaterialId)
                .OrderByDescending(g => g.Sum(s => s.Quantity))
                .Select(g => new
                {
                    MaterialId = g.Key,
                    Total = g.Sum(s => s.Quantity)
                })
                .FirstOrDefaultAsync();

            var materialVendidoNome = materialMaisVendido != null
                ? await _context.Items.OfType<Material>()
                    .Where(m => m.Id == materialMaisVendido.MaterialId)
                    .Select(m => m.Name)
                    .FirstOrDefaultAsync()
                : "Nenhum";

            var viewModel = new SupplierDashboardViewModel
            {
                NomeFornecedor = user?.UserName,
                TotalMateriais = totalMateriais,
                StockTotal = stockTotal,
                MaterialMaisFavorito = materialFavoritoNome,
                FavoritosDoMaisPopular = materialMaisFavorito?.Count ?? 0,
                MaterialMaisVendido = materialVendidoNome,
                QuantidadeVendida = materialMaisVendido?.Total ?? 0
            };

            return View(viewModel);
        }
    }
}
