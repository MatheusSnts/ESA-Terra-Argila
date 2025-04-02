using ESA_Terra_Argila.Data;
using ESA_Terra_Argila.Helpers;
using ESA_Terra_Argila.Models;
using ESA_Terra_Argila.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            string groupFormat;

            switch (range)
            {
                case "24h":
                    start = now.AddHours(-23); 
                    groupFormat = "HH\\h";
                    break;
                case "7d":
                    start = now.AddDays(-6); 
                    groupFormat = "dd/MM";
                    break;
                case "month":
                    start = now.AddMonths(-1);
                    groupFormat = "dd/MM";
                    break;
                case "year":
                    start = now.AddYears(-1);
                    groupFormat = "MMM yyyy";
                    break;
                default:
                    start = DateTime.MinValue;
                    groupFormat = "yyyy-MM-dd";
                    break;
            }

            var grouped = _context.UserActivities
                .Where(a => a.Timestamp >= start)
                .AsEnumerable() 
                .GroupBy(a => a.Timestamp.ToLocalTime().ToString(groupFormat))
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    label = g.Key,
                    count = g.Count()
                })
                .ToList(); 

            return Json(grouped);
        }


    }
}
