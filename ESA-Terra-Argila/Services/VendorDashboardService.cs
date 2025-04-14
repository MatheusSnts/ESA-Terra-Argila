
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
                .Where(p => p.UserId == user.Id)
                .CountAsync();

            
            var totalFavorites = await _context.UserMaterialFavorites
                .CountAsync(f => f.UserId == user.Id);

            

            var bestSelling = await _context.OrderItems
                .Where(oi => oi.Item != null && oi.Item.UserId == user.Id)
                .GroupBy(oi => oi.ItemId.Value)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .FirstOrDefaultAsync();

            

            var bestSellingProductName = bestSelling != null
                ? await _context.Items
                    .OfType<Product>()
                    .Where(p => p.Id == bestSelling.ProductId && p.UserId == user.Id)
                    .Select(p => p.Name)
                    .FirstOrDefaultAsync()
                : "None";

            
            var totalSales = await _context.OrderItems
                .Where(oi => oi.Item != null && oi.Item.UserId == user.Id)
                .SumAsync(oi => (int?)oi.Quantity) ?? 0;

            
            var totalRevenue = await _context.Payments
                .Where(p => p.Order.OrderItems.Any(oi => oi.Item.UserId == user.Id))
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            
            return new VendorDashboardViewModel
            {
                VendorName = user?.UserName,
                TotalProducts = totalProducts,
                TotalFavorites = totalFavorites,
                BestSellingProduct = bestSellingProductName,
                BestSellingQuantity = (int)(bestSelling?.TotalQuantity ?? 0),
                TotalSales = totalSales,
                TotalRevenue = totalRevenue
            };
        }
    }
}

