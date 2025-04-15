
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ESA_Terra_Argila.Services
{
    public class SupplierDashboardService
    {
        private readonly ApplicationDbContext _context;

        public SupplierDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SupplierDashboardViewModel> GetDashboardDataAsync(User user)
        {
            var materials = _context.Items
                .OfType<Material>()
                .Where(m => m.UserId == user.Id);

            var totalMaterials = await materials.CountAsync();
            var totalStock = await materials.SumAsync(m => m.Stock);
            //var totalRevenue = await materials.SumAsync(m => m.Revenue);

            var mostFavorited = await _context.UserMaterialFavorites
                .Where(f => materials.Select(m => m.Id).Contains(f.MaterialId))
                .GroupBy(f => f.MaterialId)
                .OrderByDescending(g => g.Count())
                .Select(g => new { MaterialId = g.Key, Count = g.Count() })
                .FirstOrDefaultAsync();

            var mostFavoritedName = mostFavorited != null
                ? await _context.Items.OfType<Material>()
                    .Where(m => m.Id == mostFavorited.MaterialId)
                    .Select(m => m.Name)
                    .FirstOrDefaultAsync()
                : "None";

            var bestSelling = await _context.StockMovements
                .Where(sm => sm.Type == "Venda" && materials.Select(m => m.Id).Contains(sm.ItemId))
                .GroupBy(sm => sm.ItemId)
                .OrderByDescending(g => g.Sum(s => s.Quantity))
                .Select(g => new { MaterialId = g.Key, Total = g.Sum(s => s.Quantity) })
                .FirstOrDefaultAsync();

            var bestSellingName = bestSelling != null
                ? await _context.Items.OfType<Material>()
                    .Where(m => m.Id == bestSelling.MaterialId)
                    .Select(m => m.Name)
                    .FirstOrDefaultAsync()
                : "None";

            return new SupplierDashboardViewModel
            {
                SupplierName = user?.UserName,
                TotalMaterials = totalMaterials,
                TotalStock = totalStock,
                MostFavoritedMaterial = mostFavoritedName,
                MostFavoritedCount = mostFavorited?.Count ?? 0,
                BestSellingMaterial = bestSellingName,
                BestSellingQuantity = bestSelling?.Total ?? 0,
                TotalRevenue = 0 //totalRevenue
            };
        }
    }
}