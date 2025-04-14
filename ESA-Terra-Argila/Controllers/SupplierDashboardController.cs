using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ESA_Terra_Argila.Controllers
{
    [Authorize(Roles = "Supplier")]
    public class SupplierDashboardController : Controller
    {
        private readonly SupplierDashboardService _dashboardService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public SupplierDashboardController(
            SupplierDashboardService dashboardService,
            UserManager<User> userManager,
            ApplicationDbContext context)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
            _context = context;
        }

        
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var viewModel = await _dashboardService.GetDashboardDataAsync(user);
            return View(viewModel);
        }


        [HttpGet]
        public async Task<IActionResult> GetRevenueData7d()
        {
            var now = DateTime.UtcNow.Date;
            var start = now.AddDays(-6);

            
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Item)
                .Where(oi => oi.Order.CreatedAt >= start)
                .ToListAsync();

            
            var materialItems = orderItems.Where(oi => oi.Item is Material);

            
            var dailyTotals = new Dictionary<DateTime, decimal>();
            for (int i = 0; i < 7; i++)
            {
                dailyTotals[start.AddDays(i)] = 0m;
            }


            foreach (var oi in materialItems)
            {
                var day = oi.Order.CreatedAt.Date;
                if (dailyTotals.ContainsKey(day))
                {
                    decimal itemValue = (decimal)(oi.Item.Price * oi.Quantity);
                    dailyTotals[day] += itemValue;
                }
            }

            
            var result = dailyTotals
                .OrderBy(x => x.Key)
                .Select(x => x.Value)
                .ToList();

            return Json(result); 
        }


        [HttpGet]
        public async Task<IActionResult> GetSalesData7d()
        {
            var now = DateTime.UtcNow.Date;
            var start = now.AddDays(-6);

            
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Item)
                .Where(oi => oi.Order.CreatedAt >= start)
                .ToListAsync();

            
            var materialItems = orderItems.Where(oi => oi.Item is Material);

            var dailyTotals = new Dictionary<DateTime, float>();
            for (int i = 0; i < 7; i++)
            {
                dailyTotals[start.AddDays(i)] = 0f;
            }

            foreach (var oi in materialItems)
            {
                var day = oi.Order.CreatedAt.Date;
                if (dailyTotals.ContainsKey(day))
                {
                    dailyTotals[day] += oi.Quantity;
                }
            }

            var result = dailyTotals.OrderBy(x => x.Key).Select(x => x.Value).ToList();
            return Json(result);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetMaterialsData7d()
        {
            
            var totalMaterials = await _context.Items
                .OfType<Material>()
                .CountAsync();

            
            var arr = Enumerable.Repeat((float)totalMaterials, 7).ToList();
            return Json(arr);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetMonthlySales7d()
        {
            var now = DateTime.UtcNow.Date;
            var start = now.AddDays(-6);

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Item)
                .Where(oi => oi.Order.CreatedAt >= start)
                .ToListAsync();

            var materialItems = orderItems.Where(oi => oi.Item is Material);

            var dailyTotals = new Dictionary<DateTime, float>();
            for (int i = 0; i < 7; i++)
            {
                dailyTotals[start.AddDays(i)] = 0f;
            }

            foreach (var oi in materialItems)
            {
                var day = oi.Order.CreatedAt.Date;
                if (dailyTotals.ContainsKey(day))
                {
                    dailyTotals[day] += oi.Quantity;
                }
            }

            var result = dailyTotals.OrderBy(x => x.Key).Select(x => x.Value).ToList();
            return Json(result);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetDepartmentSales7d()
        {
            var now = DateTime.UtcNow.Date;
            var start = now.AddDays(-6);

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.CreatedAt >= start)
                .ToListAsync();

            var materialItems = orderItems.Where(oi => oi.Item is Material);

            
            var grouped = materialItems
                .GroupBy(oi => oi.Item.Name)
                .Select(g => new {
                    label = g.Key,
                    total = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.total)
                .ToList();

            return Json(grouped);
        }
    }
}
