using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Helpers;
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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly AdminDashboardService _dashboardService;

        public AdminController(ApplicationDbContext context, UserManager<User> userManager, AdminDashboardService dashboardService)
        {
            _context = context;
            _userManager = userManager;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> AcceptUsers()
        {
            var users = _context.Users.Where(u => !u.AcceptedByAdmin && u.DeletedAt == null);
            var list = await users.ToListAsync();
            
            var filteredList = new List<User>();
            
            foreach (var usr in list)
            {
                var roles = await _userManager.GetRolesAsync(usr);
                usr.Role = UserRoleHelper.GetUserRoleFromString(roles.FirstOrDefault());
                
                if (usr.Role != Enums.UserRole.Customer)
                {
                    filteredList.Add(usr);
                }
            }
            
            return View(filteredList);
        }

        public async Task<IActionResult> Dashboard()
        {
            var model = await _dashboardService.GetDashboardStatsAsync();
            return View(model);
        }

        public async Task<IActionResult> Vendors()
        {
            return View(await GetUsersByRole("Vendor"));
        }

        public async Task<IActionResult> Suppliers()
        {
            return View(await GetUsersByRole("Supplier"));
        }

        public async Task<IActionResult> Customers()
        {
            return View(await GetUsersByRole("Customer"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptUser(string id)
        {
            var usr = await _context.Users.FindAsync(id);
            usr.AcceptedByAdmin = true;
            _context.Update(usr);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador aprovado com sucesso!";
            return RedirectToAction(nameof(AcceptUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var usr = await _context.Users.FindAsync(id);
            _context.Users.Remove(usr);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador removido com sucesso";
            return RedirectToAction(nameof(AcceptUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string id)
        {
            var usr = await _context.Users.FindAsync(id);
            usr.AcceptedByAdmin = false;
            _context.Update(usr);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador bloqueado com sucesso!";
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction(nameof(AcceptUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteVendor(string id)
        {
            return RedirectToAction(nameof(AcceptUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteSupplier(string id)
        {
            return RedirectToAction(nameof(AcceptUsers));
        }

        private async Task<List<User>> GetUsersByRole(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.Where(u => u.AcceptedByAdmin && u.DeletedAt == null).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetActivityData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            string format;
            switch (range)
            {
                case "24h":
                    start = now.AddHours(-23);
                    format = "HH\\h";
                    break;
                case "7d":
                    start = now.AddDays(-6);
                    format = "dd/MM";
                    break;
                case "month":
                    start = now.AddMonths(-1);
                    format = "dd/MM";
                    break;
                case "year":
                    start = now.AddYears(-1);
                    format = "MMM yyyy";
                    break;
                case "total":
                    start = now.AddYears(-2);
                    format = "yyyy";
                    break;
                default:
                    start = DateTime.MinValue;
                    format = "yyyy-MM-dd";
                    break;
            }
            var raw = _context.UserActivities
                .Where(a => a.Timestamp >= start)
                .AsEnumerable()
                .GroupBy(a => a.Timestamp.ToString(format))
                .ToDictionary(g => g.Key, g => g.Count());
            var result = new List<object>();
            if (range == "24h")
            {
                for (int i = 0; i < 24; i++)
                {
                    var lbl = $"{i:00}h";
                    result.Add(new { label = lbl, count = raw.ContainsKey(lbl) ? raw[lbl] : 0 });
                }
            }
            else
            {
                var current = start;
                while (current <= now)
                {
                    var lbl = current.ToString(format);
                    result.Add(new { label = lbl, count = raw.ContainsKey(lbl) ? raw[lbl] : 0 });
                    current = range switch
                    {
                        "year" => current.AddMonths(1),
                        "month" => current.AddDays(1),
                        _ => current.AddDays(1)
                    };
                }
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            string format;
            switch (range)
            {
                case "24h":
                    start = now.AddHours(-23);
                    format = "HH\\h";
                    break;
                case "7d":
                    start = now.AddDays(-6);
                    format = "dd/MM";
                    break;
                case "month":
                    start = now.AddMonths(-1);
                    format = "dd/MM";
                    break;
                case "year":
                    start = now.AddYears(-1);
                    format = "MMM yyyy";
                    break;
                default:
                    start = DateTime.UtcNow.AddMonths(-1);
                    format = "dd/MM";
                    break;
            }
            var raw = await _context.Payments
                .Where(p => p.PaymentDateTime >= start)
                .ToListAsync();
            var grouped = raw
                .GroupBy(p => p.PaymentDateTime.ToString(format))
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));
            var result = new List<object>();
            var current = start;
            while (current <= now)
            {
                var lbl = current.ToString(format);
                result.Add(new
                {
                    label = lbl,
                    total = grouped.ContainsKey(lbl) ? grouped[lbl] : 0
                });
                current = range switch
                {
                    "year" => current.AddMonths(1),
                    "month" => current.AddDays(1),
                    "7d" => current.AddDays(1),
                    "24h" => current.AddHours(1),
                    _ => current.AddDays(1)
                };
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            string format;
            switch (range)
            {
                case "24h":
                    start = now.AddHours(-23);
                    format = "HH\\h";
                    break;
                case "7d":
                    start = now.AddDays(-6);
                    format = "dd/MM";
                    break;
                case "month":
                    start = now.AddMonths(-1);
                    format = "dd/MM";
                    break;
                case "year":
                    start = now.AddYears(-1);
                    format = "MMM yyyy";
                    break;
                default:
                    start = DateTime.UtcNow.AddMonths(-1);
                    format = "dd/MM";
                    break;
            }
            var raw = await _context.Users
                .Where(u => u.CreatedAt >= start)
                .ToListAsync();
            var grouped = raw
                .GroupBy(u => u.CreatedAt.ToString(format))
                .ToDictionary(g => g.Key, g => g.Count());
            var result = new List<object>();
            var current = start;
            while (current <= now)
            {
                var lbl = current.ToString(format);
                result.Add(new
                {
                    label = lbl,
                    total = grouped.ContainsKey(lbl) ? grouped[lbl] : 0
                });
                current = range switch
                {
                    "year" => current.AddMonths(1),
                    "month" => current.AddDays(1),
                    "7d" => current.AddDays(1),
                    "24h" => current.AddHours(1),
                    _ => current.AddDays(1)
                };
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            string format;
            switch (range)
            {
                case "24h":
                    start = now.AddHours(-23);
                    format = "HH\\h";
                    break;
                case "7d":
                    start = now.AddDays(-6);
                    format = "dd/MM";
                    break;
                case "month":
                    start = now.AddMonths(-1);
                    format = "dd/MM";
                    break;
                case "year":
                    start = now.AddYears(-1);
                    format = "MMM yyyy";
                    break;
                default:
                    start = DateTime.UtcNow.AddMonths(-1);
                    format = "dd/MM";
                    break;
            }
            var raw = await _context.Items
                .OfType<Product>()
                .Where(i => i.CreatedAt >= start)
                .ToListAsync();
            var grouped = raw
                .GroupBy(i => i.CreatedAt.ToString(format))
                .ToDictionary(g => g.Key, g => g.Count());
            var result = new List<object>();
            var current = start;
            while (current <= now)
            {
                var lbl = current.ToString(format);
                result.Add(new
                {
                    label = lbl,
                    total = grouped.ContainsKey(lbl) ? grouped[lbl] : 0
                });
                current = range switch
                {
                    "year" => current.AddMonths(1),
                    "month" => current.AddDays(1),
                    "7d" => current.AddDays(1),
                    "24h" => current.AddHours(1),
                    _ => current.AddDays(1)
                };
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetMaterialsData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            string format;
            switch (range)
            {
                case "24h":
                    start = now.AddHours(-23);
                    format = "HH\\h";
                    break;
                case "7d":
                    start = now.AddDays(-6);
                    format = "dd/MM";
                    break;
                case "month":
                    start = now.AddMonths(-1);
                    format = "dd/MM";
                    break;
                case "year":
                    start = now.AddYears(-1);
                    format = "MMM yyyy";
                    break;
                default:
                    start = DateTime.UtcNow.AddMonths(-1);
                    format = "dd/MM";
                    break;
            }
            var raw = await _context.Items
                .OfType<Material>()
                .Where(m => m.CreatedAt >= start)
                .ToListAsync();
            var grouped = raw
                .GroupBy(m => m.CreatedAt.ToString(format))
                .ToDictionary(g => g.Key, g => g.Count());
            var result = new List<object>();
            var current = start;
            while (current <= now)
            {
                var lbl = current.ToString(format);
                result.Add(new
                {
                    label = lbl,
                    total = grouped.ContainsKey(lbl) ? grouped[lbl] : 0
                });
                current = range switch
                {
                    "year" => current.AddMonths(1),
                    "month" => current.AddDays(1),
                    "7d" => current.AddDays(1),
                    "24h" => current.AddHours(1),
                    _ => current.AddDays(1)
                };
            }
            return Json(result);
        }

        private async Task<List<User>> GetDeletedUsers()
        {
            return await _context.Users
                .Where(u => u.DeletedAt != null)
                .ToListAsync();
        }
    }
}
