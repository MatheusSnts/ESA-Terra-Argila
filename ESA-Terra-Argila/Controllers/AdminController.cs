using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Helpers;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

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
            var users = _context.Users.Where(u => !u.AcceptedByAdmin);
            var usersList = await users.ToListAsync();
            foreach (var user in usersList) 
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.Role = UserRoleHelper.GetUserRoleFromString(roles.FirstOrDefault());
            }
            return View(usersList);
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
            var user = await _context.Users.FindAsync(id);
            user.AcceptedByAdmin = true;
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador aprovado com sucesso!";
            return RedirectToAction(nameof(AcceptUsers));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            //delete user by id
            var user = await _context.Users.FindAsync(id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador removido com sucesso";
            return RedirectToAction(nameof(AcceptUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            user.AcceptedByAdmin = false;
            _context.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Utilizador bloqueado com sucesso!";
            var referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(AcceptUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteVendor(string id)
        {
            // TODO: not implemented yet
            return RedirectToAction(nameof(AcceptUsers));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InviteSupplier(string id)
        {
            // TODO: not implemented yet
            return RedirectToAction(nameof(AcceptUsers));

        }

        private async Task<List<User>> GetUsersByRole(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            return users.Where(u => u.AcceptedByAdmin).ToList();
        }


        [HttpGet]
        public async Task<IActionResult> GetActivityData(string range)
        {
            var now = DateTime.UtcNow;
            DateTime start;
            string format;
            Func<DateTime, DateTime> step;

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
                    step = dt => dt.AddYears(1);
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
                    var label = $"{i:00}h";
                    result.Add(new { label, count = raw.ContainsKey(label) ? raw[label] : 0 });
                }
            }
            else
            {
                var current = start;
                while (current <= now)
                {
                    var label = current.ToString(format);
                    result.Add(new { label, count = raw.ContainsKey(label) ? raw[label] : 0 });
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


    }
}
