
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
            /*
            var totalRevenue = await _context.Items
                .OfType<Product>()
                .Where(p => p.UserId == user.Id)
                .SumAsync(p => p.Revenue);
            */
            /*
            var totalRevenue = await _context.Orders
                .Where(o => o.UserId == user.Id)
                .SumAsync(o => o.TotalAmount);
            */
            /*
            var bestSelling = await _context.Items
                            .OfType<Product>()
                            .Where(p => p.UserId == user.Id)
                            .OrderByDescending(p => p.TotalSold)
                            .Select(p => new { p.Name, p.TotalSold })
                            .FirstOrDefaultAsync();
            */

            return new VendorDashboardViewModel
            {
                VendorName = user?.UserName,
                TotalProducts = totalProducts,
                TotalStock = 0,
                TotalFavorites = totalFavorites,
                TotalRevenue = 0,
                BestSellingProduct = /*bestSelling?.Name ??*/ "None",
                BestSellingQuantity = 0//bestSelling?.TotalSold ?? 0
            };
        }
    }
}

