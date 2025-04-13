
using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.AspNetCore.Identity;
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
            var totalProducts = await _context.Items
                .OfType<Product>()
                .CountAsync(p => p.UserId == user.Id);

            var totalFavorites = await _context.UserMaterialFavorites
                .CountAsync(f => f.UserId == user.Id);

            var bestSelling = await _context.Items
                .OfType<Product>()
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.Stock) 
                .Select(p => new { p.Name, Quantity = 0 })
                .FirstOrDefaultAsync();

            var totalRevenue = await _context.Payments
                .Where(p => p.Order.OrderItems.Any(oi => oi.Item.UserId == user.Id))
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return new VendorDashboardViewModel
            {
                VendorName = user?.UserName,
                TotalProducts = totalProducts,
                TotalFavorites = totalFavorites,
                TotalRevenue = totalRevenue,
                BestSellingProduct = bestSelling?.Name ?? "None",
                BestSellingQuantity = bestSelling?.Quantity ?? 0
            };
        }
    }
}

