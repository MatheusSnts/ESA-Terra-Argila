using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using Microsoft.EntityFrameworkCore;

namespace ESA_Terra_Argila.Services
{
    public class AdminDashboardService
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;
            var dayAgo = now.AddDays(-1);
            var weekAgo = now.AddDays(-7);
            var monthAgo = now.AddMonths(-1);
            var yearAgo = now.AddYears(-1);

            //Queries
            var usersQuery = _context.Users.AsNoTracking();
            var productsQuery = _context.Items.OfType<Product>().AsNoTracking();
            var materialsQuery = _context.Items.OfType<Material>().AsNoTracking();
            var ordersQuery = _context.Orders.AsNoTracking();
            var activitiesQuery = _context.UserActivities.AsNoTracking();

            //Preencher o modelo com dados reais
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await usersQuery.CountAsync(),
                ApprovedUsers = await usersQuery.CountAsync(u => u.AcceptedByAdmin),

                TotalProducts = await productsQuery.CountAsync(),
                TotalMaterials = await materialsQuery.CountAsync(),
                TotalOrders = await ordersQuery.CountAsync(),

                UniqueUsers24h = await activitiesQuery.Where(a => a.Timestamp >= dayAgo).Select(a => a.UserId).Distinct().CountAsync(),
                UniqueUsers7d = await activitiesQuery.Where(a => a.Timestamp >= weekAgo).Select(a => a.UserId).Distinct().CountAsync(),
                UniqueUsersMonth = await activitiesQuery.Where(a => a.Timestamp >= monthAgo).Select(a => a.UserId).Distinct().CountAsync(),
                UniqueUsersYear = await activitiesQuery.Where(a => a.Timestamp >= yearAgo).Select(a => a.UserId).Distinct().CountAsync(),
                UniqueUsersTotal = await activitiesQuery.Select(a => a.UserId).Distinct().CountAsync(),

                ActiveUsers24h = await activitiesQuery.CountAsync(a => a.Timestamp >= dayAgo),
                ActiveUsers7d = await activitiesQuery.CountAsync(a => a.Timestamp >= weekAgo),
                ActiveUsersMonth = await activitiesQuery.CountAsync(a => a.Timestamp >= monthAgo),
                ActiveUsersYear = await activitiesQuery.CountAsync(a => a.Timestamp >= yearAgo),
                ActiveUsersTotal = await activitiesQuery.CountAsync()
            };

            return model;
        }
    }
}
