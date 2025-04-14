using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ESA_Terra_Argila.Controllers
{
    [Authorize(Roles = "Vendor")]
    public class VendorDashboardController : Controller
    {
        private readonly VendorDashboardService _dashboardService;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public VendorDashboardController(
            VendorDashboardService dashboardService,
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
            var user = await _userManager.GetUserAsync(User);
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
                .Where(oi => oi.Order.UserId == user.Id
                    && (
                        (range == "24h" && oi.Order.CreatedAt >= start)
                        || (range != "24h" && oi.Order.CreatedAt.Date >= start.Date)
                    )
                )
                .ToListAsync();
            var intervals = new decimal[count];
            foreach (var oi in orderItems)
            {
                var val = (decimal)(oi.Item.Price * oi.Quantity);
                if (range == "24h")
                {
                    var index = (int)(oi.Order.CreatedAt - start).TotalHours;
                    if (index >= 0 && index < count) intervals[index] += val;
                }
                else
                {
                    var index = (oi.Order.CreatedAt.Date - start.Date).Days;
                    if (index >= 0 && index < count) intervals[index] += val;
                }
            }
            return Json(intervals);
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesData(string range)
        {
            var user = await _userManager.GetUserAsync(User);
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
                .Where(oi => oi.Order.UserId == user.Id
                    && (
                        (range == "24h" && oi.Order.CreatedAt >= start)
                        || (range != "24h" && oi.Order.CreatedAt.Date >= start.Date)
                    )
                )
                .ToListAsync();
            var intervals = new float[count];
            foreach (var oi in orderItems)
            {
                if (range == "24h")
                {
                    var index = (int)(oi.Order.CreatedAt - start).TotalHours;
                    if (index >= 0 && index < count) intervals[index] += oi.Quantity;
                }
                else
                {
                    var index = (oi.Order.CreatedAt.Date - start.Date).Days;
                    if (index >= 0 && index < count) intervals[index] += oi.Quantity;
                }
            }
            return Json(intervals);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsData(string range)
        {
            var user = await _userManager.GetUserAsync(User);
            var totalProducts = await _context.Items
                .OfType<Product>()
                .Where(p => p.UserId == user.Id)
                .CountAsync();
            int count = (range == "24h") ? 24 : 7;
            var arr = Enumerable.Repeat((float)totalProducts, count).ToList();
            return Json(arr);
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlySalesData(string range)
        {
            var user = await _userManager.GetUserAsync(User);
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
                .Where(oi => oi.Order.UserId == user.Id
                    && (
                        (range == "24h" && oi.Order.CreatedAt >= start)
                        || (range != "24h" && oi.Order.CreatedAt.Date >= start.Date)
                    )
                )
                .ToListAsync();
            var intervals = new float[count];
            foreach (var oi in orderItems)
            {
                var val = (float)oi.Quantity;
                if (range == "24h")
                {
                    var index = (int)(oi.Order.CreatedAt - start).TotalHours;
                    if (index >= 0 && index < count) intervals[index] += val;
                }
                else
                {
                    var index = (oi.Order.CreatedAt.Date - start.Date).Days;
                    if (index >= 0 && index < count) intervals[index] += val;
                }
            }
            return Json(intervals);
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartmentSalesData(string range)
        {
            var user = await _userManager.GetUserAsync(User);
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
                .Where(oi => oi.Order.UserId == user.Id
                    && (
                        (range == "24h" && oi.Order.CreatedAt >= start)
                        || (range != "24h" && oi.Order.CreatedAt.Date >= start.Date)
                    )
                )
                .ToListAsync();
            var grouped = orderItems
                .Where(oi => oi.Item != null)
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
