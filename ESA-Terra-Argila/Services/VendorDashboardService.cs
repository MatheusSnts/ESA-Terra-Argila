using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.EntityFrameworkCore;

namespace ESA_Terra_Argila.Services
{
    public class VendorDashboardService
    {
        private readonly ApplicationDbContext _context;

        public VendorDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VendorDashboardViewModel> GetDashboardDataAsync(User user)
        {
            // Total de produtos do Vendor
            var totalProducts = await _context.Items
                .OfType<Product>()
                .Where(p => p.UserId == user.Id)
                .CountAsync();

            // Total favoritos (exemplo)
            var totalFavorites = await _context.UserMaterialFavorites
                .CountAsync(f => f.UserId == user.Id);

            // Produto mais vendido
            var bestSelling = await _context.OrderItems
                .Where(oi => oi.Item != null && oi.Item.UserId == user.Id)
                .GroupBy(oi => oi.ItemId.Value)
                .Select(g => new {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .FirstOrDefaultAsync();

            var bestSellingProductName = "None";
            var bestSellingQuantity = 0;
            if (bestSelling != null)
            {
                bestSellingProductName = await _context.Items.OfType<Product>()
                    .Where(p => p.Id == bestSelling.ProductId && p.UserId == user.Id)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync() ?? "None";

                bestSellingQuantity = (int)bestSelling.TotalQuantity;
            }

            // TotalSales = soma das quantidades vendidas
            var totalSales = await _context.OrderItems
                .Where(oi => oi.Item != null && oi.Item.UserId == user.Id)
                .SumAsync(oi => (int?)oi.Quantity) ?? 0;

            // Faturação total
            var totalRevenue = await _context.Payments
                .Where(p => p.Order.OrderItems.Any(oi => oi.Item.UserId == user.Id))
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // Retorna o ViewModel
            return new VendorDashboardViewModel
            {
                VendorName = user.UserName,
                TotalProducts = totalProducts,
                TotalFavorites = totalFavorites,
                BestSellingProduct = bestSellingProductName,
                BestSellingQuantity = bestSellingQuantity,
                TotalSales = totalSales,
                TotalRevenue = totalRevenue
            };
        }
    }
}
