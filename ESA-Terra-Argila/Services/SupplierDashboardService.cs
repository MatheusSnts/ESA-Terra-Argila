
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            
            var materialsQuery = _context.Items
                .OfType<Material>()
                .Where(m => m.UserId == user.Id);

            var materialIds = await materialsQuery.Select(m => m.Id).ToListAsync();

            
            var totalMaterials = materialIds.Count;


            var totalSales = await _context.OrderItems
            .Where(oi => oi.ItemId.HasValue && materialIds.Contains(oi.ItemId.Value))
            .SumAsync(oi => (int?)oi.Quantity) ?? 0;


            var mostFavorited = await _context.UserMaterialFavorites
                .Where(f => materialIds.Contains(f.MaterialId))
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


            

            var bestSelling = await _context.OrderItems
                .Where(oi => oi.ItemId.HasValue && materialIds.Contains(oi.ItemId.Value))
                .GroupBy(oi => oi.ItemId.Value)
                .OrderByDescending(g => g.Sum(oi => oi.Quantity))
                .Select(g => new { MaterialId = g.Key, Quantity = g.Sum(oi => oi.Quantity) })
                .FirstOrDefaultAsync();


            var bestSellingName = bestSelling != null
                ? await _context.Items.OfType<Material>()
                    .Where(m => m.Id == bestSelling.MaterialId)
                    .Select(m => m.Name)
                    .FirstOrDefaultAsync()
                : "None";

           
            var totalRevenue = await _context.Payments
                .Where(p => p.Order.OrderItems.Any(oi => oi.Item.UserId == user.Id))
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            
            return new SupplierDashboardViewModel
            {
                SupplierName = user?.UserName,
                TotalMaterials = totalMaterials,
                TotalSales = totalSales,
                MostFavoritedMaterial = mostFavoritedName,
                MostFavoritedCount = mostFavorited?.Count ?? 0,
                BestSellingMaterial = bestSellingName,
                BestSellingQuantity = (int)(bestSelling?.Quantity ?? 0),
                TotalRevenue = totalRevenue
            };
        }
    }
}

