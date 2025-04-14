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
        public async Task<IActionResult> GetRevenueData(string range)
        {
            
            var now = DateTime.UtcNow;
            DateTime start;
            int count;

            if (range == "24h")
            {
                start = now.AddHours(-23);
                count = 24;
            }
            else
            {
                start = now.Date.AddDays(-6);
                count = 7;
            }

           
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Item)
                .Where(oi => range == "24h"
                    ? oi.Order.CreatedAt >= start
                    : oi.Order.CreatedAt.Date >= start.Date 
                )
                .ToListAsync();

            var materialItems = orderItems.Where(oi => oi.Item is Material);

           
            var dailyValues = new decimal[count];

           
            foreach (var oi in materialItems)
            {
                decimal val = (decimal)(oi.Item.Price * oi.Quantity);
                if (range == "24h")
                {
                    
                    var index = (int)(oi.Order.CreatedAt - start).TotalHours;
                    if (index >= 0 && index < 24)
                    {
                        dailyValues[index] += val;
                    }
                }
                else
                {
                    
                    var index = (oi.Order.CreatedAt.Date - start.Date).Days;
                    if (index >= 0 && index < 7)
                    {
                        dailyValues[index] += val;
                    }
                }
            }

            
            return Json(dailyValues);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetSalesData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            int count;

            if (range == "24h")
            {
                start = now.AddHours(-23);
                count = 24;
            }
            else
            {
                start = now.Date.AddDays(-6);
                count = 7;
            }

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Item)
                .Where(oi => range == "24h"
                    ? oi.Order.CreatedAt >= start
                    : oi.Order.CreatedAt.Date >= start.Date
                )
                .ToListAsync();

            var materialItems = orderItems.Where(oi => oi.Item is Material);

            var dailyValues = new float[count];

            foreach (var oi in materialItems)
            {
                if (range == "24h")
                {
                    var index = (int)(oi.Order.CreatedAt - start).TotalHours;
                    if (index >= 0 && index < 24)
                    {
                        dailyValues[index] += oi.Quantity;
                    }
                }
                else
                {
                    var index = (oi.Order.CreatedAt.Date - start.Date).Days;
                    if (index >= 0 && index < 7)
                    {
                        dailyValues[index] += oi.Quantity;
                    }
                }
            }

            return Json(dailyValues);
        }

       
        [HttpGet]
        public async Task<IActionResult> GetMaterialsData(string range)
        {
            
            var totalMaterials = await _context.Items
                .OfType<Material>()
                .CountAsync();

           
            int count = (range == "24h") ? 24 : 7;

            var arr = Enumerable.Repeat((float)totalMaterials, count).ToList();
            return Json(arr);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetMonthlySalesData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            int count;

            if (range == "24h")
            {
                start = now.AddHours(-23);
                count = 24;
            }
            else
            {
                start = now.Date.AddDays(-6);
                count = 7;
            }

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Item)
                .Where(oi => range == "24h"
                    ? oi.Order.CreatedAt >= start
                    : oi.Order.CreatedAt.Date >= start.Date
                )
                .ToListAsync();

            var materialItems = orderItems.Where(oi => oi.Item is Material);

            var dailyValues = new float[count];

            foreach (var oi in materialItems)
            {
                if (range == "24h")
                {
                    var index = (int)(oi.Order.CreatedAt - start).TotalHours;
                    if (index >= 0 && index < 24)
                    {
                        dailyValues[index] += oi.Quantity;
                    }
                }
                else
                {
                    var index = (oi.Order.CreatedAt.Date - start.Date).Days;
                    if (index >= 0 && index < 7)
                    {
                        dailyValues[index] += oi.Quantity;
                    }
                }
            }

            return Json(dailyValues);
        }

       
        [HttpGet]
        public async Task<IActionResult> GetDepartmentSalesData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;

            if (range == "24h")
            {
                start = now.AddHours(-23);
            }
            else
            {
                start = now.Date.AddDays(-6);
            }

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.Order)
                .Where(oi => range == "24h"
                    ? oi.Order.CreatedAt >= start
                    : oi.Order.CreatedAt.Date >= start.Date
                )
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
