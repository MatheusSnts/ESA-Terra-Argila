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

        // Exibe a View com os cartões (VendorDashboardViewModel)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var viewModel = await _dashboardService.GetDashboardDataAsync(user);
            return View(viewModel);
        }

        // 1) Faturação (7 dias) - soma .Amount em Payments, 
        // onde Payment.Order.UserId == user.Id
        [HttpGet]
        public async Task<IActionResult> GetRevenueData7d()
        {
            var user = await _userManager.GetUserAsync(User);
            var now = DateTime.UtcNow.Date;
            var start = now.AddDays(-6);

            // Carrega Payments do período e filtra pelos pedidos deste Vendor
            var payments = await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.PaymentDateTime >= start
                         && p.Order.UserId == user.Id)
                .ToListAsync();

            // Dicionário para 7 dias
            var dailyTotals = new Dictionary<DateTime, decimal>();
            for (int i = 0; i < 7; i++)
            {
                dailyTotals[start.AddDays(i)] = 0m;
            }

            foreach (var pay in payments)
            {
                var day = pay.PaymentDateTime.Date;
                if (dailyTotals.ContainsKey(day))
                {
                    dailyTotals[day] += (decimal)pay.Amount;
                }
            }

            var result = dailyTotals.OrderBy(x => x.Key).Select(x => x.Value).ToList();
            return Json(result);
        }

        // 2) Vendas (quantidade) - soma .Quantity em OrderItems, 
        // onde Order.UserId == user.Id
        [HttpGet]
        public async Task<IActionResult> GetSalesData7d()
        {
            var user = await _userManager.GetUserAsync(User);
            var now = DateTime.UtcNow.Date;
            var start = now.AddDays(-6);

            // Carrega OrderItems do período e filtra pelos pedidos do Vendor
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.CreatedAt >= start
                          && oi.Order.UserId == user.Id)
                .ToListAsync();

            var dailyTotals = new Dictionary<DateTime, float>();
            for (int i = 0; i < 7; i++)
            {
                dailyTotals[start.AddDays(i)] = 0f;
            }

            foreach (var oi in orderItems)
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

        // 3) TotalProdutos (7 dias) - repete o valor atual, pois não há histórico
        [HttpGet]
        public async Task<IActionResult> GetProductsData7d()
        {
            var user = await _userManager.GetUserAsync(User);

            // Quantidade de produtos do vendor
            var totalProducts = await _context.Items
                .OfType<Product>()
                .Where(p => p.UserId == user.Id)
                .CountAsync();

            // Repete esse valor 7 vezes
            var arr = Enumerable.Repeat((float)totalProducts, 7).ToList();
            return Json(arr);
        }

        // 4) "MonthlySales" (7 dias) - soma .Quantity
        // onde Order.UserId == user.Id
        [HttpGet]
        public async Task<IActionResult> GetMonthlySales7d()
        {
            var user = await _userManager.GetUserAsync(User);
            var now = DateTime.UtcNow.Date;
            var start = now.AddDays(-6);

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.CreatedAt >= start
                          && oi.Order.UserId == user.Id)
                .ToListAsync();

            var dailyTotals = new Dictionary<DateTime, float>();
            for (int i = 0; i < 7; i++)
            {
                dailyTotals[start.AddDays(i)] = 0f;
            }

            foreach (var oi in orderItems)
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

        // 5) DepartmentSales7d - agrupa .Item.Name e soma .Quantity
        // Filtra pelos pedidos do Vendor, sem checar Item.UserId
        [HttpGet]
        public async Task<IActionResult> GetDepartmentSales7d()
        {
            var user = await _userManager.GetUserAsync(User);
            var now = DateTime.UtcNow.Date;
            var start = now.AddDays(-6);

            // Carrega OrderItems (com .Item se quiser exibir .Item.Name)
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.CreatedAt >= start
                          && oi.Order.UserId == user.Id)
                .ToListAsync();

            // Agrupa por oi.Item.Name e soma .Quantity
            var grouped = orderItems
                .Where(oi => oi.Item != null)  // caso hajam registros nulos
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
